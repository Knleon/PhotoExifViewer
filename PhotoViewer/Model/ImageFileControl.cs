using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace PhotoViewer.Model
{
    public static class ImageFileControl
    {  
        /// <summary>
        /// ObservableCollectionにフォルダ内に存在する画像ファイルのリストを保存するメソッド
        /// </summary>
        /// <return>選択したフォルダパスを返す</return>
        static public string OpenDirectory()
        {
            string _selectedFolderPath = "";

            var dialog = new CommonOpenFileDialog();

            // フォルダ選択ダイアログの設定
            const string FolderDialogTitle = "フォルダ選択ダイアログ";
            dialog.Title = FolderDialogTitle;
            dialog.EnsureReadOnly = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.IsFolderPicker = true;
            dialog.DefaultDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonPictures);

            // フォルダ選択ダイアログの表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _selectedFolderPath = dialog.FileName;
            }
            
            return _selectedFolderPath;
        }

        /// <summary>
        /// 画像を開くメソッド
        /// </summary>
        /// <param name="_filePath">画像ファイルのパス</param>
        public static BitmapSource CreateViewImage(string _filePath)
        {
            using (MemoryStream _stream = new MemoryStream(File.ReadAllBytes(_filePath)))
            {
                BitmapSource _bitmapSource = null;

                // Not Raw image case
                if (Path.GetExtension(_filePath).ToLower() != ".nef" && Path.GetExtension(_filePath).ToLower() != ".dng")
                {
                    var _bitmapImage = new BitmapImage();
                    _bitmapImage.BeginInit();
                    _bitmapImage.StreamSource = _stream;
                    _bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    _bitmapImage.EndInit();

                    // 画像からメタデータを取得する
                    _stream.Position = 0;
                    var _metaData = (BitmapFrame.Create(_stream).Metadata) as BitmapMetadata;
                    _stream.Close();

                    // 画像を回転する
                    _bitmapSource = RotateBitmapSource(_metaData, _bitmapImage);
                }
                else
                {
                    // Raw Image case
                    BitmapDecoder _bmpDecoder = BitmapDecoder.Create(_stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand);
                    _bitmapSource = _bmpDecoder.Frames[0];
                }

                // 表示する画像を生成する
                const uint _maxContentsWidth = 880;
                const uint _maxContentsHeight = 660;
                _bitmapSource = CreateResizeImage(_bitmapSource, _maxContentsWidth, _maxContentsHeight);
                _bitmapSource.Freeze();

                return _bitmapSource;
            }
        }

        /// <summary>
        /// 画像を回転するメソッド
        /// </summary>
        private static BitmapSource TransformBitmap(BitmapSource source, Transform transform)
        {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            return result;
        }

        /// <summary>
        /// サムネイル画像を作成するメソッド
        /// </summary>
        public static BitmapSource CreateThumnailImage(string _filePath)
        {
            using (MemoryStream _stream = new MemoryStream(File.ReadAllBytes(_filePath)))
            {
                // 画像オブジェクトの作成
                var _frame = BitmapFrame.Create(_stream);
                var _thumbnailSource = _frame.Thumbnail;
                var _metaData = (_frame.Metadata) as BitmapMetadata;

                // サムネイルがない場合
                if (_thumbnailSource == null)
                {
                    _thumbnailSource = _frame.Clone();
                }

                // Rawファイル以外はサムネイル画像を回転する
                if (Path.GetExtension(_filePath).ToLower() != ".nef" && Path.GetExtension(_filePath).ToLower() != ".dng")
                {
                    _thumbnailSource = RotateBitmapSource(_metaData, _thumbnailSource);
                }

                // サムネイル画像を生成する(100x75以上のものはこのサイズに収まるように縮小)
                const uint _maxContentsWidth = 100;
                const uint _maxContentsHeight = 75;
                _thumbnailSource = CreateResizeImage(_thumbnailSource, _maxContentsWidth, _maxContentsHeight);
                _thumbnailSource.Freeze();

                return _thumbnailSource;
            }
        }

        /// <summary>
        /// BitmapSourceをOrientationに合わせて回転するメソッド
        /// </summary>
        private static BitmapSource RotateBitmapSource(BitmapMetadata _metaData, BitmapSource _bitmapSource)
        {
            string query = "/app1/ifd/exif:{uint=274}";
            if (!_metaData.ContainsQuery(query))
            {
                return _bitmapSource;
            }

            switch (Convert.ToUInt32(_metaData.GetQuery(query)))
            {
                case 1:
                    return _bitmapSource;
                case 3:
                    return TransformBitmap(_bitmapSource, new RotateTransform(180));
                case 6:
                    return TransformBitmap(_bitmapSource, new RotateTransform(90));
                case 8:
                    return TransformBitmap(_bitmapSource, new RotateTransform(270));
                case 2:
                    return TransformBitmap(_bitmapSource, new ScaleTransform(-1, 1, 0, 0));
                case 4:
                    return TransformBitmap(_bitmapSource, new ScaleTransform(1, -1, 0, 0));
                case 5:
                    return TransformBitmap(TransformBitmap(_bitmapSource, new RotateTransform(90)), new ScaleTransform(-1, 1, 0, 0));
                case 7:
                    return TransformBitmap(TransformBitmap(_bitmapSource, new RotateTransform(270)), new ScaleTransform(-1, 1, 0, 0));
            }
            return _bitmapSource;
        }

        /// <summary>
        /// 画像を保存するメソッド
        /// </summary>
        public static bool SaveImageFile(Bitmap _bitmap, string _path)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonPictures);
            sfd.Filter = "画像ファイル|*.jpg; *.png; *.bmp; *.gif; *.tiff";
            sfd.FileName = Path.GetFileName(_path);
            sfd.FilterIndex = 1;
            sfd.Title = "保存先のファイルを選択してください";
            sfd.RestoreDirectory = true;
            sfd.OverwritePrompt = true;
            sfd.CheckPathExists = true;

            //ダイアログを表示する
            if (sfd.ShowDialog() == true)
            {
                string _filePath = sfd.FileName;
                string _extension = Path.GetExtension(_filePath).ToLower();

                // Bitmap画像を保存する
                SaveBitmapImage(_bitmap, _extension, _filePath);
                
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ビットマップ画像を保存する
        /// </summary>
        /// <param name="_bitmap">保存する画像</param>
        /// <param name="_extension">保存するファイル拡張子</param>
        /// <param name="_filePath">保存するファイルパス</param>
        private static void SaveBitmapImage(Bitmap _bitmap, string _extension,  string _filePath)
        {
            if (_extension == ".jpg")
            {
                // Jpegファイルで保存する場合
                Bitmap _saveBitmap = new Bitmap(_bitmap);

                // Qualityの設定
                const long _quality = 90;
                System.Drawing.Imaging.EncoderParameters _eps = new System.Drawing.Imaging.EncoderParameters(1);
                System.Drawing.Imaging.EncoderParameter _ep = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, _quality);
                _eps.Param[0] = _ep;

                // エンコーダに関する情報を取得
                System.Drawing.Imaging.ImageCodecInfo ici = GetEncoderInfo(System.Drawing.Imaging.ImageFormat.Jpeg);

                _saveBitmap.Save(_filePath, ici, _eps);

                // クリア
                _saveBitmap.Dispose();
                _eps.Dispose();
            }
            else if (_extension == ".png")
            {
                // Pngファイルで保存する場合
                Bitmap _saveBitmap = new Bitmap(_bitmap);
                _saveBitmap.Save(_filePath, System.Drawing.Imaging.ImageFormat.Png);

                // クリア
                _saveBitmap.Dispose();
            }
            else if (_extension == ".bmp")
            {
                // Bmpファイルで保存する場合
                Bitmap _saveBitmap = new Bitmap(_bitmap);
                _saveBitmap.Save(_filePath, System.Drawing.Imaging.ImageFormat.Bmp);

                // クリア
                _saveBitmap.Dispose();
            }
            else if (_extension == ".gif")
            {
                // Gifファイルで保存する場合
                Bitmap _saveBitmap = new Bitmap(_bitmap);
                _saveBitmap.Save(_filePath, System.Drawing.Imaging.ImageFormat.Gif);

                // クリア
                _saveBitmap.Dispose();
            }
            else
            {
                // Tiffファイルで保存する場合
                Bitmap _saveBitmap = new Bitmap(_bitmap);
                _saveBitmap.Save(_filePath, System.Drawing.Imaging.ImageFormat.Tiff);

                // クリア
                _saveBitmap.Dispose();
            }
        }

        /// <summary>
        /// ImageFormatで指定されたImageCodecInfoを探して返す
        /// </summary>
        private static System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(System.Drawing.Imaging.ImageFormat f)
        {
            System.Drawing.Imaging.ImageCodecInfo[] encs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            foreach (System.Drawing.Imaging.ImageCodecInfo enc in encs)
            {
                if (enc.FormatID == f.Guid)
                {
                    return enc;
                }
            }
            return null;
        }

        /// <summary>
        /// アスペクト比を維持して最大のサイズに合うようにリサイズする幅と高さを算出するメソッド
        /// </summary>
        /// <param name="_source">Bitmap画像</param>
        /// <param name="_maxWidth">最大幅</param>
        /// <param name="_maxHeight">最大高さ</param>
        /// <param name="_resizeWidth">リサイズ後の幅</param>
        /// <param name="_resizeHeight">リサイズ後の高さ</param>
        public static void GetAspectScale(BitmapSource _source, int _maxWidth, int _maxHeight, ref int _resizeWidth, ref int _resizeHeight)
        {
            if ((double)_maxWidth / _maxHeight > (double)_source.PixelWidth / _source.PixelHeight)
            {
                // 画像が縦長なら縦方向いっぱいに表示するようにリサイズ
                _resizeHeight = _maxHeight;
                _resizeWidth = _resizeHeight * _source.PixelWidth / _source.PixelHeight;
            }
            else
            {
                // 画像が横長なら横方向いっぱいに表示するようにリサイズ
                _resizeWidth = _maxWidth;
                _resizeHeight = _resizeWidth * _source.PixelHeight / _source.PixelWidth;
            }
        }

        /// <summary>
        /// Bitmap画像の画像サイズをチェックするメソッド
        /// </summary>
        /// <param name="_source">Bitmap画像</param>
        /// <param name="_maxWidth">最大幅</param>
        /// <param name="_maxHeight">最大高さ</param>
        /// <returns>最大幅、最大高さよりBitmap画像のサイズが大きい場合はFalseを返す</returns>
        public static bool CheckPictureSize(BitmapSource _source, uint _maxWidth, uint _maxHeight)
        {
            uint _sourceWidth = (uint)_source.PixelWidth;
            uint _sourceHeigth = (uint)_source.PixelHeight;

            bool _isSmaller = true;

            if(_sourceWidth > _maxWidth || _sourceHeigth > _maxHeight)
            {
                _isSmaller = false;
            }

            return _isSmaller;
        }

        /// <summary>
        /// BitmapImageをリサイズするメソッド
        /// </summary>
        /// <param name="_source">Bitmapで読み込んだ画像情報</param>
        /// <param name="_maxWidth">リサイズする最大幅</param>
        /// <param name="_maxHeight">リサイズする最大高さ</param>
        /// <returns>リサイズ後のBitmapImageを返す</returns>
        private static BitmapSource ResizeImage(BitmapSource _source, int _maxWidth, int _maxHeight)
        {
            // 縮小率を求める
            double _scaleX = (double)_maxWidth / _source.PixelWidth;
            double _scaleY = (double)_maxHeight / _source.PixelHeight;
            double _scale = Math.Min(_scaleX, _scaleY);

            // 縮小されたBitmapを作成
            BitmapSource _resizeImageSource = new WriteableBitmap(new TransformedBitmap(_source, new ScaleTransform(_scale, _scale)));
            _resizeImageSource.Freeze();

            return _resizeImageSource;
        }

        /// <summary>
        /// 画像のファイルパスからViewに対応のResizeした画像を出力するメソッド
        /// </summary>
        /// <param name="_filePath">画像のファイルパス</param>
        /// <returns>作成したBitmap画像のSourceを返す</returns>
        public static BitmapSource CreateResizeImage(BitmapSource _openImage, uint _maxContentsWidth, uint _maxContentsHeight)
        {
            if(! CheckPictureSize(_openImage, _maxContentsWidth, _maxContentsHeight))
            {
                _openImage = ResizeImage(_openImage, (int)_maxContentsWidth, (int)_maxContentsHeight);
            }
            
            return _openImage;
        }

        /// <summary>
        /// MediaInfoからExif情報を削除するメソッド
        /// </summary>
        /// <param name="_info"></param>
        public static bool DeleteExifInfo(string _filePath)
        {
            Bitmap _bitmap = new Bitmap(_filePath);
            return SaveImageFile(_bitmap, _filePath);
        }
    }
}

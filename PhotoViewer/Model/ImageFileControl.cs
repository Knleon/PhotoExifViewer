﻿using Microsoft.WindowsAPICodePack.Dialogs;
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
        static private readonly string FolderDialogTitle = "フォルダ選択ダイアログ";

        /// <summary>
        /// ObservableCollectionにフォルダ内に存在する画像ファイルのリストを保存するメソッド
        /// </summary>
        /// <return>選択したフォルダパスを返す</return>
        static public string OpenDirectory()
        {
            string _selectedFolderPath = "";
            var dialog = new CommonOpenFileDialog();
            dialog.Title = FolderDialogTitle;
            dialog.EnsureReadOnly = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.IsFolderPicker = true;
            dialog.DefaultDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonPictures);

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
        public static BitmapSource OpenImageFile(MediaInfo _info)
        {
            string _filePath = _info.FilePath;
            using (WrappingStream _stream = new WrappingStream(new MemoryStream(File.ReadAllBytes(_filePath))))
            {
                var _frame = BitmapFrame.Create(_stream);
                BitmapSource _openSource = new WriteableBitmap((BitmapFrame)_frame.Clone());
                var _metaData = (_frame.Metadata) as BitmapMetadata;

                if (Path.GetExtension(_filePath) == ".NEF" || Path.GetExtension(_filePath) == ".DNG")
                {
                    return _openSource;
                }
                else
                {
                    return RotateBitmapSource(_metaData, _openSource);
                }
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
            using (WrappingStream _stream = new WrappingStream(new MemoryStream(File.ReadAllBytes(_filePath))))
            {
                // 画像オブジェクトの作成
                var _frame = BitmapFrame.Create(_stream);
                var _thumbnailSource = _frame.Thumbnail;
                var _metaData = (_frame.Metadata) as BitmapMetadata;

                // サムネイルがない場合
                if (_thumbnailSource == null)
                {
                    _thumbnailSource = new WriteableBitmap(_frame.Clone());
                }

                // Rawファイル以外はサムネイル画像を回転する
                if (Path.GetExtension(_filePath) != ".NEF" && Path.GetExtension(_filePath) != ".DNG")
                {
                    _thumbnailSource = RotateBitmapSource(_metaData, _thumbnailSource); ;
                }

                // サムネイル画像をリサイズ(100x75以上のものはこのサイズに収まるように縮小)
                const uint _maxContentsWidth = 100;
                const uint _maxContentsHeight = 75;
                _thumbnailSource = CreateResizeImage(_thumbnailSource, _maxContentsWidth, _maxContentsHeight);

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
                string _extension = Path.GetExtension(_filePath);
                
                if(_extension == ".jpg")
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
                else if(_extension == ".png")
                {
                    // Pngファイルで保存する場合
                    Bitmap _saveBitmap = new Bitmap(_bitmap);
                    _saveBitmap.Save(_filePath, System.Drawing.Imaging.ImageFormat.Png);

                    // クリア
                    _saveBitmap.Dispose();
                }
                else if(_extension == ".bmp")
                {
                    // Bmpファイルで保存する場合
                    Bitmap _saveBitmap = new Bitmap(_bitmap);
                    _saveBitmap.Save(_filePath, System.Drawing.Imaging.ImageFormat.Bmp);

                    // クリア
                    _saveBitmap.Dispose();
                }
                else if(_extension == ".gif")
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
                return true;
            }
            else
            {
                return false;
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

            if(_sourceWidth > _maxWidth || _sourceHeigth > _maxHeight)
            {
                return false;
            }
            else
            {
                return true;
            }
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
            int _resizeWidth = 0;
            int _resizeHeight = 0;

            // アスペクト比を維持したリサイズサイズを求める
            GetAspectScale(_source, _maxWidth, _maxHeight, ref _resizeWidth, ref _resizeHeight);

            var _rect = new Rect(0, 0, _resizeWidth, _resizeHeight);

            var _group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(_group, BitmapScalingMode.HighQuality);
            _group.Children.Add(new ImageDrawing(_source, _rect));

            var _drawingVisual = new DrawingVisual();
            using (var _drawingContext = _drawingVisual.RenderOpen())
            {
                _drawingContext.DrawDrawing(_group);
            }

            var _resizedImage = new RenderTargetBitmap(
                _resizeWidth, _resizeHeight,    // Resuzed dimensions
                96, 96,                         // Default DPI Values 
                PixelFormats.Default);          // Default pixel format
            _resizedImage.Render(_drawingVisual);
            BitmapSource _resizeImageSource = BitmapFrame.Create(_resizedImage);

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
        /// ファイル名を取得するメソッド
        /// </summary>
        /// <param name="_info">メディア情報</param>
        /// <returns>ファイル名を返す</returns>
        public static string GetFileName(MediaInfo _info)
        {
            string _filePath = _info.FilePath;
            return Path.GetFileName(_filePath);
        }

        /// <summary>
        /// MediaInfoからExif情報を削除するメソッド
        /// </summary>
        /// <param name="_info"></param>
        public static bool DeleteExifInfo(MediaInfo _info)
        {
            Bitmap _bitmap = new Bitmap(_info.FilePath);
            return SaveImageFile(_bitmap, _info.FilePath);
        }
    }
}

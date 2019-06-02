using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public static class ImageFileControl
    {
        // デフォルトパスの設定
        private static readonly string DEFAULT_PICTURE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);

        /// <summary>
        /// ObservableCollectionにフォルダ内に存在する画像ファイルのリストを保存するメソッド
        /// </summary>
        /// <return>選択したフォルダパスを返す(キャンセルした場合はnullを返す)</return>
        public static string GetFolderInDirectory ()
        {
            string _selectedFolderPath = null;
            var dialog = new CommonOpenFileDialog();

            // フォルダ選択ダイアログの設定
            dialog.Title = "フォルダ選択";
            dialog.IsFolderPicker = true;
            dialog.DefaultDirectory = DEFAULT_PICTURE_PATH;

            // フォルダ選択ダイアログの表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _selectedFolderPath = dialog.FileName;
            }
            
            return _selectedFolderPath;
        }

        /// <summary>
        /// 該当のメディアファイルからExif情報を削除する
        /// </summary>
        /// <param name="_filePath">ファイルパス</param>
        /// <returns></returns>
        public static bool DeleteExifInfoAndSaveFile(string _filePath)
        {
            string _outFilePath = "";
            if (ShowSaveDialog(_filePath, out _outFilePath))
            {
                BitmapSource _bitmapSource = null;

                // 保存ダイアログで保存を押下したとき
                try
                {
                    // 保存したい画像をBitmapSourceで読み込む
                    _bitmapSource = CreateBitmapSourceFromFile(_filePath);

                    // 保存したい画像の読み込み失敗時はエラーとして返す
                    if (_bitmapSource == null)
                    {
                        return false;
                    }

                    // 品質と出力拡張子の設定
                    int _quality = 80;  // デフォルトは標準画質で設定
                    string _outExtension = Path.GetExtension(_outFilePath).ToLower();

                    // Bitmap画像の保存
                    return SaveBitmapImage(_bitmapSource, _outExtension, _outFilePath, _quality);
                }
                catch
                {
                    _bitmapSource = null;

                    // 保存途中のファイルを削除
                    try
                    {
                        if (File.Exists(_outFilePath))
                        {
                            File.Delete(_outFilePath);
                        }
                    }
                    catch { }   // エラー時なので例外は握りつぶす

                    throw new FileLoadException();
                }
                finally
                {
                    // 強制メモリ解放
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }

            // 保存キャンセル時
            return false;
        }

        /// <summary>
        /// 拡大表示する画像を作成する
        /// </summary>
        /// <param name="_filePath">画像ファイルのパス</param>
        /// <returns>読み込みした画像</returns>
        public static BitmapSource CreateViewImage(string _filePath)
        {
            // 読み込む画像が縦長か横長かスクエアか確認(読み込む画像の幅、高さも取得)
            //
            int _sourceImageWidth = 1;  // 読み込む画像の幅
            int _sourceImageHeight = 1; // 読み込む画像の高さ
            MediaContentChecker.PictureType _pictureType = MediaContentChecker.CheckPictureType(_filePath, out _sourceImageWidth, out _sourceImageHeight);

            // 画像の読み込み
            using (var _stream = new WrappingStream(new MemoryStream(File.ReadAllBytes(_filePath))))
            {
                // ファイル拡張子を取得する
                string _extension = Path.GetExtension(_filePath).ToLower();

                // 表示領域のサイズ
                const int _viewWidth = 880;
                const int _viewHeight = 660;

                BitmapSource _bitmapSource = null;

                try
                {
                    if (!MediaContentChecker.CheckRawImageExtensions(_extension))
                    {
                        // Not Raw image case
                        _bitmapSource = ReadNotRawImage(_stream, _pictureType, _sourceImageWidth, _sourceImageHeight, _viewWidth, _viewHeight);
                    }
                    else
                    {
                        // Raw Image case
                        _bitmapSource = ReadRawImage(_stream, _sourceImageWidth, _sourceImageHeight, _viewWidth, _viewHeight);
                    }

                    // BitmapSourceを凍結
                    _bitmapSource.Freeze();

                    // サイズの大きな画像が残っていると、次の画像が読み込みできないので解放する
                    if (_sourceImageWidth * _sourceImageHeight * 3 > 36 * 1024 * 1024)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }

                    return _bitmapSource;
                }
                catch (Exception _ex)
                {
                    App.LogException(_ex);
                    App.ShowErrorMessageBox("画像の読み込みに失敗しました。", "画像読み込みエラー");

                    // 後処理
                    _stream.Close();
                    _bitmapSource = null;

                    return _bitmapSource;
                }
            }
        }

        /// <summary>
        /// Rawではない画像を読み込む
        /// </summary>
        /// <param name="_stream">画像のストリーム</param>
        /// <param name="_pictureType">ピクチャタイプ</param>
        /// <param name="_sourceImageWidth">ソースの幅</param>
        /// <param name="_sourceImageHeight">ソースの高さ</param>
        /// <param name="_viewWidth">表示領域の幅</param>
        /// <param name="_viewHeight">表示領域の高さ</param>
        /// <returns>読み込みした画像</returns>
        private static BitmapSource ReadNotRawImage(Stream _stream, MediaContentChecker.PictureType _pictureType, int _sourceImageWidth, int _sourceImageHeight, int _viewWidth, int _viewHeight)
        {
            // BitmapImageをデコードする(画像作成)
            BitmapSource _bitmapSource = CreateViewImageFromStream(_stream, _pictureType, _sourceImageWidth, _sourceImageHeight, _viewWidth, _viewHeight);

            // 画像からメタデータを取得する
            _stream.Position = 0;
            var _metaData = BitmapFrame.Create(_stream).Metadata as BitmapMetadata;
            _stream.Close();

            // 画像の回転情報を取得(Rotation:5,6,7,8の場合は縦画像)
            uint _rotation = GetRotation(_metaData);

            // 表示領域より大きな画像がある場合(880x660に合うようにリサイズし直す)
            // ただし、縦画像の場合は660x880に合うようにリサイズする
            //
            if (_rotation == 5 || _rotation == 6 || _rotation == 7 || _rotation == 8)
            {
                // 縦画像なので、表示幅、表示高さを入れ替えてリサイズする
                int _tmp = _viewWidth;
                _viewWidth = _viewHeight;
                _viewHeight = _tmp;
            }

            if (!CheckPictureSize(_sourceImageWidth, _sourceImageHeight, _viewWidth, _viewHeight))
            {
                _bitmapSource = CreateResizeImage(_bitmapSource, _viewWidth, _viewHeight);
            }

            // 画像を回転する
            _bitmapSource = RotateBitmapSource(_metaData, _bitmapSource, _viewWidth, _viewHeight);

            return _bitmapSource;
        }

        /// <summary>
        /// Raw画像を読み込む
        /// </summary>
        /// <param name="_stream">画像のストリーム</param>
        /// <param name="_sourceImageWidth">ソースの幅</param>
        /// <param name="_sourceImageHeight">ソースの高さ</param>
        /// <param name="_viewWidth">表示領域の幅</param>
        /// <param name="_viewHeight">表示領域の高さ</param>
        /// <returns>読み込みした画像</returns>
        private static BitmapSource ReadRawImage(Stream _stream, int _sourceImageWidth, int _sourceImageHeight, int _viewWidth, int _viewHeight)
        {
            // Bitmapデコーダで画像を読み込む
            BitmapDecoder _bmpDecoder = BitmapDecoder.Create(_stream, BitmapCreateOptions.None, BitmapCacheOption.None);
            BitmapSource _bitmapSource = _bmpDecoder.Frames[0];

            // 表示領域より大きな画像がある場合(880x660に合うようにリサイズし直す)
            if (!CheckPictureSize(_sourceImageWidth, _sourceImageHeight, _viewWidth, _viewHeight))
            {
                _bitmapSource = CreateResizeImage(_bitmapSource, _viewWidth, _viewHeight);
            }

            return _bitmapSource;
        }

        /// <summary>
        /// ピクチャコンテンツのサムネイル画像を作成する
        /// </summary>
        /// <param name="_filePath">ファイルパス</param>
        /// <returns>ビットマップ画像</returns>
        public static BitmapSource CreatePictureThumnailImage(string _filePath)
        {
            using (var _stream = new WrappingStream(new MemoryStream(File.ReadAllBytes(_filePath))))
            {
                // 画像オブジェクトの作成
                var _bitmapFrame = BitmapFrame.Create(_stream);
                var _thumbnailSource = _bitmapFrame.Thumbnail;
                var _metaData = (_bitmapFrame.Metadata) as BitmapMetadata;

                // サムネイルがない場合
                if (_thumbnailSource == null)
                {
                    _thumbnailSource = _bitmapFrame.Clone();
                }

                // サムネイル画像を生成する(100x75以上のものはこのサイズに収まるように縮小)
                const int _maxContentsWidth = 100;
                const int _maxContentsHeight = 75;
                if (!CheckPictureSize(_thumbnailSource.PixelWidth, _thumbnailSource.PixelHeight, _maxContentsWidth, _maxContentsHeight))
                {
                    _thumbnailSource = CreateResizeImage(_thumbnailSource, _maxContentsWidth, _maxContentsHeight);
                }

                // Rawファイル以外はサムネイル画像を回転する
                string _extension = Path.GetExtension(_filePath).ToLower();
                if (!MediaContentChecker.CheckRawImageExtensions(_extension))
                {
                    _thumbnailSource = RotateBitmapSource(_metaData, _thumbnailSource, _maxContentsWidth, _maxContentsHeight);
                }

                // BitmapSourceを凍結
                _thumbnailSource.Freeze();

                // サイズの大きな画像が残っていると、次の画像が読み込みできないので解放する
                if (_bitmapFrame.PixelWidth * _bitmapFrame.PixelHeight * 3 > 36 * 1024 * 1024)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                return _thumbnailSource;
            }
        }

        /// <summary>
        /// ビデオコンテンツのサムネイル画像を作成する
        /// </summary>
        /// <param name="_filePath">ファイルパス</param>
        /// <returns>ビットマップ画像</returns>
        public static BitmapSource CreateMovieThumbnailImage(string _filePath)
        {
            ShellFile _shellFile = ShellFile.FromFilePath(_filePath);
            BitmapSource _thumbnailSource = _shellFile.Thumbnail.BitmapSource;

            // サムネイル画像を生成する(100x75以上のものはこのサイズに収まるように縮小)
            const int _maxContentsWidth = 100;
            const int _maxContentsHeight = 75;
            if (!CheckPictureSize(_thumbnailSource.PixelWidth, _thumbnailSource.PixelHeight, _maxContentsWidth, _maxContentsHeight))
            {
                _thumbnailSource = CreateResizeImage(_thumbnailSource, _maxContentsWidth, _maxContentsHeight);
            }

            // BitmapSourceを凍結
            _thumbnailSource.Freeze();

            return _thumbnailSource;
        }

        /// <summary>
        /// ファイルからBitmapSourceを取得する
        /// </summary>
        /// <param name="_filePath">ファイルパス</param>
        /// <returns>ファイルから得られたBitmapSource</returns>
        private static BitmapSource CreateBitmapSourceFromFile(string _filePath)
        {
            BitmapSource _bitmapSource = null;

            try
            {
                using (var _stream = new WrappingStream(new MemoryStream(File.ReadAllBytes(_filePath))))
                {
                    string _extension = Path.GetExtension(_filePath).ToLower();
                    if (!MediaContentChecker.CheckRawImageExtensions(_extension))
                    {
                        // Not Raw image case
                        var _bitmapImage = new BitmapImage();
                        _bitmapImage.BeginInit();
                        _bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        _bitmapImage.CreateOptions = BitmapCreateOptions.None;
                        _bitmapImage.StreamSource = _stream;
                        _bitmapImage.EndInit();
                        _bitmapSource = _bitmapImage;
                    }
                    else
                    {
                        // Raw Image case
                        // Bitmapデコーダで画像を読み込む
                        BitmapDecoder _bmpDecoder = BitmapDecoder.Create(_stream, BitmapCreateOptions.None, BitmapCacheOption.None);
                        _bitmapSource = _bmpDecoder.Frames[0];
                        _bitmapSource = new WriteableBitmap(_bitmapSource);
                    }

                    // BitmapSourceを凍結
                    _bitmapSource.Freeze();
                }

                return _bitmapSource;
            }
            catch (Exception)
            {
                // 後始末
                _bitmapSource = null;
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// 表示領域に合ったBitmapImageを作成する
        /// </summary>
        /// <param name="_stream">読み込む画像のストリーム</param>
        /// <param name="_pictureType">読み込む画像のタイプ</param>
        /// <param name="_sourceWidth">読み込む画像の幅</param>
        /// <param name="_sourceHeight">読み込む画像の高さ</param>
        /// <param name="_viewWidth">表示領域の幅</param>
        /// <param name="_viewHeight">表示領域の高さ</param>
        /// <returns></returns>
        private static BitmapImage CreateViewImageFromStream(Stream _stream, MediaContentChecker.PictureType _pictureType, int _sourceWidth, int _sourceHeight, int _viewWidth, int _viewHeight)
        {
            var _bitmapImage = new BitmapImage();

            try
            {
                _bitmapImage.BeginInit();
                _bitmapImage.StreamSource = _stream;

                // 表示領域より大きな画像を読み込む場合
                if (!CheckPictureSize(_sourceWidth, _sourceHeight, _viewWidth, _viewHeight))
                {
                    // 読み込む画像が縦長か横長かスクエアであるかによって、Decode制限を決める
                    switch (_pictureType)
                    {
                        case MediaContentChecker.PictureType.Horizontal:
                            _bitmapImage.DecodePixelWidth = _viewWidth;
                            break;
                        case MediaContentChecker.PictureType.Vertical:
                        case MediaContentChecker.PictureType.Square:
                            _bitmapImage.DecodePixelHeight = _viewHeight;
                            break;
                        case MediaContentChecker.PictureType.Unknown:
                        default:
                            // Todo エラー処理する必要あり。
                            break;
                    }
                }

                _bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                _bitmapImage.CreateOptions = BitmapCreateOptions.None;
                _bitmapImage.EndInit();
            }
            catch (Exception)
            {
                _bitmapImage = null;    // エラー時はリセット
                throw new OutOfMemoryException();
            }

            return _bitmapImage;
        }

        /// <summary>
        /// BitmapSourceをリサイズする
        /// </summary>
        /// <param name="_source">Bitmapで読み込んだ画像情報</param>
        /// <param name="_maxWidth">リサイズする最大幅</param>
        /// <param name="_maxHeight">リサイズする最大高さ</param>
        /// <returns>リサイズ後のBitmapSourceを返す</returns>
        private static BitmapSource CreateResizeImage(BitmapSource _source, int _maxWidth, int _maxHeight)
        {
            // 縮小率を求める
            double _scaleX = (double)_maxWidth / _source.PixelWidth;
            double _scaleY = (double)_maxHeight / _source.PixelHeight;
            double _scale = Math.Min(_scaleX, _scaleY);

            // 縮小されたBitmapを作成
            var _transformedBitmap = new TransformedBitmap();
            _transformedBitmap.BeginInit();
            _transformedBitmap.Source = _source;
            _transformedBitmap.Transform = new ScaleTransform(_scale, _scale);
            _transformedBitmap.EndInit();

            return new WriteableBitmap(_transformedBitmap);
        }

        /// <summary>
        /// メタデータから回転情報を取得する
        /// </summary>
        /// <param name="_metaData"><メタデータ/param>
        /// <returns>画像の回転情報</returns>
        private static uint GetRotation(BitmapMetadata _metaData)
        {
            string _query = "/app1/ifd/exif:{uint=274}";
            if (!_metaData.ContainsQuery(_query))
            {
                return 0;   // エラーとして返す
            }

            return Convert.ToUInt32(_metaData.GetQuery(_query));
        }

        /// <summary>
        /// BitmapSourceをOrientationに合わせて回転するメソッド
        /// </summary>
        private static BitmapSource RotateBitmapSource(BitmapMetadata _metaData, BitmapSource _bitmapSource, int _viewWidth, int _viewHeight)
        {
            // 画像の回転情報を取得
            uint _rotation = GetRotation(_metaData);

            switch (_rotation)
            {
                case 1:
                    break;
                case 3:
                    _bitmapSource = TransformBitmap(_bitmapSource, new RotateTransform(180));
                    break;
                case 6:
                    _bitmapSource = TransformBitmap(_bitmapSource, new RotateTransform(90));
                    break;
                case 8:
                    _bitmapSource = TransformBitmap(_bitmapSource, new RotateTransform(270));
                    break;
                case 2:
                    _bitmapSource = TransformBitmap(_bitmapSource, new ScaleTransform(-1, 1, 0, 0));
                    break;
                case 4:
                    _bitmapSource = TransformBitmap(_bitmapSource, new ScaleTransform(1, -1, 0, 0));
                    break;
                case 5:
                    _bitmapSource = TransformBitmap(TransformBitmap(_bitmapSource, new RotateTransform(90)), new ScaleTransform(-1, 1, 0, 0));
                    break;
                case 7:
                    _bitmapSource = TransformBitmap(TransformBitmap(_bitmapSource, new RotateTransform(270)), new ScaleTransform(-1, 1, 0, 0));
                    break;
                default:
                    break;
            }

            return _bitmapSource;
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
        /// ソースとなる画像のサイズをチェックするメソッド
        /// </summary>
        /// <param name="_sourceWidth">ソース画像の幅</param>
        /// <param name="_sourceHeight">ソース画像の高さ</param>
        /// <param name="_maxWidth">最大幅</param>
        /// <param name="_maxHeight">最大高さ</param>
        /// <returns>最大幅、最大高さよりSourceとなる画像のサイズが大きい場合はFalseを返す</returns>
        private static bool CheckPictureSize(int _sourceWidth, int _sourceHeight, int _maxWidth, int _maxHeight)
        {
            bool _isSmaller = true;

            if (_sourceWidth > _maxWidth || _sourceHeight > _maxHeight)
            {
                _isSmaller = false;
            }

            return _isSmaller;
        }

        /// <summary>
        /// ビットマップ画像を指定した拡張子のファイルに保存する
        /// </summary>
        /// <param name="_bitmap">保存する画像</param>
        /// <param name="_extension">保存するファイル拡張子</param>
        /// <param name="_filePath">保存するファイルパス</param>
        /// <returns>保存に成功したかどうか</returns>
        private static bool SaveBitmapImage(BitmapSource _bitmapSource, string _extension, string _filePath, int _quality)
        {
            // Jpegファイルで保存する場合
            if (_extension == ".jpg")
            {
                using (FileStream _stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    var _encoder = new JpegBitmapEncoder()
                    {
                        QualityLevel = _quality    // 保存時の品質を設定
                    };

                    _encoder.Frames.Add(BitmapFrame.Create(_bitmapSource));
                    _encoder.Save(_stream);
                }

                return true;
            }

            // Pngファイルで保存する場合
            if (_extension == ".png")
            {
                using (FileStream _stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    var _encoder = new PngBitmapEncoder();

                    _encoder.Frames.Add(BitmapFrame.Create(_bitmapSource));
                    _encoder.Save(_stream);
                }

                return true;
            }

            // Bmpファイルで保存する場合
            if (_extension == ".bmp")
            {
                using (FileStream _stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    var _encoder = new BmpBitmapEncoder();

                    _encoder.Frames.Add(BitmapFrame.Create(_bitmapSource));
                    _encoder.Save(_stream);
                }

                return true;
            }

            // Gifファイルで保存する場合
            if (_extension == ".gif")
            {
                using (FileStream _stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    var _encoder = new GifBitmapEncoder();

                    _encoder.Frames.Add(BitmapFrame.Create(_bitmapSource));
                    _encoder.Save(_stream);
                }

                return true;
            }

            // Tiffファイルで保存する場合
            if (_extension == ".tif")
            {
                using (FileStream _stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    var _encoder = new TiffBitmapEncoder();

                    _encoder.Frames.Add(BitmapFrame.Create(_bitmapSource));
                    _encoder.Save(_stream);
                }

                return true;
            }

            // 保存失敗時
            return false;
        }

        /// <summary>
        /// 保存ダイアログを表示する
        /// </summary>
        /// <param name="_path">保存したいファイルのパス</param>
        /// <param name="_outFilePath">保存ダイアログで設定した保存用のファイルパス</param>
        /// <returns>保存する場合はTrue、保存キャンセルはFalseを返す</returns>
        private static bool ShowSaveDialog(string _path, out string _outFilePath)
        {
            CommonSaveFileDialog _sfd = new CommonSaveFileDialog();

            // 保存ダイアログの設定
            _sfd.InitialDirectory = DEFAULT_PICTURE_PATH;
            _sfd.DefaultFileName = Path.GetFileName(_path);
            _sfd.Title = "保存先のファイルを選択してください";

            // 保存できる拡張子
            _sfd.Filters.Add(new CommonFileDialogFilter("Jpegファイル", ".jpg"));
            _sfd.Filters.Add(new CommonFileDialogFilter("Pngファイル", ".png"));
            _sfd.Filters.Add(new CommonFileDialogFilter("Bmpファイル", ".bmp"));
            _sfd.Filters.Add(new CommonFileDialogFilter("Gifファイル", ".gif"));
            _sfd.Filters.Add(new CommonFileDialogFilter("Tiffファイル", ".tif"));

            // デフォルトの拡張子の設定
            _sfd.AlwaysAppendDefaultExtension = true;
            _sfd.DefaultExtension = ".jpg";    // デフォルトはJpegファイルを指定

            // 保存ダイアログを表示
            if (_sfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _outFilePath = _sfd.FileAsShellObject.ParsingName;
                return true;
            }

            // 保存ダイアログでキャンセルした場合
            _outFilePath = "";
            return false;
        }
    }
}

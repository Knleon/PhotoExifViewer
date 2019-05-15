using Prism.Mvvm;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class MediaContentInfo : BindableBase
    {
        /// <summary>
        /// メディアタイプ(ピクチャ or ビデオ)
        /// </summary>
        public enum MediaType
        {
            PICTURE,
            MOVIE,
        }

        /// <summary>
        /// メディアタイプ
        /// </summary>
        public MediaType ContentMediaType
        {
            get { return CheckMediaType(FilePath); }
        }

        private string _filePath;
        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileName(this.FilePath); }
        }

        private DateTime _createTime;
        /// <summary>
        /// ファイル作成日
        /// </summary>
        public DateTime CreateTime
        {
            get { return _createTime; }
            set { SetProperty(ref _createTime, value); }
        }

        private string _mediaDate;
        /// <summary>
        /// メディアファイルの日付
        /// </summary>
        public string MediaDate
        {
            get { return _mediaDate; }
            set { SetProperty(ref _mediaDate, value); }
        }

        /// <summary>
        /// サムネイル画像
        /// </summary>
        public BitmapSource ThumbnailImage { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MediaContentInfo()
        {
        }

        /// <summary>
        /// コピーコンストラクタ(各コンテンツのコンストラクタで呼ばれる)
        /// </summary>
        /// <param name="_mediaFileInfo">メディアファイルの情報</param>
        public MediaContentInfo(MediaContentInfo _mediaFileInfo)
        {
            this.FilePath = _mediaFileInfo.FilePath;
            this.CreateTime = _mediaFileInfo.CreateTime;
            this.MediaDate = _mediaFileInfo.MediaDate;
            this.ThumbnailImage = _mediaFileInfo.ThumbnailImage;
        }

        /// <summary>
        /// メディアファイルのタイプを取得する
        /// </summary>
        /// <param name="_filePath">確認するファイルパス</param>
        /// <returns>ファイルのタイプ</returns>
        private MediaType CheckMediaType(string _filePath)
        {
            string _extension = Path.GetExtension(_filePath).ToLower();

            if (MediaContentChecker.CheckPictureExtensions(_extension))
            {
                return MediaType.PICTURE;
            }
            else if (MediaContentChecker.CheckMovieExtensions(_extension))
            { 
                return MediaType.MOVIE;
            }

            throw new FileFormatException();
        }

        /// <summary>
        /// サムネイル画像を生成する
        /// </summary>
        /// <param name="_info">メディア情報</param>
        /// <returns>サムネイル画像</returns>
        public bool CreateThumbnailImage()
        {
            if (this.FilePath == null || !File.Exists(this.FilePath))
            {
                // 見つからない場合は例外を投げる
                throw new FileNotFoundException();
            }

            switch (this.ContentMediaType)
            {
                case MediaType.PICTURE:
                    this.ThumbnailImage = ImageFileControl.CreatePictureThumnailImage(this.FilePath);
                    break;
                case MediaType.MOVIE:
                    this.ThumbnailImage = ImageFileControl.CreateMovieThumbnailImage(this.FilePath);
                    break;
                default:
                    // Todo: エラー処理が必要
                    return false;
            }

            return true;
        }
    }

    // ピクチャコンテンツ
    //
    public class PictureMediaContent : MediaContentInfo
    {
        #region MediaInfo Parameter (ex:Photo Info...)        
        private string _pictureWidth;
        /// <summary>
        /// 写真の幅
        /// </summary>
        public string PictureWidth
        {
            get { return _pictureWidth; }
            set { SetProperty(ref _pictureWidth, value); }
        }

        private string _pictureHeight;
        /// <summary>
        /// 写真の高さ
        /// </summary>
        public string PictureHeight
        {
            get { return _pictureHeight; }
            set { SetProperty(ref _pictureHeight, value); }
        }

        private string _bitDepth;
        /// <summary>
        /// 写真のビット深度
        /// </summary>
        public string BitDepth
        {
            get { return _bitDepth; }
            set { SetProperty(ref _bitDepth, value); }
        }

        private string _verticalResolution;
        /// <summary>
        /// 写真の垂直解像度
        /// </summary>
        public string VerticalResolution
        {
            get { return _verticalResolution; }
            set { SetProperty(ref _verticalResolution, value); }
        }

        private string _horizenResolution;
        /// <summary>
        /// 写真の水平解像度
        /// </summary>
        public string HorizenResolution
        {
            get { return _horizenResolution; }
            set { SetProperty(ref _horizenResolution, value); }
        }
        #endregion

        #region Camera Info
        private string _aperture;
        /// <summary>
        /// 露出
        /// </summary>
        public string Aperture
        {
            get { return _aperture; }
            set { SetProperty(ref _aperture, value); }
        }

        private string _shutterSpeedText;
        /// <summary>
        /// シャッタースピード
        /// </summary>
        public string ShutterSpeedText
        {
            get { return _shutterSpeedText; }
            set { SetProperty(ref _shutterSpeedText, value); }
        }

        private string _iso;
        /// <summary>
        /// ISO
        /// </summary>
        public string Iso
        {
            get { return _iso; }
            set { SetProperty(ref _iso, value); }
        }

        private string _meteringModeText;
        /// <summary>
        /// 焦点距離の算出方法(AF or MF)
        /// </summary>
        public string MeteringModeText
        {
            get { return _meteringModeText; }
            set { SetProperty(ref _meteringModeText, value); }
        }

        private string _focalLength;
        /// <summary>
        /// 焦点距離
        /// </summary>
        public string FocalLength
        {
            get { return _focalLength; }
            set { SetProperty(ref _focalLength, value); }
        }

        private string _cameraModel;
        /// <summary>
        /// カメラのモデル
        /// </summary>
        public string CameraModel
        {
            get { return _cameraModel; }
            set { SetProperty(ref _cameraModel, value); }
        }

        private string _cameraManufacturer;
        /// <summary>
        /// カメラの製造会社
        /// </summary>
        public string CameraManufacturer
        {
            get { return _cameraManufacturer; }
            set { SetProperty(ref _cameraManufacturer, value); }
        }

        private string _whiteBlanceText;
        /// <summary>
        /// ホワイトバランス
        /// </summary>
        public string WhiteBlanceText
        {
            get { return _whiteBlanceText; }
            set { SetProperty(ref _whiteBlanceText, value); }
        }

        private string _exposeProgramText;
        /// <summary>
        /// 露出の設定プログラム（絞り優先など）
        /// </summary>
        public string ExposeProgramText
        {
            get { return _exposeProgramText; }
            set { SetProperty(ref _exposeProgramText, value); }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PictureMediaContent() : base(null)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PictureMediaContent(MediaContentInfo _mediaFileInfo) : base(_mediaFileInfo)
        {
        }
    }

    // ビデオコンテンツ (今後、拡張可能なように…)
    //
    public class MovieMediaContent : MediaContentInfo
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MovieMediaContent() : base(null)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MovieMediaContent(MediaContentInfo _mediaFileInfo) : base(_mediaFileInfo)
        {
        }
    }
}

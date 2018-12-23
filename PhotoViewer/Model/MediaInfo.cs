using Prism.Mvvm;
using System;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class MediaInfo : BindableBase
    {
        #region FileInfo Parameter
        private string _fileName;
        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
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

        private DateTime _createTime;
        /// <summary>
        /// ファイル作成日
        /// </summary>
        public DateTime CreateTime
        {
            get { return _createTime; }
            set { SetProperty(ref _createTime, value); }
        }

        private string _mediaInfoItemTooltip;
        /// <summary>
        /// メディアファイルのTooltip(ファイル名)
        /// </summary>
        public string MediaInfoItemTooltip
        {
            get { return _mediaInfoItemTooltip; }
            set { SetProperty(ref _mediaInfoItemTooltip, value); }
        }
        #endregion

        #region MediaInfo Parameter (ex:Photo Info...)
        private string _mediaDate;
        /// <summary>
        /// メディアファイルの日付
        /// </summary>
        public string MediaDate
        {
            get { return _mediaDate; }
            set { SetProperty(ref _mediaDate, value); }
        }
        
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
        /// サムネイル画像
        /// </summary>
        public BitmapSource ThumbnailImage { get; set; }

        // コンストラクタ
        public MediaInfo(){}
    }
}

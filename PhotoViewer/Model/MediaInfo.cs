using Prism.Mvvm;
using System;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class MediaInfo : BindableBase
    {
        // メディア情報をプロパティで保持
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        private DateTime _createTime;
        public DateTime CreateTime
        {
            get { return _createTime; }
            set { SetProperty(ref _createTime, value); }
        }

        // MediaInfoItemのTooltip(ファイル名)
        private string _mediaInfoItemTooltip;
        public string MediaInfoItemTooltip
        {
            get { return _mediaInfoItemTooltip; }
            set { SetProperty(ref _mediaInfoItemTooltip, value); }
        }

        // イメージ情報
        private string _mediaDate;
        public string MediaDate
        {
            get { return _mediaDate; }
            set { SetProperty(ref _mediaDate, value); }
        }
        
        private string _pictureWidth;
        public string PictureWidth
        {
            get { return _pictureWidth; }
            set { SetProperty(ref _pictureWidth, value); }
        }

        private string _pictureHeight;
        public string PictureHeight
        {
            get { return _pictureHeight; }
            set { SetProperty(ref _pictureHeight, value); }
        }

        private string _bitDepth;
        public string BitDepth
        {
            get { return _bitDepth; }
            set { SetProperty(ref _bitDepth, value); }
        }

        private string _verticalResolution;
        public string VerticalResolution
        {
            get { return _verticalResolution; }
            set { SetProperty(ref _verticalResolution, value); }
        }

        private string _horizenResolution;
        public string HorizenResolution
        {
            get { return _horizenResolution; }
            set { SetProperty(ref _horizenResolution, value); }
        }

        // カメラ情報
        private string _aperture;
        public string Aperture
        {
            get { return _aperture; }
            set { SetProperty(ref _aperture, value); }
        }

        private string _shutterSpeedText;
        public string ShutterSpeedText
        {
            get { return _shutterSpeedText; }
            set { SetProperty(ref _shutterSpeedText, value); }
        }

        private string _iso;
        public string Iso
        {
            get { return _iso; }
            set { SetProperty(ref _iso, value); }
        }

        private string _meteringModeText;
        public string MeteringModeText
        {
            get { return _meteringModeText; }
            set { SetProperty(ref _meteringModeText, value); }
        }

        private string _focalLength;
        public string FocalLength
        {
            get { return _focalLength; }
            set { SetProperty(ref _focalLength, value); }
        }

        private string _cameraModel;
        public string CameraModel
        {
            get { return _cameraModel; }
            set { SetProperty(ref _cameraModel, value); }
        }

        private string _cameraManufacturer;
        public string CameraManufacturer
        {
            get { return _cameraManufacturer; }
            set { SetProperty(ref _cameraManufacturer, value); }
        }

        private string _whiteBlanceText;
        public string WhiteBlanceText
        {
            get { return _whiteBlanceText; }
            set { SetProperty(ref _whiteBlanceText, value); }
        }

        private string _exposeProgramText;
        public string ExposeProgramText
        {
            get { return _exposeProgramText; }
            set { SetProperty(ref _exposeProgramText, value); }
        }

        public BitmapSource ThumbnailImage { get; set; }

        // コンストラクタ
        public MediaInfo()
        {

        }
    }
}

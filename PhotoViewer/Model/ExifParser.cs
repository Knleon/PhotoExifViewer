using Shell32;
using System;
using System.IO;

namespace PhotoViewer.Model
{
    public static class ExifParser
    {
        /// <summary>
        /// Exifタグをまとめた列挙型
        /// </summary>
        public enum Property_Tag
        {
            MEDIA_DATE = 12,
            IMAGE_WIDTH = 177,
            IMAGE_HEIGHT = 179,
            BIT_DEPTH = 175,
            IMAGE_X_RESOLUTION = 176,
            IMAGE_Y_RESOLUTION = 178,
            F_NUMBER = 261,
            SHUTTER_SPEED = 260,
            ISO = 265,
            METERING_MODE = 270,
            FOCAL_LENGTH = 263,
            CAMERA_MODEL = 30,
            CAMERA_MANUFACTURER = 32,
            WHITE_BALANCE = 276,
            EXPOSURE_PROGRAM = 259
        }

        /// <summary>
        /// ファイルプロパティを取得するメソッド
        /// </summary>
        [STAThread]
        public static string GetFileProperty(string _filePath, Property_Tag _propertyTag)
        {
            string _directoryName = Path.GetDirectoryName(_filePath);
            string _fileName = Path.GetFileName(_filePath);

            var shellAppType = Type.GetTypeFromProgID("Shell.Application");
            dynamic _shell = Activator.CreateInstance(shellAppType);

            Folder _objFolder = _shell.NameSpace(_directoryName);
            FolderItem _folderItem = _objFolder.ParseName(_fileName);
            
            return _objFolder.GetDetailsOf(_folderItem, (int)_propertyTag);
        }

        /// <summary>
        /// ExifデータをMediaInfoにセットするメソッド
        /// </summary>
        public static void SetExifDataToMediaInfo(PictureMediaContent _info)
        {
            // ファイルパスをセット
            string _filePath = _info.FilePath;
            
            // Exifタグにより場合分け
            foreach(Property_Tag _tag in Enum.GetValues(typeof(Property_Tag)))
            {
                string _propertyText = GetFileProperty(_filePath, _tag);
                switch(_tag)
                {
                    case Property_Tag.MEDIA_DATE:
                        _info.MediaDate = _propertyText;
                        break;
                    case Property_Tag.IMAGE_WIDTH:
                        _info.PictureWidth = _propertyText;
                        break;
                    case Property_Tag.IMAGE_HEIGHT:
                        _info.PictureHeight = _propertyText;
                        break;
                    case Property_Tag.BIT_DEPTH:
                        _info.BitDepth = _propertyText;
                        break;
                    case Property_Tag.IMAGE_X_RESOLUTION:
                        _info.HorizenResolution = _propertyText;
                        break;
                    case Property_Tag.IMAGE_Y_RESOLUTION:
                        _info.VerticalResolution = _propertyText;
                        break;
                    case Property_Tag.F_NUMBER:
                        _info.Aperture = _propertyText;
                        break;
                    case Property_Tag.SHUTTER_SPEED:
                        _info.ShutterSpeedText = _propertyText;
                        break;
                    case Property_Tag.ISO:
                        _info.Iso = _propertyText;
                        break;
                    case Property_Tag.METERING_MODE:
                        _info.MeteringModeText = _propertyText;
                        break;
                    case Property_Tag.FOCAL_LENGTH:
                        _info.FocalLength = _propertyText;
                        break;
                    case Property_Tag.CAMERA_MODEL:
                        _info.CameraModel = _propertyText;
                        break;
                    case Property_Tag.CAMERA_MANUFACTURER:
                        _info.CameraManufacturer = _propertyText;
                        break;
                    case Property_Tag.WHITE_BALANCE:
                        _info.WhiteBlanceText = _propertyText;
                        break;
                    case Property_Tag.EXPOSURE_PROGRAM:
                        _info.ExposeProgramText = _propertyText;
                        break;
                }
            }
        }
    }
}

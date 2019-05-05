using Microsoft.WindowsAPICodePack.Shell;

namespace PhotoViewer.Model
{
    public static class ExifParser
    {
        /// <summary>
        /// ファイルプロパティを取得するメソッド
        /// </summary>
        public static void SetFileProperty(string _filePath, PictureMediaContent _pictureContent)
        {
            //  ファイルからプロパティを取得
            using (var _shell = ShellObject.FromParsingName(_filePath))
            {
                // 撮影日時の設定
                SetMediaDate(_shell, _pictureContent);
                // カメラモデルとカメラ製造元の情報の設定
                SetCameraData(_shell, _pictureContent);
                // 画像の幅と高さの設定
                SetImageWidthAndHeight(_shell, _pictureContent);
                // 画像の解像度の設定
                SetImageResolutionWidthAndHeight(_shell, _pictureContent);
                // ビットの深さを設定
                SetBitDepth(_shell, _pictureContent);
                // シャッター速度と絞り値の設定
                SetFnumberAndShutterSpeed(_shell, _pictureContent);
                // ISOの設定
                SetISO(_shell, _pictureContent);
                // 焦点距離の設定
                SetFocusLength(_shell, _pictureContent);
                // 測光モードの設定
                SetMeteringMode(_shell, _pictureContent);
                // 露出プログラムとホワイトバランスの設定
                SetExposeModeAndWhiteBlance(_shell, _pictureContent);
            }
        }

        /// <summary>
        /// 撮影日時の情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetMediaDate(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            string _propertyText = "";

            //  プロパティの取得
            var _property = _shell.Properties.System.DateCreated;
            if (_property?.ValueAsObject == null)
            {
                _propertyText = "";
            }
            else
            {
                _propertyText = _property.Value.ToString();
            }

            // プロパティ値の設定
            _pictureContent.MediaDate = _propertyText;
        }

        /// <summary>
        /// カメラモデルとカメラ製造元の情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetCameraData(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Photo.CameraModel;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.CameraModel = "";
            }
            else
            {
                _pictureContent.CameraModel = _property.Value;
            }

            _property = _shell.Properties.System.Photo.CameraManufacturer;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.CameraManufacturer = "";
            }
            else
            {
                _pictureContent.CameraManufacturer = _property.Value;
            }
        }

        /// <summary>
        /// 画像の幅と高さの情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetImageWidthAndHeight(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Image.HorizontalSize;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.PictureWidth = "";
            }
            else
            {
                _pictureContent.PictureWidth = _property.Value.ToString();
            }

            _property = _shell.Properties.System.Image.VerticalSize;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.PictureHeight = "";
            }
            else
            {
                _pictureContent.PictureHeight = _property.Value.ToString();
            }
        }

        /// <summary>
        /// 画像の解像度の情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetImageResolutionWidthAndHeight(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Image.HorizontalResolution;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.HorizenResolution = "";
            }
            else
            {
                _pictureContent.HorizenResolution = _property.Value.ToString();
            }

            _property = _shell.Properties.System.Image.VerticalResolution;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.VerticalResolution = "";
            }
            else
            {
                _pictureContent.VerticalResolution = _property.Value.ToString();
            }
        }

        /// <summary>
        /// 画像のビットの深さ情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetBitDepth(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Image.BitDepth;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.BitDepth = "";
            }
            else
            {
                _pictureContent.BitDepth = _property.Value.ToString();
            }
        }

        /// <summary>
        /// シャッタ―速度と絞り値の情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetFnumberAndShutterSpeed(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _denominatorProperty = _shell.Properties.System.Photo.ExposureTimeDenominator;
            var _numeratorProperty = _shell.Properties.System.Photo.ExposureTimeNumerator;
            if (_denominatorProperty?.ValueAsObject == null || _numeratorProperty?.ValueAsObject == null)
            {
                _pictureContent.ShutterSpeedText = "";
            }
            else
            {
                int _denominator = (int)_denominatorProperty.Value.Value;
                int _numerator = (int)_numeratorProperty.Value.Value;

                // 最大公約数を求める
                int _commonFactor = _denominator % _numerator;
                if (_commonFactor == 0)
                {
                    _commonFactor = _numerator;
                }

                // 約分したシャッタースピードを表示する
                _pictureContent.ShutterSpeedText = (_numeratorProperty.Value.Value / _commonFactor).ToString() + "/" + (_denominatorProperty.Value.Value / _commonFactor).ToString();
            }

            var _property = _shell.Properties.System.Photo.FNumber;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.Aperture = "";
            }
            else
            {
                _pictureContent.Aperture = _property.Value.ToString();
            }
        }

        /// <summary>
        /// ISO情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetISO(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Photo.ISOSpeed;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.Iso = "";
            }
            else
            {
                _pictureContent.Iso = _property.Value.ToString();
            }
        }

        /// <summary>
        /// 焦点距離の情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetFocusLength(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Photo.FocalLength;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.FocalLength = "";
            }
            else
            {
                _pictureContent.FocalLength = ((uint)_property.Value).ToString();
            }
        }

        /// <summary>
        /// 露出プログラムとホワイトバランスの情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetExposeModeAndWhiteBlance(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Photo.ExposureProgramText;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.ExposeProgramText = "";
            }
            else
            {
                _pictureContent.ExposeProgramText = _property.Value;
            }

            _property = _shell.Properties.System.Photo.WhiteBalanceText;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.WhiteBlanceText = "";
            }
            else
            {
                _pictureContent.WhiteBlanceText = _property.Value;
            }
        }

        /// <summary>
        /// 測光モードの情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="_shell">ShellObject</param>
        /// <param name="_pictureContent">ピクチャコンテンツ</param>
        private static void SetMeteringMode(ShellObject _shell, PictureMediaContent _pictureContent)
        {
            //  プロパティの取得
            var _property = _shell.Properties.System.Photo.MeteringModeText;
            if (_property?.ValueAsObject == null)
            {
                _pictureContent.MeteringModeText = "";
            }
            else
            {
                _pictureContent.MeteringModeText = _property.Value;
            }
        }

        /// <summary>
        /// ExifデータをMediaInfoにセットするメソッド
        /// </summary>
        public static void SetExifDataToMediaInfo(PictureMediaContent _info)
        {
            // ファイルパスをセット
            string _filePath = _info.FilePath;

            // ファイルの各プロパティを設定する
            SetFileProperty(_filePath, _info);
        }
    }
}

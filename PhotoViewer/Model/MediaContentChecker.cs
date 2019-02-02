using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.IO;

namespace PhotoViewer.Model
{
    public static class MediaContentChecker
    {
        // サポートする拡張子(Picture、Movie)
        private static readonly string[] SupportPictureExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif", ".gif", ".nef", ".dng" };
        private static readonly string[] SupportRawPictureExtensions = { ".nef", ".dng" };
        private static readonly string[] SupportMovieExtensions = { ".avi", ".mov", ".mp4" };

        /// <summary>
        /// ピクチャサイズで縦長か横長かスクエアか
        /// </summary>
        public enum PictureType
        {
            Unknown,
            Square,
            Horizontal,
            Vertical,
        }

        /// <summary>
        /// サポートする拡張子の一覧を取得する
        /// </summary>
        /// <returns>サポートする拡張子の一覧</returns>
        public static string[] GetSupportExtensions()
        {
            int _numSupportPictureExtension = SupportPictureExtensions.Length;
            int _numSupportMovieExtension = SupportMovieExtensions.Length;
            string[] _supportExtensions = new string[_numSupportPictureExtension + _numSupportMovieExtension];

            // サポートする拡張子の配列を結合
            Array.Copy(SupportPictureExtensions, _supportExtensions, _numSupportPictureExtension);
            Array.Copy(SupportMovieExtensions, 0, _supportExtensions, _numSupportPictureExtension, _numSupportMovieExtension);

            return _supportExtensions;
        }

        /// <summary>
        /// 静止画のサポートする拡張子であるか確認する
        /// </summary>
        /// <param name="_extension">確認する拡張子</param>
        /// <returns>サポートする拡張子の場合はTrue, ない場合はFalseを返す</returns>
        public static bool CheckPictureExtensions(string _extension)
        {
            bool _isSupport = false;

            foreach (var _supportExtension in SupportPictureExtensions)
            {
                if (_supportExtension == _extension)
                {
                    _isSupport = true;
                }
            }

            return _isSupport;
        }

        /// <summary>
        /// 静止画のRaw画像の拡張子であるか確認する
        /// </summary>
        /// <param name="_extension">確認する拡張子</param>
        /// <returns>Raw画像の拡張子の場合はTrue、そうでない場合はFalseを返す</returns>
        public static bool CheckRawImageExtensions(string _extension)
        {
            bool _isRawExtension = false;

            foreach (var _rawExtension in SupportRawPictureExtensions)
            {
                if (_rawExtension == _extension)
                {
                    _isRawExtension = true;
                }
            }

            return _isRawExtension;
        }

        /// <summary>
        /// 動画のサポートする拡張子であるか確認する
        /// </summary>
        /// <param name="_extension">確認する拡張子</param>
        /// <returns>サポートする拡張子の場合はTrue, ない場合はFalseを返す</returns>
        public static bool CheckMovieExtensions(string _extension)
        {
            bool _isSupport = false;

            foreach (var _supportExtension in SupportMovieExtensions)
            {
                if (_supportExtension == _extension)
                {
                    _isSupport = true;
                }
            }

            return _isSupport;
        }

        /// <summary>
        /// 読み込む画像が縦長か横長かスクエアか確認
        /// </summary>
        /// <param name="_filePath">ファイルパス</param>
        /// <param name="_sourceWidth">読み込む画像の幅(out)</param>
        /// <param name="_sourceHeight">読み込む画像の高さ(out)</param>
        /// <returns>読み込む画像が縦長か横長かスクエアかを返す</returns>
        public static PictureType CheckPictureType(string _filePath, out int _sourceWidth, out int _sourceHeight)
        {
            try
            {
                // WindowsAPICodePackを用いてファイル情報を取得
                ShellFile _shellFile = ShellFile.FromFilePath(_filePath);

                // ファイル情報から画像サイズを取得する
                _sourceWidth = (int)_shellFile.Properties.System.Image.HorizontalSize.Value;
                _sourceHeight = (int)_shellFile.Properties.System.Image.VerticalSize.Value;

                if (_sourceWidth > _sourceHeight)
                {
                    return PictureType.Horizontal;
                }

                if (_sourceHeight > _sourceWidth)
                {
                    return PictureType.Vertical;
                }

                if (_sourceWidth == _sourceHeight)
                {
                    return PictureType.Square;
                }

                return PictureType.Unknown;
            }
            catch (Exception _ex)
            {
                App.LogException(_ex);
                throw new FileNotFoundException();
            }
        }
    }
}

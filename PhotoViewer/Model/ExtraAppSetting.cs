using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace PhotoViewer.Model
{
    public class ExtraAppSetting:BindableBase
    {
        #region ExtraAppSettingProperty
        private int _id;
        /// <summary>
        /// 管理ID
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _name;
        /// <summary>
        /// 連携アプリ名
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _path;
        /// <summary>
        /// 連携アプリの実行ファイルのパス
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }
        #endregion
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExtraAppSetting(){}

        /// <summary>
        /// XMLファイルを生成し、ConfファイルをExportするメソッド
        /// </summary>
        public static void Export(ObservableCollection<ExtraAppSetting> _appSettingList)
        {
            // XMLの生成
            XDocument _xdoc = CreateExtraAppXml(_appSettingList);

            // ファイル保存(存在する場合は上書き)
            const string _appPath = @"\Photo Exif Viewer";
            const string _appConfPath = @"\Photo Exif Viewer.conf";
            string _applicationDataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string _directoryPath = _applicationDataPath + _appPath;
            string _appFilePath = _directoryPath + _appConfPath;

            // フォルダが存在しない場合は作成
            if (! Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
            SaveXml(_xdoc, _appFilePath);
        }

        /// <summary>
        /// ConfファイルからExtraAppSettingを取得するメソッド
        /// </summary>
        public static void Import(ObservableCollection<ExtraAppSetting> _appSettingList)
        {
            // confファイルの読み込み(ObservableCollectionに値を代入)
            const string _appPath = @"\Photo Exif Viewer\Photo Exif Viewer.conf";
            string _applicationDataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string _path = _applicationDataPath + _appPath;
            try
            {
                ParseExtraAppXml(_path, ref _appSettingList);
            }
            catch
            {
                throw new IOException();
            }
        }

        /// <summary>
        /// XMLのドキュメントを生成するメソッド
        /// </summary>
        private static XDocument CreateExtraAppXml(ObservableCollection<ExtraAppSetting> _appSettingList)
        {
            // XML文書の生成
            var _xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null));

            // Elementの生成
            var _datasElement = new XElement("datas");
            foreach(var _appSetting in _appSettingList)
            {
                var _dataElement = new XElement("data");
                var _idElement = new XElement("id", new XText(_appSetting.Id.ToString()));
                var _nameElement = new XElement("name", new XText(_appSetting.Name));
                var _pathElement = new XElement("path", new XText(_appSetting.Path));

                // dataElement内部にID, Name, Pathの要素を追加
                _dataElement.Add(_idElement);
                _dataElement.Add(_nameElement);
                _dataElement.Add(_pathElement);

                _datasElement.Add(_dataElement);
            }

            // XMLメイン要素に追加
            _xdoc.Add(_datasElement);

            return _xdoc;
        }

        /// <summary>
        /// XMLパース
        /// </summary>
        private static void ParseExtraAppXml(string _filePath, ref ObservableCollection<ExtraAppSetting> _appSettingList)
        {
            // XMLより各要素を取得
            var _xdoc = XDocument.Load(_filePath);
            var _dataElement = _xdoc.Root.Elements();

            foreach (var _element in _dataElement)
            {
                XElement _idElement = _element.Element("id");
                XElement _nameElement = _element.Element("name");
                XElement _pathElement = _element.Element("path");

                ExtraAppSetting _extraAppSetting = new ExtraAppSetting();
                _extraAppSetting.Id = Convert.ToInt32(_idElement.Value);
                _extraAppSetting.Name = _nameElement.Value;
                _extraAppSetting.Path = _pathElement.Value;

                _appSettingList.Add(_extraAppSetting);
            }
        }

        /// <summary>
        /// XMLファイルを保存するメソッド
        /// </summary>
        private static void SaveXml(XDocument _xDocument, string _filePath)
        {
            _xDocument.Save(_filePath);
        }
    }
}

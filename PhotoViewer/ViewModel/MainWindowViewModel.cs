using System.Collections.ObjectModel;
using Prism.Mvvm;
using PhotoViewer.Model;
using System.Windows.Input;
using Prism.Commands;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System;
using PhotoViewer.View;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;

namespace PhotoViewer.ViewModel
{
    class MainWindowViewModel:BindableBase
    {
        // Commandを定義
        public ICommand ReferenceButtonCommand { get; set; }
        public ICommand ExifDeleteButtonCommand { get; set; }
        public ICommand GearButtonCommand { get; set; }
        public ICommand OpenFileExplorerCommand { get; set; }

        #region 値のBinding
        // ViewのImageパラメータ
        private BitmapSource _viewImageSource;
        public BitmapSource ViewImageSource
        {
            get { return _viewImageSource; }
            set { SetProperty(ref _viewImageSource, value); }
        }

        // 選択されたMediaInfoのパラメータ
        private MediaInfo _selectedMediaInfo;
        public MediaInfo SelectedMediaInfo
        {
            get { return _selectedMediaInfo; }
            set { SetProperty(ref _selectedMediaInfo, value); }
        }

        // 表示中のピクチャのフォルダパス
        private string _selectedPicturePath;
        public string SelectedPicturePath
        {
            get { return _selectedPicturePath; }
            set { SetProperty(ref _selectedPicturePath, value); }
        }

        // SaveButtonのIsEnable
        private bool _saveButtonIsEnable;
        public bool SaveButtonIsEnable
        {
            get { return _saveButtonIsEnable; }
            set { SetProperty(ref _saveButtonIsEnable, value); }
        }

        // ExifDeleteButtonのIsEnable
        private bool _exifDeleteButtonIsEnable;
        public bool ExifDeleteButtonIsEnable
        {
            get { return _exifDeleteButtonIsEnable; }
            set { SetProperty(ref _exifDeleteButtonIsEnable, value); }
        }

        // 以前のディレクトリ保持
        private string PreviousFilePath { get; set; }
        #endregion

        /// <summary>
        /// コマンドの設定
        /// </summary>
        private void SetCommand()
        {
            ReferenceButtonCommand = new DelegateCommand(ReferenceButtonClicked);
            ExifDeleteButtonCommand = new DelegateCommand(ExifDeleteButtonClicked);
            GearButtonCommand = new DelegateCommand(GearButtonClicked);
            OpenFileExplorerCommand = new DelegateCommand(OpenFileExplorerButtonClicked);
        }

        // メディア情報の読み込みスレッド
        // 写真、Exif情報を読み込む
        private BackgroundWorker LoadPictureContentsBackgroundWorker;
        private bool LoadPictureContentsBackgroundWorker_Reload; 

        // 情報を格納するリスト
        public ObservableCollection<MediaInfo> MediaInfoList { get; set; }
        public ObservableCollection<ExplorerTreeSourceViewModel> ExplorerTree { get; }
        public ObservableCollection<ContextMenuControl> ContextMenuCollection { get; set; }
        public ObservableCollection<ExtraAppSetting> ExtraAppSettingCollection { get; set; }

        // 外部起動アプリのDictionary
        private Dictionary<string, string> ExtraAppPathDictionary { set; get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            // Viewのコマンドと変数の初期値を設定
            SetCommand();
            PreviousFilePath = "";
            SelectedPicturePath = "";

            // 情報をもつリストを定義
            MediaInfoList = new ObservableCollection<MediaInfo>();
            ExplorerTree = new ObservableCollection<ExplorerTreeSourceViewModel>();
            ContextMenuCollection = new ObservableCollection<ContextMenuControl>();
            ExtraAppSettingCollection = new ObservableCollection<ExtraAppSetting>();

            // 外部起動アプリのDictionaryを定義
            ExtraAppPathDictionary = new Dictionary<string, string>();

            // confファイルからExtraAppSettingを取得(ない場合はスルー)
            ExtraAppSetting.Import(ExtraAppSettingCollection);

            // デフォルトのコンテキストメニューの「メディア削除」を追加
            // ExtraAppSettingCollectionが取得できている場合は、それも追加
            if (ExtraAppSettingCollection.Count == 0)
            {
                SetupDefaultContextMenu();
            }
            else
            {
                UpdateContextMenuFromExtraAppSetting(ExtraAppSettingCollection);
            }

            // 連携アプリ設定からのEvent設定
            LinkageProgramViewModel.LinkageEvent += UpdateLinkageContents;
            LinkageProgramViewModel.DeleteAppEvent += DeleteLinkageContents;
            LinkageProgramViewModel.AllDeleteEvent += AllDeleteLinkageContents;

            // デフォルトパスのリストを取得
            string _defaultPicturePath = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonPictures);

            // エクスプローラのツリーをデフォルト状態で更新
            UpdateExplorerTreeSource();

            // 別スレッドでピクチャコンテンツの更新
            ChangePictureContentsList(_defaultPicturePath);
        }

        /// <summary>
        /// デフォルトのコンテキストメニューである「メディア削除」を設定するメソッド
        /// </summary>
        private void SetupDefaultContextMenu()
        {
            var _contextMenuControl = new ContextMenuControl();

            // コンテキストメニューの表示名を設定
            const string _displayName = "メディア削除";
            _contextMenuControl.DisplayName = _displayName;

            // コンテキストメニューのコレクションに追加
            ContextMenuCollection.Add(_contextMenuControl);
        }

        
        /// <summary>
        /// ContextMenuの実行
        /// </summary>
        public void DoContextMenu(MenuItem _item)
        {
            string _itemHeader = Convert.ToString(_item.Header);
            foreach (var _contextCollection in ContextMenuCollection)
            {
                // デフォルトのメディア削除だった場合
                if (_itemHeader == "メディア削除")
                {
                    DeleteMediaFromFolderClicked();
                    return;
                }

                try
                {
                    // それ以外の場合
                    if (_itemHeader == _contextCollection.DisplayName)
                    {
                        string _appName = _itemHeader.Replace("で開く", "");
                        string _appPath = ExtraAppPathDictionary[_appName];
                        System.Diagnostics.Process.Start(_appPath, SelectedMediaInfo.FilePath);
                        return;
                    }
                }
                catch (Exception _ex)
                {
                    App.LogException(_ex);
                }
            }
        }

        /// <summary>
        /// コンテキストメニューにExtraAppSettingをセットするメソッド
        /// </summary>
        private void UpdateContextMenuFromExtraAppSetting(ObservableCollection<ExtraAppSetting> _extraAppSettingCollection)
        {
            ExtraAppPathDictionary.Clear();
            ContextMenuCollection.Clear();

            // デフォルトのコンテキストメニューであるメディア削除を生成
            SetupDefaultContextMenu();

            foreach (var _extraAppSetting in _extraAppSettingCollection)
            {
                var _contextMenuControl = new ContextMenuControl();

                // コンテキストメニューの表示名を設定
                string _displayName = _extraAppSetting.Name + "で開く";
                _contextMenuControl.DisplayName = _displayName;

                // アイコン画像を読み込み
                Icon _appIcon = Icon.ExtractAssociatedIcon(_extraAppSetting.Path);
                using (MemoryStream _iconStream = new MemoryStream())
                {
                    _appIcon.Save(_iconStream);
                    _contextMenuControl.ContextIcon = BitmapFrame.Create(_iconStream).Clone();
                }

                // 外部起動アプリのDictionaryにDisplayNameとそれに対応したPathを登録
                ExtraAppPathDictionary.Add(_extraAppSetting.Name, _extraAppSetting.Path);

                // コンテキストメニューのコレクションに追加
                ContextMenuCollection.Add(_contextMenuControl);
            }
        }

        /// <summary>
        /// 連携アプリの設定が行われた場合のイベント処理
        /// </summary>
        private void UpdateLinkageContents(object _sender, LinkageEventArgs _e)
        {
            // 追加したい外部アプリ
            var _addExtraAppSettingCollection = _e._addExtraAppSettingCollection;   

            // 1つも登録されていないときは、全て追加
            if (ExtraAppSettingCollection.Count == 0)
            {
                foreach (var _extraAppSetting in _addExtraAppSettingCollection)
                {
                    // 仮のExtraAppSettingのコレクションに保存し、IDで並べ替え
                    var _extraAppSettingCollection = new ObservableCollection<ExtraAppSetting>();
                    _extraAppSettingCollection.Add(_extraAppSetting);
                    var _orderedById = new ObservableCollection<ExtraAppSetting>(_extraAppSettingCollection.OrderBy(n => n.Id));

                    ExtraAppSettingCollection = _orderedById;

                    // ContextMenuの項目を更新
                    UpdateContextMenuFromExtraAppSetting(ExtraAppSettingCollection);

                    // Confファイルに書き出し
                    ExtraAppSetting.Export(ExtraAppSettingCollection);

                    return;
                }
            }

            // 1つ以上登録されているとき
            foreach (var _extraAppSetting in _addExtraAppSettingCollection)
            {
                // 既存のコレクションに同じIDが含まれるか確認
                ExtraAppSetting _containItem = ExtraAppSettingCollection.Where((i) => i.Id == _extraAppSetting.Id).SingleOrDefault();
                if (_containItem != null)
                {
                    // 存在する場合はPathを確認
                    if (_containItem.Path != _extraAppSetting.Path)
                    {
                        // 置き換え
                        ExtraAppSettingCollection[_containItem.Id - 1] = _extraAppSetting;
                    }
                }
                else
                {
                    // 存在しない場合は既存のコレクションに追加してIDでソート
                    ExtraAppSettingCollection.Add(_extraAppSetting);
                    var _orderedById = new ObservableCollection<ExtraAppSetting>(ExtraAppSettingCollection.OrderBy(n => n.Id));
                    ExtraAppSettingCollection = _orderedById;
                }
            }

            // ContextMenuの項目を更新
            UpdateContextMenuFromExtraAppSetting(ExtraAppSettingCollection);

            // Confファイルに書き出し
            ExtraAppSetting.Export(ExtraAppSettingCollection);
        }

        /// <summary>
        /// 連携アプリの設定が削除された場合のイベント処理
        /// </summary>
        private void DeleteLinkageContents(object _sender, DeleteEventArgs _e)
        {
            // 削除するIDを確認
            int _deleteId = _e.DeleteId;

            for (int i = 0; i < ExtraAppSettingCollection.Count; i++)
            {
                try
                {
                    // DeleteIDと一致したら削除する
                    if (ExtraAppSettingCollection[i].Id == _deleteId)
                    {
                        ExtraAppSettingCollection.RemoveAt(i);
                    }
                }
                catch (Exception _ex)
                {
                    App.LogException(_ex);
                }
            }

            // ContextMenuの項目を更新
            UpdateContextMenuFromExtraAppSetting(ExtraAppSettingCollection);

            // Confファイルに書き出し
            ExtraAppSetting.Export(ExtraAppSettingCollection);
        }

        /// <summary>
        /// 連携アプリの全削除が選択された場合のイベント処理
        /// </summary>
        private void AllDeleteLinkageContents(object _sender, EventArgs _e)
        {
            // すべての項目をリセット
            ExtraAppSettingCollection.Clear();
            ExtraAppPathDictionary.Clear();
            ContextMenuCollection.Clear();

            // Confファイルに書き出し
            ExtraAppSetting.Export(ExtraAppSettingCollection);
        }

        /// <summary>
        /// エクスプローラのツリーを更新するメソッド
        /// </summary>
        private void UpdateExplorerTreeSource()
        {
            ExplorerTree.Clear();

            DriveInfo[] _allDrives = DriveInfo.GetDrives();
            foreach(var _drive in _allDrives)
            {
                if(_drive.IsReady == true)
                {
                    bool _isDrive = true;
                    var _explorerTree = new ExplorerTreeSourceViewModel(_drive.Name, _isDrive);
                    _explorerTree.ExplorerEvent += UpdatePictureContentsListFromExplorer;
                    ExplorerTree.Add(_explorerTree);
                }
            }
        }

        /// <summary>
        /// ピクチャコンテンツリストを更新するメソッド
        /// </summary>
        private void ChangePictureContentsList(string _folder)
        {
            if (_folder == SelectedPicturePath)
            {
                return;
            }

            // 以前のファイルパスを保持
            PreviousFilePath = SelectedPicturePath;

            // ファイルパスの更新
            SelectedPicturePath = _folder;
            UpdatePictureContentsList();
        }

        /// <summary>
        /// 別スレッドでピクチャコンテンツの読み込みを行う
        /// </summary>
        private void UpdatePictureContentsList()
        {
            if (LoadPictureContentsBackgroundWorker != null && LoadPictureContentsBackgroundWorker.IsBusy)
            {
                LoadPictureContentsBackgroundWorker_Reload = true;
                LoadPictureContentsBackgroundWorker.CancelAsync();
            }
            else
            {
                if (LoadPictureContentsBackgroundWorker_Reload)
                {
                    return;
                }

                // 別スレッドでピクチャコンテンツの読み込み
                LoadPictureContentsList();
            }
        }

        /// <summary>
        /// ピクチャコンテンツリストを非同期で取り込む
        /// </summary>
        private void LoadPictureContentsList()
        {
            if (!Directory.Exists(SelectedPicturePath))
            {
                // パスが見つからなければ以前のパスを指定
                SelectedPicturePath = PreviousFilePath;

                // 以前のパスはリセット
                PreviousFilePath = "";
                return;
            }

            // メディア情報のリストをクリア
            MediaInfoList.Clear();

            // 読み込みスレッド
            var _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(LoadPictureContentsWorker_DoWork);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadPictureContentsWorker_Completed);
            LoadPictureContentsBackgroundWorker = _backgroundWorker;

            // 読み込みスレッドの実行
            LoadPictureContentsBackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// 静止画を読み込むスレッドの処理
        /// </summary>
        private void LoadPictureContentsWorker_DoWork(object _sender, DoWorkEventArgs _args)
        {
            try
            {
                LoadPictureContentsWorker(_sender, _args);
            }
            catch (Exception _ex)
            {
                App.LogException(_ex);
            }
        }

        /// <summary>
        /// 静止画を読み込む
        /// </summary>
        private void LoadPictureContentsWorker(object _sender, DoWorkEventArgs _args)
        {
            // 選択されたフォルダ内に存在するサポートされる拡張子のファイルをすべて取得
            foreach (string _supportExt in App.Current.SupportExts)
            {
                IEnumerable<string> _filePaths = Directory.EnumerateFiles(SelectedPicturePath, "*" + _supportExt);
                foreach (string _filePath in _filePaths)
                {
                    // キャンセルチェック
                    var _worker = _sender as BackgroundWorker;
                    if (_worker.CancellationPending)
                    {
                        _args.Cancel = true;
                        return;
                    }

                    MediaInfo _mediaInfo = new MediaInfo();
                    _mediaInfo.FilePath = _filePath;

                    // Tooltipのファイル名を登録
                    _mediaInfo.MediaInfoItemTooltip = ImageFileControl.GetFileName(_mediaInfo);

                    // ThumbnailImageの作成
                    _mediaInfo.ThumbnailImage = ImageFileControl.CreateThumnailImage(_mediaInfo.FilePath);
                    _mediaInfo.ThumbnailImage.Freeze();

                    // 準備できたものから先に画像をリストに登録
                    var dispatcher = App.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        MediaInfoList.Add(_mediaInfo);
                    }));
                }
            }
        }

        /// <summary>
        /// 読み込み完了後の処理
        /// </summary>
        private void LoadPictureContentsWorker_Completed(object _sender, RunWorkerCompletedEventArgs _args)
        {
            // 読み込み完了後に処理したいことを記述
            if (_args.Error != null)
            {
                return;
            }
            else if (_args.Cancelled)
            {
                var _worker = _sender as BackgroundWorker;
                _worker.Dispose();
            }
            else
            {
                var _worker = _sender as BackgroundWorker;
                _worker.Dispose();
            }

            if(LoadPictureContentsBackgroundWorker_Reload)
            {
                LoadPictureContentsBackgroundWorker_Reload = false;
                // 別スレッドでピクチャコンテンツの更新
                LoadPictureContentsList();
            }
        }

        /// <summary>
        /// 参照ボタンを押したときの挙動
        /// </summary>
        private void ReferenceButtonClicked()
        {
            // SaveButtonとExifDeleteButtonの無効化
            const bool IsEnableFlag = false;
            SetIsEnableButton(IsEnableFlag);

            // フォルダ選択ダイアログの表示
            string _openFolderPath = ImageFileControl.OpenDirectory();

            // ピクチャコンテンツリストを変更
            ChangePictureContentsList(_openFolderPath);
        }

        /// <summary>
        /// Explorerから選択されたフォルダのメディアを取得するメソッド
        /// </summary>
        public void UpdatePictureContentsListFromExplorer(object _sender, ExplorerEventArgs _e)
        {
            string _folderPath = _e._directoryPath;
            ChangePictureContentsList(_folderPath);
        }

        /// <summary>
        /// Viewに拡大表示するImageSourceを読み込むメソッド
        /// </summary>
        /// <param name="_filePath">画像のファイルパス</param>
        public async void LoadViewImageSource(MediaInfo _info)
        {
            // 以前に選択されたMediaと同じ場合はロードしない
            if(SelectedMediaInfo == _info)
            {
                return;
            }

            // ファイルチェック
            if (!File.Exists(_info.FilePath))
            {
                MessageBox.Show("ファイルは存在しません", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // アクセス権チェック
            if (IsFileLocked(_info.FilePath))
            {
                // ロックされている場合
                MessageBox.Show("ファイルはロックされています", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 選択されているMediaの情報を保持
            SelectedMediaInfo = _info;

            // 画像を表示
            await Task.Run(() => SetPictureAndExifInfo());

            // SaveButtonとExifDeleteButtonの有効化
            const bool IsEnableFlag = true;
            SetIsEnableButton(IsEnableFlag);
        }

        /// <summary>
        /// 静止画とExif情報をセットする
        /// </summary>
        private void SetPictureAndExifInfo()
        {
            const uint _maxContentsWidth = 880;
            const uint _maxContentsHeight = 660;
            BitmapSource _openImage = ImageFileControl.OpenImageFile(SelectedMediaInfo);
            ViewImageSource = ImageFileControl.CreateResizeImage(_openImage, _maxContentsWidth, _maxContentsHeight);
            ViewImageSource.Freeze();

            // Exif情報を取得
            ExifParser.SetExifDataToMediaInfo(SelectedMediaInfo);
        }

        /// <summary>
        /// MediaInfoListの画像をダブルクリックしたときの挙動
        /// </summary>
        public void MediaInfoListDoubleClicked(MediaInfo _info)
        {
            // TODO ファイルを別アプリで開くことを検討中
            string _selectFilePath = _info.FilePath;
        }

        /// <summary>
        /// フォルダを開くボタンをクリックしたときの挙動
        /// </summary>
        private void OpenFileExplorerButtonClicked()
        {
            System.Diagnostics.Process.Start("EXPLORER.EXE", SelectedPicturePath);
        }

        /// <summary>
        /// ListBoxItemの右クリックでメディア削除が押されたときの挙動
        /// </summary>
        private void DeleteMediaFromFolderClicked()
        {
            // メッセージボックスを表示し、削除確認を行う
            MessageBoxResult result = MessageBox.Show("メディアファイルをフォルダから削除しますか？", "メディア削除の確認", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
            
            // メッセージボックスの確認でOKだった場合、フォルダからファイル削除
            if (result == MessageBoxResult.OK)
            {
                try
                {
                    // ファイルチェック
                    if (!File.Exists(SelectedMediaInfo.FilePath))
                    {
                        MessageBox.Show("ファイルは存在しません", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // アクセス権のチェック
                    if (!IsFileLocked(SelectedMediaInfo.FilePath))
                    {
                        // アクセス権があれば削除
                        File.Delete(SelectedMediaInfo.FilePath);

                        // 現在のディレクトリを再読込
                        UpdatePictureContentsList();
                    }
                    else
                    {
                        MessageBox.Show("ファイルはロックされています", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                catch (Exception _ex)
                {
                    App.LogException(_ex);
                }
            }
            else
            {
                // キャンセルだった場合、何もしない
                return;
            }
        }

        /// <summary>
        /// Exif削除ボタンが押されたときの挙動
        /// </summary>
        private void ExifDeleteButtonClicked()
        {
            // ファイルチェック
            if (!File.Exists(SelectedMediaInfo.FilePath))
            {
                MessageBox.Show("ファイルは存在しません", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // アクセス権のチェック
            if (!IsFileLocked(SelectedMediaInfo.FilePath))
            {
                // Exif情報を削除して画像を保存
                if (ImageFileControl.DeleteExifInfo(SelectedMediaInfo))
                {
                    // Exif情報を削除した画像を保存後、現在のディレクトリを再読込
                    UpdatePictureContentsList();
                }
            }
            else
            {
                MessageBox.Show("ファイルはロックされています", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        /// <summary>
        /// SaveButtonとExifDeleteButtonのフラグ設定メソッド
        /// </summary>
        /// <param name="_flag">IsEnableのフラグ</param>
        private void SetIsEnableButton(bool _flag)
        {
            SaveButtonIsEnable = _flag;
            ExifDeleteButtonIsEnable = _flag;
        }

        /// <summary>
        /// 歯車ボタンが押されたときの挙動
        /// </summary>
        private void GearButtonClicked()
        {
            PhotoAppInfoView _infoView = new PhotoAppInfoView();
            _infoView.Owner = Application.Current.MainWindow;
            _infoView.ShowDialog();
        }

        /// <summary>
        /// 指定されたファイルがロックされているかどうかを返します。
        /// </summary>
        /// <param name="path">検証したいファイルへのフルパス</param>
        /// <returns>ロックされているかどうか</returns>
        private bool IsFileLocked(string path)
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return false;
        }

        /// <summary>
        /// スレッドを停止するメソッド
        /// </summary>
        /// <return>スレッドが停止しているかどうか</return>
        public bool StopThreadAndTask()
        {
            bool _isStop = true;
            if (LoadPictureContentsBackgroundWorker != null && LoadPictureContentsBackgroundWorker.IsBusy)
            {
                _isStop = false;
                LoadPictureContentsBackgroundWorker.CancelAsync();
            }

            return _isStop;
        }
    }
}

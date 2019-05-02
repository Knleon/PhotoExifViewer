using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using PhotoViewer.Model;
using System.Collections.ObjectModel;

namespace PhotoViewer.ViewModel
{
    public class LinkageEventArgs : EventArgs
    {
        public ObservableCollection<ExtraAppSetting> _addExtraAppSettingCollection;
    }

    public class DeleteEventArgs : EventArgs
    {
        public int DeleteId;
    }

    class LinkageProgramViewModel : BindableBase
    {
        #region LinkageProgramProperty
        // 連携アプリ選択ダイアログのテキスト
        private readonly string FolderDialogTitle = "連携アプリ選択ダイアログ";

        private string _linkAppPath1;
        /// <summary>
        /// 連携アプリ１の情報
        /// </summary>
        public string LinkAppPath1
        {
            get { return _linkAppPath1; }
            set { SetProperty(ref _linkAppPath1, value); }
        }

        private string _linkAppPath2;
        /// <summary>
        /// 連携アプリ２の情報
        /// </summary>
        public string LinkAppPath2
        {
            get { return _linkAppPath2; }
            set { SetProperty(ref _linkAppPath2, value); }
        }

        private string _linkAppPath3;
        /// <summary>
        /// 連携アプリ３の情報
        /// </summary>
        public string LinkAppPath3
        {
            get { return _linkAppPath3; }
            set { SetProperty(ref _linkAppPath3, value); }
        }
        #endregion

        // 各コマンド
        public ICommand LinkApp1ReferenceCommand { get; set; }
        public ICommand LinkApp2ReferenceCommand { get; set; }
        public ICommand LinkApp3ReferenceCommand { get; set; }
        public ICommand LinkApp1DeleteCommand { get; set; }
        public ICommand LinkApp2DeleteCommand { get; set; }
        public ICommand LinkApp3DeleteCommand { get; set; }
        public ICommand RegisterLinkAppCommand { get; set; }
        public ICommand AllLinkAppDeleteCommand { get; set; }

        // 連携アプリ情報を格納するリストを定義
        private ObservableCollection<ExtraAppSetting> extraAppSettingCollection = new ObservableCollection<ExtraAppSetting>();
        public ObservableCollection<ExtraAppSetting> ExtraAppSettingCollection
        {
            get { return extraAppSettingCollection; }
            set { extraAppSettingCollection = value; }
        }

        private ObservableCollection<ExtraAppSetting> addAppSettingCollection = new ObservableCollection<ExtraAppSetting>();
        public ObservableCollection<ExtraAppSetting> AddAppSettingCollection
        {
            get { return addAppSettingCollection; }
            set { addAppSettingCollection = value; }
        }

        // 参照ボタンでの処理後、ExtraAppSettingを渡すためのイベント
        public delegate void LinkageEventHandler(object _sender, LinkageEventArgs e);
        public static event LinkageEventHandler LinkageEvent;
        protected virtual void OnLinkageEvent(LinkageEventArgs e)
        {
            LinkageEvent?.Invoke(this, e);
        }

        // 全削除ボタンでのイベント
        public static event EventHandler AllDeleteEvent;
        protected virtual void OnAllDeleteEvent(EventArgs e)
        {
            AllDeleteEvent?.Invoke(this, e);
        }

        // 削除ボタンでのDeleteIdを渡すためのイベント
        public delegate void DeleteAppEventHandler(object _sender, DeleteEventArgs e);
        public static event DeleteAppEventHandler DeleteAppEvent;
        protected virtual void OnDeleteAppEvent(DeleteEventArgs e)
        {
            DeleteAppEvent?.Invoke(this, e);
        }

        /// <summary>
        /// コマンドの設定
        /// </summary>
        private void SetCommand()
        {
            LinkApp1ReferenceCommand = new DelegateCommand(LinkApp1ReferenceButtonClicked);
            LinkApp2ReferenceCommand = new DelegateCommand(LinkApp2ReferenceButtonClicked);
            LinkApp3ReferenceCommand = new DelegateCommand(LinkApp3ReferenceButtonClicked);
            LinkApp1DeleteCommand    = new DelegateCommand(LinkApp1DeleteButtonClicked);
            LinkApp2DeleteCommand    = new DelegateCommand(LinkApp2DeleteButtonClicked);
            LinkApp3DeleteCommand    = new DelegateCommand(LinkApp3DeleteButtonClicked);
            RegisterLinkAppCommand   = new DelegateCommand(RegisterLinkAppButtonClicked);
            AllLinkAppDeleteCommand  = new DelegateCommand(AllLinkAppDeleteButtonClicked);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LinkageProgramViewModel()
        {
            // Viewのコマンドと変数の初期値を設定
            SetCommand();
            LinkAppPath1 = "";
            LinkAppPath2 = "";
            LinkAppPath3 = "";

            // confファイルの読み込み(ObservableCollectionに値を代入)
            ExtraAppSetting.Import(ExtraAppSettingCollection);

            // 値をUIに反映
            foreach (var _extraAppSetting in ExtraAppSettingCollection)
            {
                switch (_extraAppSetting.Id)
                {
                    case 1:
                        LinkAppPath1 = _extraAppSetting.Path;
                        break;
                    case 2:
                        LinkAppPath2 = _extraAppSetting.Path;
                        break;
                    case 3:
                        LinkAppPath3 = _extraAppSetting.Path;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// EXEファイル選択ダイアログを表示する
        /// </summary>
        /// <return>ファイル選択でOKが押された場合はTrueを返す</return>
        private bool ShowExeFileSelectDialog(ref string _selectedExeFilePath)
        {
            var _dialog = new CommonOpenFileDialog();

            _dialog.Title = FolderDialogTitle;
            _dialog.EnsureReadOnly = false;
            _dialog.AllowNonFileSystemItems = false;
            _dialog.IsFolderPicker = false;
            _dialog.DefaultExtension = ".exe";
            _dialog.DefaultDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);

            if (_dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                _selectedExeFilePath = "";
                return false;
            }

            _selectedExeFilePath = _dialog.FileName;
            return true;
        }

        /// <summary>
        /// ExtraAppSettingに値をセットする
        /// </summary>
        /// <param name="_id">ID</param>
        /// <param name="_exeFileName">NAME</param>
        /// <param name="_selectExeFilePath">PATH</param>
        private ExtraAppSetting SetExtraAppSetting(int _id, string _exeFileName, string _selectExeFilePath)
        {
            var _extraAppSetting = new ExtraAppSetting(_id, _exeFileName, _selectExeFilePath);
            return _extraAppSetting;
        }

        /// <summary>
        /// 外部連携アプリ1の参照ボタンをクリックしたとき
        /// </summary>
        private void LinkApp1ReferenceButtonClicked()
        {
            // 外部連携IDなどの値設定
            const int _id = 1;
            string _selectExeFilePath = "";

            // Exeファイルの選択ダイアログを表示
            if (!ShowExeFileSelectDialog(ref _selectExeFilePath))
            {
                return;
            }

            // UIに値を反映
            LinkAppPath1 = _selectExeFilePath;

            // AddExtraAppSettingCollectionに値を格納
            var _extraAppSetting = SetExtraAppSetting(_id, Path.GetFileNameWithoutExtension(LinkAppPath1), LinkAppPath1);
            AddAppSettingCollection.Add(_extraAppSetting);
        }

        /// <summary>
        /// 外部連携アプリ2の参照ボタンをクリックしたとき
        /// </summary>
        private void LinkApp2ReferenceButtonClicked()
        {
            // 外部連携IDなどの値設定
            const int _id = 2;
            string _selectExeFilePath = "";

            // Exeファイルの選択ダイアログを表示
            if (!ShowExeFileSelectDialog(ref _selectExeFilePath))
            {
                return;
            }

            // UIに値を反映
            LinkAppPath2 = _selectExeFilePath;

            // AddExtraAppSettingCollectionに値を格納
            var _extraAppSetting = SetExtraAppSetting(_id, Path.GetFileNameWithoutExtension(LinkAppPath2), LinkAppPath2);
            AddAppSettingCollection.Add(_extraAppSetting);
        }

        /// <summary>
        /// 外部連携アプリ3の参照ボタンをクリックしたとき
        /// </summary>
        private void LinkApp3ReferenceButtonClicked()
        {
            // 外部連携IDなどの値設定
            const int _id = 3;
            string _selectExeFilePath = "";

            // Exeファイルの選択ダイアログを表示
            if (!ShowExeFileSelectDialog(ref _selectExeFilePath))
            {
                return;
            }

            // UIに値を反映
            LinkAppPath3 = _selectExeFilePath;

            // AddExtraAppSettingCollectionに値を格納
            var _extraAppSetting = SetExtraAppSetting(_id, Path.GetFileNameWithoutExtension(LinkAppPath3), LinkAppPath3);
            AddAppSettingCollection.Add(_extraAppSetting);
        }

        /// <summary>
        /// 外部連携アプリ1の削除ボタンをクリックしたとき
        /// </summary>
        private void LinkApp1DeleteButtonClicked()
        {
            const int _id = 1;

            // 連携解除のため、文字列削除
            SetDeleteEvent(_id);
            LinkAppPath1 = "";
        }

        /// <summary>
        /// 外部連携アプリ2の削除ボタンをクリックしたとき
        /// </summary>
        private void LinkApp2DeleteButtonClicked()
        {
            const int _id = 2;

            // 連携解除のため、文字列削除
            SetDeleteEvent(_id);
            LinkAppPath2 = "";
        }

        /// <summary>
        /// 外部連携アプリ3の削除ボタンをクリックしたとき
        /// </summary>
        private void LinkApp3DeleteButtonClicked()
        {
            const int _id = 3;

            // 連携解除のため、文字列削除
            SetDeleteEvent(_id);
            LinkAppPath3 = "";
        }

        /// <summary>
        /// DeleteEventを設定する
        /// </summary>
        private void SetDeleteEvent(int _id)
        {
            DeleteEventArgs _deleteEventArgs = new DeleteEventArgs();
            _deleteEventArgs.DeleteId = _id;

            OnDeleteAppEvent(_deleteEventArgs);
        }

        /// <summary>
        /// 外部連携アプリの全削除ボタンをクリックしたとき
        /// </summary>
        private void AllLinkAppDeleteButtonClicked()
        {
            LinkAppPath1 = "";
            LinkAppPath2 = "";
            LinkAppPath3 = "";

            OnAllDeleteEvent(null);
        }

        /// <summary>
        /// 登録ボタンをクリックしたとき
        /// </summary>
        private void RegisterLinkAppButtonClicked()
        {
            // 登録予定のアプリ情報リストをイベント引数に格納
            LinkageEventArgs _linkageEventArgs = new LinkageEventArgs();
            _linkageEventArgs._addExtraAppSettingCollection = AddAppSettingCollection;

            // 登録イベントを発行
            OnLinkageEvent(_linkageEventArgs);

            // 追加予定の連携アプリ情報リストをクリア
            AddAppSettingCollection.Clear();
        }
    }
}

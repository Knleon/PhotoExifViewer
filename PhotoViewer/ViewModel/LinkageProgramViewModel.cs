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
        public ObservableCollection<ExtraAppSetting> ExtraAppSettingCollection { get; set; }
        public ObservableCollection<ExtraAppSetting> AddAppSettingCollection { get; set; }

        // 参照ボタンでの処理後、ExtraAppSettingを渡すためのイベント
        public delegate void LinkageEventHandler(object _sender, LinkageEventArgs e);
        public static event LinkageEventHandler LinkageEvent;
        protected virtual void OnLinkageEvent(LinkageEventArgs e)
        {
            if (LinkageEvent != null)
            {
                LinkageEvent(this, e);
            }
        }

        // 全削除ボタンでのイベント
        public static event EventHandler AllDeleteEvent;
        protected virtual void OnAllDeleteEvent(EventArgs e)
        {
            if (AllDeleteEvent != null)
            {
                AllDeleteEvent(this, e);
            }
        }

        // 削除ボタンでのDeleteIdを渡すためのイベント
        public delegate void DeleteAppEventHandler(object _sender, DeleteEventArgs e);
        public static event DeleteAppEventHandler DeleteAppEvent;
        protected virtual void OnDeleteAppEvent(DeleteEventArgs e)
        {
            if (DeleteAppEvent != null)
            {
                DeleteAppEvent(this, e);
            }
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
        /// 参照ボタンが押されたときの挙動
        /// </summary>
        /// <return>ファイル選択でOKが押された場合はTrueを返す</return>
        private bool LinkAppReferenceButtonClicked(ref string _selectedExeFilePath)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = FolderDialogTitle;
            dialog.EnsureReadOnly = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.IsFolderPicker = false;
            dialog.DefaultExtension = ".exe";
            dialog.DefaultDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _selectedExeFilePath = dialog.FileName;
                return true;
            }
            else
            {
                _selectedExeFilePath = "";
                return false;
            }
        }

        /// <summary>
        /// ExtraAppSettingに値をセットするメソッド
        /// </summary>
        /// <param name="_id">ID</param>
        /// <param name="_exeFileName">NAME</param>
        /// <param name="_selectExeFilePath">PATH</param>
        private ExtraAppSetting SetExtraAppSetting(int _id, string _exeFileName, string _selectExeFilePath)
        {
            var _extraAppSetting = new ExtraAppSetting();
            _extraAppSetting.Id = _id;
            _extraAppSetting.Name = _exeFileName;
            _extraAppSetting.Path = _selectExeFilePath;
            return _extraAppSetting;
        }

        /// <summary>
        /// 外部連携アプリ1の参照ボタンをクリックしたとき
        /// </summary>
        private void LinkApp1ReferenceButtonClicked()
        {
            string _selectExeFilePath = "";
            bool _flag = LinkAppReferenceButtonClicked(ref _selectExeFilePath);
            string _exeFileName = Path.GetFileNameWithoutExtension(_selectExeFilePath);
            const int _id = 1;

            // UIに反映
            if (_flag == true)
            {
                LinkAppPath1 = _selectExeFilePath;

                // AddExtraAppSettingCollectionに値を格納
                var _extraAppSettingClass = new ExtraAppSetting();
                _extraAppSettingClass = SetExtraAppSetting(_id, _exeFileName, _selectExeFilePath);
                AddAppSettingCollection.Add(_extraAppSettingClass);
            }
        }

        /// <summary>
        /// 外部連携アプリ2の参照ボタンをクリックしたとき
        /// </summary>
        private void LinkApp2ReferenceButtonClicked()
        {
            string _selectExeFilePath = "";
            bool _flag = LinkAppReferenceButtonClicked(ref _selectExeFilePath);
            string _exeFileName = Path.GetFileNameWithoutExtension(_selectExeFilePath);
            const int _id = 2;

            // UIに反映
            if (_flag == true)
            {
                LinkAppPath2 = _selectExeFilePath;

                // AddExtraAppSettingCollectionに値を格納
                var _extraAppSettingClass = new ExtraAppSetting();
                _extraAppSettingClass = SetExtraAppSetting(_id, _exeFileName, _selectExeFilePath);
                AddAppSettingCollection.Add(_extraAppSettingClass);
            }
        }

        /// <summary>
        /// 外部連携アプリ3の参照ボタンをクリックしたとき
        /// </summary>
        private void LinkApp3ReferenceButtonClicked()
        {
            string _selectExeFilePath = "";
            bool _flag = LinkAppReferenceButtonClicked(ref _selectExeFilePath);
            string _exeFileName = Path.GetFileNameWithoutExtension(_selectExeFilePath);
            const int _id = 3;

            // UIに反映
            if (_flag == true)
            {
                LinkAppPath3 = _selectExeFilePath;

                // AddExtraAppSettingCollectionに値を格納
                var _extraAppSettingClass = new ExtraAppSetting();
                _extraAppSettingClass = SetExtraAppSetting(_id, _exeFileName, _selectExeFilePath);
                AddAppSettingCollection.Add(_extraAppSettingClass);
            }
        }

        /// <summary>
        /// 外部連携アプリ1の削除ボタンをクリックしたとき
        /// </summary>
        private void LinkApp1DeleteButtonClicked()
        {
            // 連携解除のため、文字列削除
            LinkAppPath1 = "";
            const int _id = 1;
            SetDeleteEvent(_id);
        }

        /// <summary>
        /// 外部連携アプリ2の削除ボタンをクリックしたとき
        /// </summary>
        private void LinkApp2DeleteButtonClicked()
        {
            // 連携解除のため、文字列削除
            LinkAppPath2 = "";
            const int _id = 2;
            SetDeleteEvent(_id);
        }

        /// <summary>
        /// 外部連携アプリ3の削除ボタンをクリックしたとき
        /// </summary>
        private void LinkApp3DeleteButtonClicked()
        {
            // 連携解除のため、文字列削除
            LinkAppPath3 = "";
            const int _id = 3;
            SetDeleteEvent(_id);
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
            LinkageEventArgs _linkageEventArgs = new LinkageEventArgs();
            _linkageEventArgs._addExtraAppSettingCollection = AddAppSettingCollection;
            OnLinkageEvent(_linkageEventArgs);
            AddAppSettingCollection.Clear();
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

            // 連携アプリ情報を格納するリストを定義
            ExtraAppSettingCollection = new ObservableCollection<ExtraAppSetting>();
            AddAppSettingCollection = new ObservableCollection<ExtraAppSetting>();

            // confファイルの読み込み(ObservableCollectionに値を代入)
            ExtraAppSetting.Import(ExtraAppSettingCollection);

            // 値をUIに反映
            foreach(var _extraAppSetting in ExtraAppSettingCollection)
            {
                switch(_extraAppSetting.Id)
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
    }
}

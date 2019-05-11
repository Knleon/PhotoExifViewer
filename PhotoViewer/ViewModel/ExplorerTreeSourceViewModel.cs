using PhotoViewer.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoViewer.ViewModel
{
    public class ExplorerEventArgs : EventArgs
    {
        public string _directoryPath;
    }

    public class ExplorerTreeSourceViewModel : TreeViewItem, INotifyPropertyChanged
    {
        private DirectoryInfo _Directory = null;
        private bool _Expanded = false;
        private bool IsDrive = false;
        private System.IO.FileSystemWatcher FileWatcher = null;

        /// <summary>
        /// ExplorerTreeの更新情報を受け取るイベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private ExplorerTreeSourceViewModel _selectionItem;
        public ExplorerTreeSourceViewModel SelectionItem
        {
            get { return _selectionItem; }
            set
            {
                _selectionItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionItem)));
            }
        }

        /// <summary>
        /// エクスプローライベント
        /// </summary>
        public delegate void ExplorerEventHandler(object _sender, ExplorerEventArgs e);
        public event ExplorerEventHandler ExplorerEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_isDrive"></param>
        public ExplorerTreeSourceViewModel(string _path, bool _isDrive)
        {
            // イベントの設定
            Expanded += ExplorerTreeSource_Expanded;
            Selected += ExplorerTreeSource_Selected;

            // TreeViewItemのセット
            GetTreeViewItem(_path, _isDrive);

            // フォルダの監視を開始
            FileWatcher = new FileSystemWatcher();
            FileWatcher.Path = _path;
            FileWatcher.Filter = "*";
            FileWatcher.NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName);

            // 変更があった場合は通知する
            FileWatcher.Changed += new System.IO.FileSystemEventHandler(FileWatcher_Changed);
            FileWatcher.Created += new System.IO.FileSystemEventHandler(FileWatcher_Changed);
            FileWatcher.Deleted += new System.IO.FileSystemEventHandler(FileWatcher_Changed);
            FileWatcher.Renamed += new System.IO.RenamedEventHandler(FileWatcher_Changed);

            // 監視を開始
            FileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// フォルダ内の情報を取得する
        /// </summary>
        /// <param name="_path">ファイルパス or フォルダパス</param>
        /// <param name="_isDrive">ドライブ直下であるかどうか</param>
        private void GetTreeViewItem(string _path, bool _isDrive)
        {
            Items.Clear();
            IsDrive = _isDrive;

            _Directory = new DirectoryInfo(_path);
            if (_Directory.GetDirectories().Count() > 0)
            {
                Items.Add(new TreeViewItem());
            }

            Header = CreateHeader();
        }

        /// <summary>
        /// ディレクトリ内の情報を更新する
        /// </summary>
        private void UpdateDirectoryNode()
        {
            // ソート用にDirectoryInfoのリストを保持する
            ObservableCollection<DirectoryInfo> _tmpDirecotyInfoList = new ObservableCollection<DirectoryInfo>();
            var _directoryInfoList = _Directory.GetDirectories().ToList();
            foreach (var _dirInfo in _directoryInfoList)
            {
                _tmpDirecotyInfoList.Add(_dirInfo);
            }

            // 自然順でディレクトリのリストを入れ替える
            _tmpDirecotyInfoList = new ObservableCollection<DirectoryInfo>(_tmpDirecotyInfoList.OrderBy(directory => directory, new NaturalDirectoryInfoNameComparer()));

            // TreeViewItemをリセット
            Items.Clear();

            foreach (var _tmpDirInfo in _tmpDirecotyInfoList)
            {
                if (!IsDirectoryLocked(_tmpDirInfo.FullName))
                {
                    // ファイル名の最初の文字を取得
                    string _fileNameFirst = Path.GetFileName(_tmpDirInfo.FullName).Substring(0, 1);
                    const string _tempRecycleFileIndicator = "$";

                    // 最初の文字が”$”だった場合、Windowsの特殊ファイルのためスキップ
                    if (_fileNameFirst != _tempRecycleFileIndicator)
                    {
                        bool _isDrive = false;
                        var _node = new ExplorerTreeSourceViewModel(_tmpDirInfo.FullName, _isDrive);
                        Items.Add(_node);
                    }
                }
            }
        }

        /// <summary>
        /// エクスプローラ内でフォルダをクリックして展開したときのイベント
        /// </summary>
        /// <param name="_sender">Object</param>
        /// <param name="e">引数情報</param>
        private void ExplorerTreeSource_Expanded(object _sender, RoutedEventArgs e)
        {
            if (!_Expanded)
            {
                UpdateDirectoryNode();
            }
            _Expanded = true;
        }

        /// <summary>
        /// FileWatcherがリスト操作を検知したとき
        /// </summary>
        private void FileWatcher_Changed(Object _source, System.IO.FileSystemEventArgs _e)
        {
            // UIスレッドで実行させる
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                UpdateDirectoryNode();
            }));
        }

        /// <summary>
        /// スタックパネルの設定
        /// </summary>
        /// <returns>TreeViewに見せるスタックパネルの設定</returns>
        private StackPanel CreateHeader()
        {
            StackPanel _stackpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            // Iconを生成
            BitmapSource _iconSource = CreateIconBitmapSource(IsDrive);

            _stackpanel.Children.Add(new Image()
            {
                Source = _iconSource,
                Width = 20,
                Height = 20
            });
            _stackpanel.Children.Add(new TextBlock()
            {
                Text = _Directory.Name,
                Margin = new Thickness(2.5, 0, 0, 0)
            });
            _stackpanel.Margin = new Thickness(0, 5, 0, 0);

            return _stackpanel;
        }

        /// <summary>
        /// TreeViewで表示するアイコンの画像を生成するメソッド
        /// </summary>
        private BitmapSource CreateIconBitmapSource(bool _isDrive)
        {
            if (_isDrive)
            {
                BitmapSource _iconImage = WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SIID_DRIVEFIXED);
                return _iconImage;
            }
            else
            {
                BitmapSource _iconImage = WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SIID_FOLDER);
                return _iconImage;
            }
        }

        /// <summary>
        /// TreeViewで選択されたときのイベントメソッド
        /// </summary>
        private void ExplorerTreeSource_Selected(object sender, RoutedEventArgs e)
        {
            SelectionItem = (this.IsSelected) ? this : (ExplorerTreeSourceViewModel)e.Source;
            ExplorerEventArgs _explorerEventArgs = new ExplorerEventArgs();
            _explorerEventArgs._directoryPath = SelectionItem._Directory.FullName;
            ExplorerEvent?.Invoke(this, _explorerEventArgs);
        }

        /// <summary>
        /// ディレクトリのアクセス権チェック
        /// </summary>
        private bool IsDirectoryLocked(string _filePath)
        {
            DirectoryInfo _directoryInfo = null;
            try
            {
                _directoryInfo = new DirectoryInfo(_filePath);
                int count = _directoryInfo.GetDirectories().Count();
            }
            catch
            {
                return true;
            }
            return false;
        }
    }
}

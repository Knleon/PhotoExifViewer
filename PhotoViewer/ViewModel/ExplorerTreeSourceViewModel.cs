using PhotoViewer.Model;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoViewer.ViewModel
{
    public class ExplorerEventArgs:EventArgs
    {
        public string _directoryPath;
    }

    public class ExplorerTreeSourceViewModel : TreeViewItem, INotifyPropertyChanged
    {
        public DirectoryInfo _Directory { get; set; }
        private bool _Expanded { get; set; } = false;
        public bool IsDrive { get; set; } = false;

        // ExplorerTreeの更新情報を受け取る
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


        public delegate void ExplorerEventHandler(object _sender, ExplorerEventArgs e);
        public event ExplorerEventHandler ExplorerEvent;
        protected virtual void OnExplorerEvent(ExplorerEventArgs e)
        {
            if(ExplorerEvent != null)
            {
                ExplorerEvent(this, e);
            }
        }

        public ExplorerTreeSourceViewModel(string _path, bool _isDrive)
        {
            // イベントの設定
            Expanded += ExplorerTreeSource_Expanded;
            Selected += ExplorerTreeSource_Selected;

            // TreeViewItemのセット
            GetTreeViewItem(_path, _isDrive);
        }

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

        private void ExplorerTreeSource_Expanded(object _sender, RoutedEventArgs e)
        {
            if (!_Expanded)
            {
                Items.Clear();
                foreach(var _dirInfo in _Directory.GetDirectories())
                {
                    if (!IsDirectoryLocked(_dirInfo.FullName))
                    {
                        // ファイル名の最初の文字を取得
                        string _fileNameFirst = Path.GetFileName(_dirInfo.FullName).Substring(0, 1);
                        const string _tempRecycleFileIndicator = "$";
                       
                        // 最初の文字が”$”だった場合、Windowsの特殊ファイルのためスキップ
                        if(_fileNameFirst != _tempRecycleFileIndicator)
                        {
                            bool _isDrive = false;
                            var _node = new ExplorerTreeSourceViewModel(_dirInfo.FullName, _isDrive);
                            Items.Add(_node);
                        }
                    }
                }
            }
            _Expanded = true;
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
            if (_isDrive == true)
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
            OnExplorerEvent(_explorerEventArgs);
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

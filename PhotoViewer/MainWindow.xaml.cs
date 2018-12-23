using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PhotoViewer.Model;
using PhotoViewer.ViewModel;

namespace PhotoViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // MainWindowのViewModelの読み込み
            MainWindowViewModel _mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = _mainWindowViewModel;
        }

        /// <summary>
        /// ListBoxItemで左クリックしたときのメソッド
        /// </summary>
        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBoxItem _item = sender as ListBoxItem;
            MediaInfo _info = _item.DataContext as MediaInfo;

            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;

            // 元々の画像を読み込み中の場合
            if (_model.IsReadMedia)
            {
                // 現状は何もしない
                return;
            }
            else
            {
                _model.LoadViewImageSource(_info);
            }
        }

        /// <summary>
        /// ListBox上でマウスホイールしたときのメソッド
        /// </summary>
        private void ListBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ListBox _box = sender as ListBox;
            if (_box == null) return;

            var _border = VisualTreeHelper.GetChild(_box, 0) as Border;
            if (_border == null) return;

            var _viewer = VisualTreeHelper.GetChild(_border, 0) as ScrollViewer;
            if (_viewer == null) return;

            if (e.Delta > 0)
            {
                _viewer.LineUp();
            }
            else
            {
                _viewer.LineDown();
            }

            e.Handled = true;
        }

        /// <summary>
        /// 矢印キーを入力したときのメソッド
        /// </summary>
        private void ListBoxItem_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem _item = sender as ListBoxItem;
            MediaInfo _info = _item.DataContext as MediaInfo;

            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;

            // 元々の画像を読み込み中の場合
            if (_model.IsReadMedia)
            {
                // 現状は何もしない
                return;
            }
            else
            {
                _model.LoadViewImageSource(_info);
            }
        }

        /// <summary>
        /// Exif情報部分でマウススクロールしたときのメソッド
        /// </summary>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer _viewer = sender as ScrollViewer;
            if (_viewer == null) return;

            if (e.Delta > 0)
            {
                _viewer.LineLeft();
            }
            else
            {
                _viewer.LineRight();
            }

            e.Handled = true;
        }

        /// <summary>
        /// ListBoxItemでダブルクリックしたときのメソッド
        /// </summary>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem _item = sender as ListBoxItem;
            MediaInfo _info = _item.DataContext as MediaInfo;

            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;
            _model.MediaInfoListDoubleClicked(_info);
        }

        /// <summary>
        /// MenuItemをクリックしたとき
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var _menuItem = sender as MenuItem;

            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;
            _model.DoContextMenu(_menuItem);
        }

        /// <summary>
        /// ウィンドウを終了したとき
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 終了処理
            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;
            _model.StopThreadAndTask();
        }
    }
}

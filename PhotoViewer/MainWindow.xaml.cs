using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
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
        // 動画を再生中であるかどうかのフラグ
        private bool IsPlayMovie { get; set; }

        // 動画の再生が終了しているかどうかのフラグ
        private bool IsPlayEnded { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            DataContextChanged += (o, e) =>
            {
                MainWindowViewModel _vm = DataContext as MainWindowViewModel;

                if (_vm != null)
                {
                    _vm.ChangeSourceEvent += (sender, args) => { ChangeSourceBeforeExecute(); };
                }
            };

            // MainWindowのViewModelの読み込み
            MainWindowViewModel _mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = _mainWindowViewModel;

            // ウィンドウの位置・サイズ情報の復元
            LoadWindowPlacement();
        }

        //
        // ウィンドウ位置、サイズの保持
        //

        /// <summary>
        /// ウィンドウを閉じるときの処理
        /// </summary>
        /// <param name="e">引数情報</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                SaveWindowPlacement();
            }
        }

        /// <summary>
        /// ウィンドウ位置・サイズの保存
        /// </summary>
        private void SaveWindowPlacement()
        {
            // ウィンドウ位置とサイズの取得
            Properties.Settings.Default.WindowState = (this.WindowState == WindowState.Minimized) ? WindowState.Normal : this.WindowState;
            Properties.Settings.Default.Bounds = this.RestoreBounds;

            // 設定ファイルに保存
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// ウィンドウ位置・サイズの復元
        /// </summary>
        private void LoadWindowPlacement()
        {
            // 設定ファイルから読み込み
            Properties.Settings.Default.Reload();

            // 位置とサイズ情報を復元
            Rect _bounds = Properties.Settings.Default.Bounds;
            this.Left = _bounds.Left;
            this.Top = _bounds.Top;
            this.Width = _bounds.Width;
            this.Height = _bounds.Height;

            // ウィンドウの状態を復元(Normal or Maximized)
            this.WindowState = Properties.Settings.Default.WindowState;
        }

        /// <summary>
        /// ListBoxItemで左クリックしたときのメソッド
        /// </summary>
        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBoxItem _item = sender as ListBoxItem;
            MediaContentInfo _info = _item.DataContext as MediaContentInfo;

            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;

            // 元々の画像を読み込み中の場合
            if (_model.IsReadMedia)
            {
                // 何もしない
                return;
            }
            else
            {
                _model.LoadContentSource(_info);
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
            MediaContentInfo _info = _item.DataContext as MediaContentInfo;

            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;

            // 元々の画像を読み込み中の場合
            if (_model.IsReadMedia)
            {
                // 何もしない
                return;
            }
            else
            {
                _model.LoadContentSource(_info);
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
            MediaContentInfo _info = _item.DataContext as MediaContentInfo;

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
            _model.ExecuteContextMenu(_menuItem);
        }

        /// <summary>
        /// ウィンドウを終了したとき
        /// </summary>
        /// <param name="sender">MainWindow</param>
        /// <param name="e">引数情報</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 終了処理
            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;
            _model.StopThreadAndTask();
        }

        /// <summary>
        /// ウィンドウを移動したとき
        /// </summary>
        /// <param name="sender">MainWindow</param>
        /// <param name="e">引数情報</param>
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            // 現在のウィンドウの位置を取得
            WINDOWPLACEMENT _placement;
            var _hwnd = new WindowInteropHelper(this).Handle;
            WindowManager.GetWindowPlacement(_hwnd, out _placement);

            if (_placement.normalPosition.Left > SystemParameters.PrimaryScreenWidth)
            {
                // セカンドモニターにウィンドウがある場合
                // ソフトウェアレンダリングに切り替え
                // MediaElementがセカンドモニターでフリーズする問題の対策
                //
                var _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                if (_hwndSource != null)
                {
                    var _hwndTarget = _hwndSource.CompositionTarget;
                    if (_hwndTarget != null) _hwndTarget.RenderMode = RenderMode.SoftwareOnly;
                }
            }
            else
            {
                // プライマリモニターにウィンドウがある場合
                // ハードウェアレンダリングに切り替え
                //
                var _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                if (_hwndSource != null)
                {
                    var _hwndTarget = _hwndSource.CompositionTarget;
                    if (_hwndTarget != null) _hwndTarget.RenderMode = RenderMode.Default;
                }
            }
        }

        /// <summary>
        /// ソースを切り替える前に行う動作(MainWindowViewModelからのイベントで動作する)
        /// </summary>
        private void ChangeSourceBeforeExecute()
        {
            try
            {
                if (this.viewMovieElement.Source == null)
                {
                    return;
                }

                // 再生中または再生終了の場合
                if (IsPlayMovie || IsPlayEnded)
                {
                    // 再生停止
                    this.viewMovieElement.Stop();

                    IsPlayMovie = false;
                    IsPlayEnded = false;

                    return;
                }
            }
            catch
            {
                App.ShowErrorMessageBox("内部エラー", "内部でエラーが発生しました。");
                try
                {
                    this.viewMovieElement.Stop();
                }
                catch { }   // エラーからの回復なので、ここでのエラーは握りつぶす
            }
        }

        /// <summary>
        /// MediaElementをクリックした場合は再生または停止処理を行う
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">引数情報</param>
        private void ViewMovieElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel _model = this.DataContext as MainWindowViewModel;

            if (this.viewMovieElement.Source == null)
            {
                return;
            }

            // 動画の再生が終了している場合
            if (IsPlayEnded)
            {
                // 再生停止
                this.viewMovieElement.Stop();
                IsPlayEnded = false;

                // 再度、再生する
                this.viewMovieElement.Play();

                return;
            }

            if (IsPlayMovie)
            {
                // 動画再生中の場合はPauseを行う
                this.viewMovieElement.Pause();
                IsPlayMovie = false;
                return;
            }

            // 動画を再生する
            IsPlayMovie = true;
            this.viewMovieElement.Play();
        }

        /// <summary>
        /// 動画の再生が終了したときに実行する
        /// </summary>
        /// <param name="sender">MediaElement</param>
        /// <param name="e">引数情報</param>
        private void ViewMovieElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            IsPlayEnded = true;
        }

        /// <summary>
        /// 動画をロードしたときに実行する
        /// </summary>
        /// <param name="sender">MediaElement</param>
        /// <param name="e">引数情報</param>
        private void ViewMovieElement_Loaded(object sender, RoutedEventArgs e)
        {
            // 再生一時停止
            this.viewMovieElement.Pause();
        }
    }
}

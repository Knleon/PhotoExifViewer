using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace PhotoViewer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 多重起動禁止
        /// </summary>
        private Mutex mutex = new Mutex(false, "Photo_Viewer_App");
        protected override void OnStartup(StartupEventArgs e)
        {
            if(mutex.WaitOne(0, false) == false)
            {
                // すでに起動済みのため終了する
                mutex.Close();
                mutex = null;
                this.Shutdown();

                // 起動中の場合は起動中のアプリケーションを最前面に表示
                Process _prevProcess = GetPreviousProcess();
                if(_prevProcess != null && _prevProcess.MainWindowHandle != IntPtr.Zero)
                {
                    WakeupWindow(_prevProcess.MainWindowHandle);
                }
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if(mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
            base.OnExit(e);
        }

        /// <summary>
        /// 実行中の同じアプリケーションのプロセスを取得するメソッド
        /// </summary>
        public Process GetPreviousProcess()
        {
            Process curProcess = Process.GetCurrentProcess();
            Process[] allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

            foreach (Process checkProcess in allProcesses)
            {
                // 自分自身のプロセスIDは無視する
                if (checkProcess.Id != curProcess.Id)
                {
                    // プロセスのフルパス名を比較して同じアプリケーションか検証
                    if (String.Compare(
                            checkProcess.MainModule.FileName,
                            curProcess.MainModule.FileName, true) == 0)
                    {
                        // 同じフルパス名のプロセスを取得
                        return checkProcess;
                    }
                }
            }

            return null;
        }

        // 外部プロセスのメイン・ウィンドウを起動するためのWin32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        
        // ShowWindowAsync関数のパラメータに渡す定義値
        private const int SW_RESTORE = 9;  // 画面を元の大きさに戻す

        /// <summary>
        /// 外部プロセスのウィンドウを起動するメソッド
        /// </summary>
        public void WakeupWindow(IntPtr hWnd)
        {
            // メイン・ウィンドウが最小化されていれば元に戻す
            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SW_RESTORE);
            }

            // メイン・ウィンドウを最前面に表示する
            SetForegroundWindow(hWnd);
        }

        /// <summary>
        /// 現在のAppクラスのインスタンスを取得する
        /// </summary>
        public static new App Current
        {
            get { return Application.Current as App; }
        }

        /// <summary>
        /// 例外発生時はコンソールにエラーメッセージを出力する
        /// </summary>
        /// <param name="_ex">例外時のメッセージ</param>
        public static void LogException(Exception ex, 
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            Console.WriteLine("ERROR -> " + ex.Message + ", LineNumber:" + callerLineNumber + ", FilePath:" + callerFilePath);
        }

        /// <summary>
        /// エラーメッセージボックスを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="caption">タイトル</param>
        public static void ShowErrorMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 警告メッセージボックスを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="caption">タイトル</param>
        public static void ShowWarningMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 質問メッセージボックスを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="caption">タイトル</param>
        /// <returns>結果</returns>
        public static MessageBoxResult ShowQuestionMessageBox(string message, string caption)
        {
            return MessageBox.Show(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        }
    }

    public class WindowManager
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public SW showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    public enum SW
    {
        HIDE = 0,
        SHOWNORMAL = 1,
        SHOWMINIMIZED = 2,
        SHOWMAXIMIZED = 3,
        SHOWNOACTIVATE = 4,
        SHOW = 5,
        MINIMIZE = 6,
        SHOWMINNOACTIVE = 7,
        SHOWNA = 8,
        RESTORE = 9,
        SHOWDEFAULT = 10,
    }
}

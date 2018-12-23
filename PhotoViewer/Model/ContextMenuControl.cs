using Prism.Mvvm;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class ContextMenuControl:BindableBase
    {
        private string _displayName;
        /// <summary>
        /// コンテキストメニューに表示するアプリ名
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private BitmapSource _contextIcon;
        /// <summary>
        /// コンテキストメニューのアイコン情報
        /// </summary>
        public BitmapSource ContextIcon
        {
            get { return _contextIcon; }
            set { SetProperty(ref _contextIcon, value); }
        }

        // コンストラクタ
        public ContextMenuControl(){}
    }
}

using Prism.Mvvm;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class ContextMenuControl:BindableBase
    {
        // ContextMenuのHeader
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        // ContextMenuのIcon
        private BitmapSource _contextIcon;
        public BitmapSource ContextIcon
        {
            get { return _contextIcon; }
            set { SetProperty(ref _contextIcon, value); }
        }

        // コンストラクタ
        public ContextMenuControl()
        {

        }
    }
}

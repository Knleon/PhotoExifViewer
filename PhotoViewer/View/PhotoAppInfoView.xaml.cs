using PhotoViewer.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PhotoViewer.View
{
    /// <summary>
    /// Interaction logic for PhotoAppInfoView.xaml
    /// </summary>
    public partial class PhotoAppInfoView : Window
    {
        public PhotoAppInfoView()
        {
            InitializeComponent();

            PhotoAppInfoListView.ItemsSource = new String[]
            {
                "連携アプリ設定", "情報"
            };

            // デフォルト表示設定
            PhotoAppInfoListView.SelectedIndex = 0;
            PhotoAppInfoListView.Loaded += new RoutedEventHandler(PhotoAppInfoListView_Loaded);
        }

        private void PhotoAppInfoListView_SelectionChanged(object _sender, SelectionChangedEventArgs _e)
        {
            switch (PhotoAppInfoListView.SelectedIndex)
            {
                case 0:
                    var _linkageProgramView = new LinkageProgramView();
                    LinkageProgramViewModel _linkageProgramViewModel = new LinkageProgramViewModel();
                    _linkageProgramView.DataContext = _linkageProgramViewModel;
                    _Frame.Navigate(_linkageProgramView);
                    break;
                case 1:
                    _Frame.Navigate(new InformationView());
                    break;
                default:
                    break;
            }
        }

        private void PhotoAppInfoListView_Loaded(object _sender, RoutedEventArgs _e)
        {
            if(PhotoAppInfoListView.SelectedIndex >= 0)
            {
                ListViewItem _item = PhotoAppInfoListView.ItemContainerGenerator.ContainerFromItem(PhotoAppInfoListView.SelectedItem) as ListViewItem;
                _item.Focus();
            }
            PhotoAppInfoListView.Loaded -= new RoutedEventHandler(PhotoAppInfoListView_Loaded);
        }
    }
}

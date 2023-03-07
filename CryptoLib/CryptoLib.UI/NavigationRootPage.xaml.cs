using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Frame = ModernWpf.Controls.Frame;

namespace CryptoLib.UI
{
    /// <summary>
    /// NavigationRootPage.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationRootPage : Page
    {
        public static Frame? RootFrame { get; set; }
        private bool _ignoreSelectionChange;
        private Type? _startPage = null;
        public NavigationRootPage()
        {
            InitializeComponent();
            RootFrame = rootFrame;
            if (_startPage != null)
            {
                PagesList.SelectedItem = PagesList.Items.OfType<ControlInfoDataItem>().FirstOrDefault(x => x.PageType == _startPage);
            }

            NavigateToSelectedPage();
        }

        partial void SetStartPage();

        private void NavigateToSelectedPage()
        {
            if (PagesList.SelectedValue is Type type)
            {
                RootFrame?.Navigate(type);
            }
        }

        private void PagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ignoreSelectionChange)
            {
                NavigateToSelectedPage();
            }
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                RootFrame?.RemoveBackEntry();
            }
        }
        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            _ignoreSelectionChange = true;
            PagesList.SelectedValue = RootFrame?.CurrentSourcePageType;
            _ignoreSelectionChange = false;
        }
    }
}

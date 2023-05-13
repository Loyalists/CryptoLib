using CryptoLib.UI.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class NavigationRootPage : Page
    {
        public static Frame? RootFrame { get; set; }
        private bool _ignoreSelectionChange;
        private Type? _startPage = null;

        //public Dictionary<Type, Page> PageInstances = new Dictionary<Type, Page>();
        public ObservableCollection<ControlInfoDataItem> Pages = new ObservableCollection<ControlInfoDataItem>()
        {
            new ControlInfoDataItem(typeof(RSAPage), "RSA"),
            new ControlInfoDataItem(typeof(DESPage), "DES"),
        };

        public NavigationRootPage()
        {
            InitializeComponent();
            PagesList.DataContext = Pages;
            //foreach (var item in PagesList.Items.OfType<ControlInfoDataItem>())
            //{
            //    Page? instance = Activator.CreateInstance(item.PageType) as Page;
            //    if (instance == null)
            //    {
            //        throw new Exception();
            //    }

            //    PageInstances.Add(item.PageType, instance);
            //}

            RootFrame = rootFrame;
            if (_startPage != null)
            {
                PagesList.SelectedItem = PagesList.Items.OfType<ControlInfoDataItem>().FirstOrDefault(x => x.PageType == _startPage);
            }

            NavigateToSelectedPage();
        }

        private void NavigateToSelectedPage()
        {
            if (PagesList.SelectedValue is Type type)
            {
                //RootFrame?.Navigate(PageInstances[type]);
                int index = PagesList.SelectedIndex;
                RootFrame?.Navigate(Pages[index].Instance);
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

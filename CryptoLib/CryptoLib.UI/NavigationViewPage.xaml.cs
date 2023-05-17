using CryptoLib.UI.Pages;
using ModernWpf.Controls;
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
using Page = System.Windows.Controls.Page;

namespace CryptoLib.UI
{
    public partial class NavigationViewPage : Page
    {
        public Dictionary<string, Page> PageInstances = new Dictionary<string, Page>()
        {
            { "RSA", new RSAPage() },
            { "DES", new DESPage() },
            { "3DES", new TDESPage() },
        };

        public NavigationViewPage()
        {
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer.Tag == null)
            {
                return;
            }

            var navItemTag = args.SelectedItemContainer.Tag.ToString();
            if (navItemTag == null)
            {
                return;
            }

            Page? instance = PageInstances.GetValueOrDefault(navItemTag);
            rootFrame.Navigate(instance);
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                rootFrame?.RemoveBackEntry();
            }
        }
    }
}

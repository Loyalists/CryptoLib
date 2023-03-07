using CryptoLib.UI.Pages;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace CryptoLib.UI
{
    public class ControlPagesData : List<ControlInfoDataItem>
    {
        public ControlPagesData()
        {
            Add(new ControlInfoDataItem(typeof(RSAPage), "RSA"));
            Add(new ControlInfoDataItem(typeof(DESPage), "DES"));
        }
    }

    public class ControlInfoDataItem
    {
        public Type PageType { get; }
        public string Title { get; }

        public ControlInfoDataItem(Type pageType, string title)
        {
            PageType = pageType;
            Title = title;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}

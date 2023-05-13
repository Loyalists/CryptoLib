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
    public class ControlInfoDataItem
    {
        public Type PageType { get; }
        public string Title { get; }
        public object? Instance { get; }

        public ControlInfoDataItem(Type pageType, string title)
        {
            PageType = pageType;
            Title = title;
            Instance = Activator.CreateInstance(pageType);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}

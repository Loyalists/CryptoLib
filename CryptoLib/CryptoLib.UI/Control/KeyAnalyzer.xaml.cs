using CryptoLib.Algorithm.Key;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

namespace CryptoLib.UI.Control
{
    public partial class KeyAnalyzer : ContentDialog
    {
        public KeyAnalyzer(IKey key)
        {
            InitializeComponent();
            AddComponents(key);
        }

        public void AddComponents(IKey key)
        {
            AddComponent("Key Size", key.GetKeySize().ToString());
            Type type = key.GetType();
            PropertyInfo[] infos = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (PropertyInfo info in infos)
            {
                object? _value = info.GetValue(key);
                if (_value != null)
                {
                    string? value = _value.ToString();
                    AddComponent(info.Name, value);
                }
            }

        }

        public void AddComponent(string header, string? value)
        {
            var control = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap
            };
            ControlHelper.SetHeader(control, header);
            control.Text = value;
            ResultPanel.Children.Add(control);
        }
    }
}

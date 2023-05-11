using CryptoLib.Algorithm.Key;
using CryptoLib.Service;
using CryptoLib.Service.Format;
using CryptoLib.Service.Padding;
using CryptoLib.UI.Utility;
using CryptoLib.Utility;
using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace CryptoLib.UI.Pages
{
    /// <summary>
    /// RSAPage.xaml 的交互逻辑
    /// </summary>
    public partial class RSAPage : INotifyPropertyChanged
    {
        public int DefaultKeySize = 1024;
        public List<int> KeySizeList { get; } = new List<int>() { 
            128, 
            256, 
            512, 
            1024, 
            2048, 
            4096,
        };
        public IEnumerable<RSAPublicKeyFormat> PublicKeyFormatTypes
        {
            get
            {
                return Enum.GetValues(typeof(RSAPublicKeyFormat)).Cast<RSAPublicKeyFormat>();
            }
        }

        public IEnumerable<RSAPrivateKeyFormat> PrivateKeyFormatTypes
        {
            get
            {
                return Enum.GetValues(typeof(RSAPrivateKeyFormat)).Cast<RSAPrivateKeyFormat>();
            }
        }

        public IEnumerable<RSAPaddingScheme> PaddingSchemes
        {
            get
            {
                return Enum.GetValues(typeof(RSAPaddingScheme)).Cast<RSAPaddingScheme>();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RSAPage()
        {
            InitializeComponent();
            KeySizeComboBox.SelectedItem = KeySizeList.FirstOrDefault(x => x == DefaultKeySize);
            PublicKeyFormatComboBox.SelectedItem = PublicKeyFormatTypes.FirstOrDefault();
            PrivateKeyFormatComboBox.SelectedItem = PrivateKeyFormatTypes.FirstOrDefault();
            PaddingSchemeComboBox.SelectedItem = PaddingSchemes.FirstOrDefault();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            int keySize = KeySizeList[KeySizeComboBox.SelectedIndex];
            var selectedPublicKeyFormat = PublicKeyFormatTypes.ElementAt(PublicKeyFormatComboBox.SelectedIndex);
            IKeyFormat publicKeyFormat = RSAKeyFormatType.CreatePublicKeyFormatInstance(selectedPublicKeyFormat);

            var selectedPrivateKeyFormat = PrivateKeyFormatTypes.ElementAt(PrivateKeyFormatComboBox.SelectedIndex);
            IKeyFormat privateKeyFormat = RSAKeyFormatType.CreatePrivateKeyFormatInstance(selectedPrivateKeyFormat);

            RSAService service = new RSAService();
            service.KeySize = keySize;
            var keysTask = service.GenerateAsync();
            GenerateButton.IsEnabled = false;
            string buttonText = (string)GenerateButton.Content;
            GenerateButton.Content = "Generating...";
            var keys = await keysTask;
            GenerateButton.Content = buttonText;
            GenerateButton.IsEnabled = true;

            var publicKey = (RSAPublicKey)keys[RSAKeyType.PublicKey];
            var privateKey = (RSAPrivateKey)keys[RSAKeyType.PrivateKey];

            bool formatted = FormattedCheckBox.IsChecked == true;
            PublicKeyTextBox.Text = publicKey.ToString(publicKeyFormat, formatted);
            PrivateKeyTextBox.Text = privateKey.ToString(privateKeyFormat, formatted);
        }

        private async void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string plainText = PlainTextBox.Text;
            if (string.IsNullOrEmpty(plainText))
            {
                return;
            }

            string publicKeyText = PublicKeyTextBox.Text;
            var selectedPublicKeyFormat = PublicKeyFormatTypes.ElementAt(PublicKeyFormatComboBox.SelectedIndex);
            IKeyFormat publicKeyFormat = RSAKeyFormatType.CreatePublicKeyFormatInstance(selectedPublicKeyFormat);

            RSAPublicKey? publicKey;
            try
            {
                publicKey = RSAPublicKey.FromString(publicKeyText, publicKeyFormat);
            }
            catch (Exception ex) 
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }

            var selectedPaddingScheme = PaddingSchemes.ElementAt(PaddingSchemeComboBox.SelectedIndex);
            int keySize = publicKey.GetKeySize();
            RSAService service = new RSAService();
            service.Padding = selectedPaddingScheme;
            service.KeySize = keySize;
            try
            {
                string encryptedText = service.Encrypt(plainText, publicKey);
                EncryptedTextBox.Text = encryptedText;
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            string encryptedText = EncryptedTextBox.Text;
            if (string.IsNullOrEmpty(encryptedText))
            {
                return;
            }

            string privateKeyText = PrivateKeyTextBox.Text;
            var selectedPrivateKeyFormat = PrivateKeyFormatTypes.ElementAt(PrivateKeyFormatComboBox.SelectedIndex);
            IKeyFormat privateKeyFormat = RSAKeyFormatType.CreatePrivateKeyFormatInstance(selectedPrivateKeyFormat);

            RSAPrivateKey? privateKey;
            try
            {
                privateKey = RSAPrivateKey.FromString(privateKeyText, privateKeyFormat);
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }

            var selectedPaddingScheme = PaddingSchemes.ElementAt(PaddingSchemeComboBox.SelectedIndex);
            int keySize = privateKey.GetKeySize();
            RSAService service = new RSAService();
            service.Padding = selectedPaddingScheme;
            service.KeySize = keySize;

            try
            {
                string decryptedText = service.Decrypt(encryptedText, privateKey);
                PlainTextBox.Text = decryptedText;
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void OpenPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "PEM|*.pem";
                //dialog.FileName = "PublicKey";
                string path;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                path = dialog.FileName;

                bool formatted = FormattedCheckBox.IsChecked == true;
                var selectedFormat = PublicKeyFormatTypes.ElementAt(PublicKeyFormatComboBox.SelectedIndex);
                IKeyFormat format = RSAKeyFormatType.CreatePublicKeyFormatInstance(selectedFormat);

                string keyText;
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    keyText = sr.ReadToEnd();
                }

                RSAPublicKey key = RSAPublicKey.FromString(keyText, format);
                PublicKeyTextBox.Text = key.ToString(format, formatted);
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void SavePublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "PEM|*.pem";
                dialog.FileName = "PublicKey";
                string path;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                path = dialog.FileName;

                bool formatted = FormattedCheckBox.IsChecked == true;
                string keyText = PublicKeyTextBox.Text;
                var selectedFormat = PublicKeyFormatTypes.ElementAt(PublicKeyFormatComboBox.SelectedIndex);
                IKeyFormat format = RSAKeyFormatType.CreatePublicKeyFormatInstance(selectedFormat);
                RSAPublicKey key = RSAPublicKey.FromString(keyText, format);
                string keystr = key.ToString(format, formatted);

                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(keystr);
                }
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void OpenPrivateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "PEM|*.pem";
                //dialog.FileName = "PublicKey";
                string path;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                path = dialog.FileName;

                bool formatted = FormattedCheckBox.IsChecked == true;
                var selectedFormat = PrivateKeyFormatTypes.ElementAt(PrivateKeyFormatComboBox.SelectedIndex);
                IKeyFormat format = RSAKeyFormatType.CreatePrivateKeyFormatInstance(selectedFormat);

                string keyText;
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    keyText = sr.ReadToEnd();
                }

                RSAPrivateKey key = RSAPrivateKey.FromString(keyText, format);
                PrivateKeyTextBox.Text = key.ToString(format, formatted);
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void SavePrivateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "PEM|*.pem";
                dialog.FileName = "PrivateKey";
                string path;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                path = dialog.FileName;

                bool formatted = FormattedCheckBox.IsChecked == true;
                string keyText = PrivateKeyTextBox.Text;
                var selectedFormat = PrivateKeyFormatTypes.ElementAt(PrivateKeyFormatComboBox.SelectedIndex);
                IKeyFormat format = RSAKeyFormatType.CreatePrivateKeyFormatInstance(selectedFormat);
                RSAPrivateKey key = RSAPrivateKey.FromString(keyText, format);
                string keystr = key.ToString(format, formatted);

                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(keystr);
                }
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }
    }
}

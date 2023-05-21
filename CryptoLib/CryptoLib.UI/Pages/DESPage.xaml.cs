using CryptoLib.Algorithm.Key;
using CryptoLib.Service;
using CryptoLib.Service.Format;
using CryptoLib.Service.Mode;
using CryptoLib.Service.Padding;
using CryptoLib.UI.Utility;
using CryptoLib.Utility;
using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
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
    public partial class DESPage
    {
        public DESPaddingScheme DefaultPaddingScheme = DESPaddingScheme.PKCS5;
        public BlockCipherMode DefaultBlockCipherMode = BlockCipherMode.ECB;

        public List<string> KeyFormats { get; } = new List<string>() {
            "Hex",
            "Base64",
        };

        public IEnumerable<DESPaddingScheme> PaddingSchemes
        {
            get
            {
                return Enum.GetValues(typeof(DESPaddingScheme)).Cast<DESPaddingScheme>();
            }
        }

        public IEnumerable<BlockCipherMode> Modes
        {
            get
            {
                return Enum.GetValues(typeof(BlockCipherMode)).Cast<BlockCipherMode>();
            }
        }

        public List<string> TextFormats { get; } = new List<string>() {
            "Base64",
            "Hex",
        };

        public DESPage()
        {
            InitializeComponent();
            KeyFormatComboBox.SelectedItem = KeyFormats.FirstOrDefault();
            PaddingSchemeComboBox.SelectedItem = PaddingSchemes.FirstOrDefault(x => x == DefaultPaddingScheme);
            ModeComboBox.SelectedItem = Modes.FirstOrDefault(x => x == DefaultBlockCipherMode);
            TextFormatComboBox.SelectedItem = TextFormats.FirstOrDefault();
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string passphrase = PassphraseTextBox.Text;
            if (string.IsNullOrEmpty(passphrase))
            {
                await UIHelper.ShowSimpleDialog("To generate a DES key, fill in the Passphrase textbox first.");
                return;
            }

            try
            {
                byte[]? salt = null;
                if (!string.IsNullOrEmpty(SaltForKeyGeneratorTextBox.Text))
                {
                    salt = Convert.FromHexString(SaltForKeyGeneratorTextBox.Text);
                }

                DESService service = new DESService();
                service.Passphrase = passphrase;
                service.Salt = salt;
                var keysTask = service.GenerateAsync();
                GenerateButton.IsEnabled = false;
                string buttonText = (string)GenerateButton.Content;
                GenerateButton.Content = "Generating...";
                var keys = await keysTask;
                GenerateButton.Content = buttonText;
                GenerateButton.IsEnabled = true;

                var key = (DESKey)keys[DESKeyType.Key];
                if (key.Salt == null || key.IV == null) 
                {
                    throw new Exception();
                }

                SetKey(key);
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void GenerateIVButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFormat = KeyFormats.ElementAt(KeyFormatComboBox.SelectedIndex);
            try
            {
                byte[] iv = CryptoHelper.GenerateIV(DESService.IVSize);
                if (selectedKeyFormat == "Hex")
                {
                    IVTextBox.Text = Convert.ToHexString(iv);
                }
                else
                {
                    IVTextBox.Text = Convert.ToBase64String(iv);
                }
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string plainText = PlainTextBox.Text;
            if (string.IsNullOrEmpty(plainText))
            {
                return;
            }

            var selectedMode = Modes.ElementAt(ModeComboBox.SelectedIndex);
            var selectedPadding = PaddingSchemes.ElementAt(PaddingSchemeComboBox.SelectedIndex);
            var selectedTextFormat = TextFormats.ElementAt(TextFormatComboBox.SelectedIndex);

            if (selectedPadding == DESPaddingScheme.None)
            {
                if (selectedMode == BlockCipherMode.ECB || selectedMode == BlockCipherMode.CBC)
                {
                    await UIHelper.ShowSimpleDialog("ECB and CBC cipher mode require a valid padding scheme to function!");
                    return;
                }
            }

            try
            {
                DESService service = new DESService();
                service.CipherMode = selectedMode;
                service.Padding = selectedPadding;
                DESKey key = GetKey();

                string encryptedText = service.Encrypt(plainText, key);
                string converted = encryptedText;
                if (selectedTextFormat == "Hex")
                {
                    byte[] bytes = Convert.FromBase64String(encryptedText);
                    converted = Convert.ToHexString(bytes);
                }
                EncryptedTextBox.Text = converted;
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

            var selectedMode = Modes.ElementAt(ModeComboBox.SelectedIndex);
            var selectedPadding = PaddingSchemes.ElementAt(PaddingSchemeComboBox.SelectedIndex);
            var selectedTextFormat = TextFormats.ElementAt(TextFormatComboBox.SelectedIndex);

            if (selectedPadding == DESPaddingScheme.None)
            {
                if (selectedMode == BlockCipherMode.ECB || selectedMode == BlockCipherMode.CBC)
                {
                    await UIHelper.ShowSimpleDialog("ECB and CBC cipher mode require a valid padding scheme to function!");
                    return;
                }
            }

            try
            {
                DESService service = new DESService();
                service.CipherMode = selectedMode;
                service.Padding = selectedPadding;
                DESKey key = GetKey();

                string converted = encryptedText;
                if (selectedTextFormat == "Hex")
                {
                    byte[] bytes = Convert.FromHexString(converted);
                    converted = Convert.ToBase64String(bytes);
                }
                string decryptedText = service.Decrypt(converted, key);
                PlainTextBox.Text = decryptedText;
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private DESKey GetKey()
        {
            if (string.IsNullOrEmpty(KeyTextBox.Text))
            {
                throw new Exception("null or empty key");
            }

            var selectedKeyFormat = KeyFormats.ElementAt(KeyFormatComboBox.SelectedIndex);
            byte[] bytes;
            byte[] salt;
            byte[] iv;
            if (selectedKeyFormat == "Hex")
            {
                bytes = Convert.FromHexString(KeyTextBox.Text);
                salt = Convert.FromHexString(SaltTextBox.Text);
                iv = Convert.FromHexString(IVTextBox.Text);
            }
            else
            {
                bytes = Convert.FromBase64String(KeyTextBox.Text);
                salt = Convert.FromBase64String(SaltTextBox.Text);
                iv = Convert.FromBase64String(IVTextBox.Text);
            }

            var key = new DESKey(bytes);
            key.Salt = salt;
            key.IV = iv;

            return key;
        }

        private void SetKey(DESKey key)
        {
            SaltTextBox.Text = string.Empty;
            IVTextBox.Text = string.Empty;
            var selectedKeyFormat = KeyFormats.ElementAt(KeyFormatComboBox.SelectedIndex);
            if (selectedKeyFormat == "Hex")
            {
                KeyTextBox.Text = key.ToString(null);
                if (key.Salt != null)
                {
                    SaltTextBox.Text = Convert.ToHexString(key.Salt);
                }

                if (key.IV != null)
                {
                    IVTextBox.Text = Convert.ToHexString(key.IV);
                }
            }
            else
            {
                KeyTextBox.Text = Convert.ToBase64String(key.Bytes);
                if (key.Salt != null)
                {
                    SaltTextBox.Text = Convert.ToBase64String(key.Salt);
                }

                if (key.IV != null)
                {
                    IVTextBox.Text = Convert.ToBase64String(key.IV);
                }
            }
        }

        private async void OpenKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "JSON|*.json";
                string path;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                path = dialog.FileName;

                string keyText;
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    keyText = sr.ReadToEnd();
                }

                DESKey? key = JsonSerializer.Deserialize<DESKey>(keyText);
                if (key == null)
                {
                    throw new Exception();
                }

                SetKey(key);
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void SaveKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "JSON|*.json";
                dialog.FileName = "DESKey";
                string path;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                path = dialog.FileName;

                DESKey key = GetKey();
                string keystr = JsonSerializer.Serialize(key);
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

        private async void EncryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedMode = Modes.ElementAt(ModeComboBox.SelectedIndex);
            var selectedPadding = PaddingSchemes.ElementAt(PaddingSchemeComboBox.SelectedIndex);

            if (selectedPadding == DESPaddingScheme.None)
            {
                if (selectedMode == BlockCipherMode.ECB || selectedMode == BlockCipherMode.CBC)
                {
                    await UIHelper.ShowSimpleDialog("ECB and CBC cipher mode require a valid padding scheme to function!");
                    return;
                }
            }

            try
            {
                var dialog = new OpenFileDialog();
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                string path = dialog.FileName;

                var dialog_save = new SaveFileDialog();
                if (dialog_save.ShowDialog() != true)
                {
                    return;
                }
                string path_save = dialog_save.FileName;

                DESService service = new DESService();
                service.CipherMode = selectedMode;
                service.Padding = selectedPadding;
                DESKey key = GetKey();

                var sw = Stopwatch.StartNew();
                EncryptFileButton.IsEnabled = false;
                string buttonText = (string)EncryptFileButton.Content;
                EncryptFileButton.Content = "Encrypting...";
                await Task.Run(() =>
                {
                    byte[] plainData = File.ReadAllBytes(path);
                    byte[] encrypted = service.Encrypt(plainData, key);
                    using (var stream = File.Open(path_save, FileMode.Create))
                    {
                        using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                        {
                            writer.Write(encrypted);
                        }
                    }
                });
                EncryptFileButton.Content = buttonText;
                EncryptFileButton.IsEnabled = true;
                sw.Stop();
                double time = sw.Elapsed.TotalSeconds;

                await UIHelper.ShowSimpleDialog($"The encrypted file has been saved in {path_save}. Time elapsed: {time}");
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }

        private async void DecryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedMode = Modes.ElementAt(ModeComboBox.SelectedIndex);
            var selectedPadding = PaddingSchemes.ElementAt(PaddingSchemeComboBox.SelectedIndex);

            if (selectedPadding == DESPaddingScheme.None)
            {
                if (selectedMode == BlockCipherMode.ECB || selectedMode == BlockCipherMode.CBC)
                {
                    await UIHelper.ShowSimpleDialog("ECB and CBC cipher mode require a valid padding scheme to function!");
                    return;
                }
            }

            try
            {
                var dialog = new OpenFileDialog();
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                string path = dialog.FileName;

                var dialog_save = new SaveFileDialog();
                if (dialog_save.ShowDialog() != true)
                {
                    return;
                }
                string path_save = dialog_save.FileName;

                DESService service = new DESService();
                service.CipherMode = selectedMode;
                service.Padding = selectedPadding;
                DESKey key = GetKey();

                var sw = Stopwatch.StartNew();
                DecryptFileButton.IsEnabled = false;
                string buttonText = (string)DecryptFileButton.Content;
                DecryptFileButton.Content = "Decrypting...";
                await Task.Run(() =>
                {
                    byte[] encrypted = File.ReadAllBytes(path);
                    byte[] decrypted = service.Decrypt(encrypted, key);
                    using (var stream = File.Open(path_save, FileMode.Create))
                    {
                        using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                        {
                            writer.Write(decrypted);
                        }
                    }
                });
                DecryptFileButton.Content = buttonText;
                DecryptFileButton.IsEnabled = true;
                sw.Stop();
                double time = sw.Elapsed.TotalSeconds;

                await UIHelper.ShowSimpleDialog($"The decrypted file has been saved in {path_save}. Time elapsed: {time}");
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }
    }
}

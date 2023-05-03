using CryptoLib.Algorithm.Key;
using CryptoLib.Service;
using CryptoLib.Service.Format;
using CryptoLib.Service.Mode;
using CryptoLib.Service.Padding;
using CryptoLib.UI.Utility;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
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
                DESService service = new DESService();
                service.Passphrase = passphrase;
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

                KeyTextBox.Text = key.ToString(null);
                SaltTextBox.Text = Convert.ToHexString(key.Salt);
                IVTextBox.Text = Convert.ToHexString(key.IV);
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

                byte[] bytes = Convert.FromHexString(KeyTextBox.Text);
                byte[] salt = Convert.FromHexString(SaltTextBox.Text);
                byte[] iv = Convert.FromHexString(IVTextBox.Text);
                var key = new DESKey(bytes);
                key.Salt = salt;
                key.IV = iv;

                string encryptedText = service.Encrypt(plainText, key);
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
                DESService service = new DESService();
                service.CipherMode = selectedMode;
                service.Padding = selectedPadding;

                byte[] bytes = Convert.FromHexString(KeyTextBox.Text);
                byte[] salt = Convert.FromHexString(SaltTextBox.Text);
                byte[] iv = Convert.FromHexString(IVTextBox.Text);
                var key = new DESKey(bytes);
                key.Salt = salt;
                key.IV = iv;

                string decryptedText = service.Decrypt(encryptedText, key);
                PlainTextBox.Text = decryptedText;
            }
            catch (Exception ex)
            {
                await UIHelper.ShowSimpleDialog(ex.ToString());
                return;
            }
        }
    }
}

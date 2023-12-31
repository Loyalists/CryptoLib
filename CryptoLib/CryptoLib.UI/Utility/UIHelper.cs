﻿using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib.UI.Utility
{
    public static class UIHelper
    {
        public static async Task ShowSimpleDialog(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Content = message,
                CloseButtonText = "Ok"
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception)
            {

            }
        }
    }
}

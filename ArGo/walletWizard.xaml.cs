using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ArGo.src;
using Microsoft.Win32;
using NBitcoin;
using Xceed.Wpf.Toolkit.Core;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for walletWizard.xaml
    /// </summary>
    public partial class walletWizard : Window
    {
        public walletWizard()
        {
            InitializeComponent();
            var walletPath = GetNextAvailableFilename();
            ChooseTextBox.Text = Path.GetFileNameWithoutExtension(walletPath);
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wallet Files|*.json";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                              Path.DirectorySeparatorChar + "ArGo";
            if (openFileDialog.ShowDialog() == true)
                if (File.Exists(openFileDialog.FileName))
                {
                    new walletPasswordWindow(openFileDialog.FileName).Show();
                    Close();
                }
        }

        private void walletwizard_PageChanged(object sender, RoutedEventArgs e)
        {
            var WalletDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            Path.DirectorySeparatorChar + "ArGo";
            if (walletwizard.CurrentPage == NewSeedPage)
            {
                var walletPath = string.Format("{0}{1}{2}.json", WalletDir, Path.DirectorySeparatorChar,
                    ChooseTextBox.Text);
                Mnemonic mnemonic;
                var wallet = walletManagement.Create(out mnemonic, NewSeedPasswordBox1.Password, walletPath,
                    Network.Main);
                NewSeedTextBox.Text = mnemonic.ToString();

                MainWindow.Model = new walletViewModel(NewSeedPasswordBox1.Password, walletPath);
                MainWindow.Model.Update();
            }

            if (walletwizard.CurrentPage == WalletHasRecovered)
                try
                {
                    var walletPath = string.Format("{0}{1}{2}.json", WalletDir, Path.DirectorySeparatorChar,
                        ChooseTextBox.Text);
                    var wallet = walletManagement.Recover(new Mnemonic(ExistingSeedTextBox.Text),
                        ExistingSeedPasswordBox.Password, walletPath, Network.Main);

                    MainWindow.Model = new walletViewModel(ExistingSeedPasswordBox.Password, walletPath);
                    MainWindow.Model.Update();
                }
                catch
                {
                    MessageBox.Show("Incorect Seed Or Password", "Warning");
                }
        }

        private void NewSeedPasswordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (NewSeedPasswordBox1.Password == NewSeedPasswordBox2.Password) NewPasswordPage.CanSelectNextPage = true;
            else
                NewPasswordPage.CanSelectNextPage = false;
        }

        private void NewSeedPasswordBox2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (NewSeedPasswordBox2.Password == NewSeedPasswordBox1.Password) NewPasswordPage.CanSelectNextPage = true;
            else
                NewPasswordPage.CanSelectNextPage = false;
        }

        private void ExistingSeedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var Wordscount = ExistingSeedTextBox.Text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                .Count();
            if (Wordscount >= 15) ExistingSeedPage.CanSelectNextPage = true;
            else
                ExistingSeedPage.CanSelectNextPage = false;
        }

        public static string GetNextAvailableFilename()
        {
            var defaultfilename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                  Path.DirectorySeparatorChar + "ArGo" + Path.DirectorySeparatorChar +
                                  "default_wallet.json";
            if (!File.Exists(defaultfilename)) return defaultfilename;

            string alternateFilename;
            var fileNameIndex = 0;
            do
            {
                fileNameIndex += 1;
                alternateFilename = string.Format("{0}{1}wallet_{2}{3}",
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar +
                    "ArGo", Path.DirectorySeparatorChar, fileNameIndex, Path.GetExtension(defaultfilename));
            } while (File.Exists(alternateFilename));

            return alternateFilename;
        }

        private void walletwizard_Finish(object sender, CancelRoutedEventArgs e)
        {
            new MainWindow().Show();
        }
    }
}
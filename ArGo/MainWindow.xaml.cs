using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows;
using ArGo.src;
using Info.Blockchain.API.BlockExplorer;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BlockExplorer explorer;

        public MainWindow()
        {
            InitializeComponent();

            if (walletPasswordCancel)
            {
                Model = null;
                Backup_Wallet.Visibility = Visibility.Collapsed;
                Delete.Visibility = Visibility.Collapsed;
                Tools.Visibility = Visibility.Collapsed;
                Wallet.Visibility = Visibility.Collapsed;
            }

            else
            {
                if (Model != null)
                {
                    MainFrame.NavigationService.Navigate(new overview(Model));
                    explorer = new BlockExplorer();

                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;

                        if (CheckForInternetConnection())
                        {
                            walletData.Serialize(Model.Wallet);
                            Model.Update();
                        }

                        while (true)
                        {
                            if (CheckForInternetConnection())
                            {
                                var Addresses = explorer.GetMultiAddressAsync(Model.Addresses).Result.Addresses;
                                var TxsCount = Model.TxRecords == null ? 0 : Model.TxRecords.Count;
                                if (explorer.GetMultiAddressAsync(Model.Addresses).Result.Transactions.Count() >
                                    TxsCount)
                                {
                                    walletData.Serialize(Model.Wallet);
                                    Model.Update();
                                }

                                foreach (var tx in Model.TxRecords)
                                    if (tx.lockTime < 0)
                                    {
                                        var txAsync = explorer.GetTransactionByHashAsync(tx.hash).Result;
                                        if (txAsync.BlockHeight > 0)
                                        {
                                            var data = JsonConvert.DeserializeObject<Data>(
                                                File.ReadAllText(walletFileSerializer
                                                    .Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath));
                                            data.txData[tx.hash].lockTime = txAsync.BlockHeight;
                                            File.WriteAllText(
                                                walletFileSerializer.Deserialize(Model.Wallet.WalletFilePath)
                                                    .walletTransactionsPath, JsonConvert.SerializeObject(data));
                                            Model.Update();
                                        }
                                    }

                                using (var client = new HttpClient())
                                {
                                    const string url = @"https://api.blockcypher.com/v1/btc/main";
                                    var result = client.GetAsync(url, HttpCompletionOption.ResponseContentRead).Result;
                                    var asyncData = new FileInfo(Model.Wallet.WalletFilePath).Directory.FullName +
                                                    Path.DirectorySeparatorChar + "asyncData.json";
                                    if (result.IsSuccessStatusCode)
                                        File.WriteAllText(asyncData, result.Content.ReadAsStringAsync().Result);
                                    Model.Update();
                                }
                            }

                            Thread.Sleep(15000);
                        }
                    }).Start();
                }

                else
                {
                    var defaultWallet = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        Path.DirectorySeparatorChar + "ArGo" + Path.DirectorySeparatorChar +
                                        "default_wallet.json";
                    if (File.Exists(defaultWallet))
                    {
                        var walletPasswordWindow = new walletPasswordWindow(defaultWallet);
                        walletPasswordWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        walletPasswordWindow.Show();
                        Close();
                    }
                    else
                    {
                        var walletWizard = new walletWizard();
                        walletWizard.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        walletWizard.Show();
                        Close();
                    }
                }
            }
        }

        public static walletViewModel Model { set; get; }
        public static bool walletPasswordCancel { set; get; }

        private void OverView_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
                MainFrame.NavigationService.Navigate(new overview(Model));
            else
                MessageBox.Show("Please Selecte Wallet File", "Warning");
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
                if (Model.asyncData != null)
                    MainFrame.NavigationService.Navigate(new send(Model));
                else
                    MessageBox.Show("No internet connection To estimate Bitcoin fee", "Warning");
            else
                MessageBox.Show("Please Selecte Wallet File", "Warning");
        }

        private void Recieve_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
                MainFrame.NavigationService.Navigate(new recieve(Model));
            else
                MessageBox.Show("Please Selecte Wallet File", "Warning");
        }

        private void Transactions_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
                MainFrame.NavigationService.Navigate(new transactions());
            else
                MessageBox.Show("Please Selecte Wallet File", "Warning");
        }

        private void New_Restore_Click(object sender, RoutedEventArgs e)
        {
            var walletWizard = new walletWizard();
            walletWizard.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            walletWizard.Show();
            Close();
        }

        private void OpenWallet_Click(object sender, RoutedEventArgs e)
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

        private void sign_Message_Click(object sender, RoutedEventArgs e)
        {
            new signMessage(Model).Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new verifyMessage().Show();
        }

        private void Backup_Wallet_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Wallet Files|*.json";
            saveFileDialog.Title = "Backup Wallet";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var walletFile = new FileInfo(Model.Wallet.WalletFilePath);
            saveFileDialog.FileName = walletFile.Name;
            if (saveFileDialog.ShowDialog() == true) walletFile.CopyTo(saveFileDialog.FileName, true);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var walletFile = new FileInfo(Model.Wallet.WalletFilePath);
            var message = "Delete '" + walletFile.Name + "' File?\n" +
                          "if your wallet contains funds, make sure save wallet seed and password";
            var messageBoxResult = MessageBox.Show(message, "Warning?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
                walletFile.Delete();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void privateKeys_Click(object sender, RoutedEventArgs e)
        {
            new privateKeys(Model).Show();
        }

        private void Addresses_Click(object sender, RoutedEventArgs e)
        {
            new Addresses(Model).Show();
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            new about().Show();
        }
    }
}
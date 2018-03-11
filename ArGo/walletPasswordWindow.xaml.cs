using System.Windows;
using ArGo.src;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for walletPasswordWindow.xaml
    /// </summary>
    public partial class walletPasswordWindow : Window
    {
        public walletPasswordWindow(string walletPath)
        {
            InitializeComponent();

            WalletFilePath = walletPath;
        }

        public string WalletFilePath { set; get; }

        private void WalletPasswordOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.Model = new walletViewModel(WalletPasswordBox.Password, WalletFilePath);
                MainWindow.Model.Update();
                MainWindow.walletPasswordCancel = false;
                var main = new MainWindow();
                main.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                main.Show();
                Close();
            }
            catch
            {
                MessageBox.Show("Invaid Password Or Wallet File!", "Warning", MessageBoxButton.OK);
            }
        }

        private void WalletPasswordCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.walletPasswordCancel = true;
            new MainWindow().Show();
            Close();
        }
    }
}
using System.IO;
using System.Windows;
using ArGo.src;
using Newtonsoft.Json;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for TxInfo.xaml
    /// </summary>
    public partial class TxInfo : Window
    {
        public TxInfo(walletViewModel model, TxData tx)
        {
            Tx = tx;
            Model = model;

            if (Tx.lockTime < 1)
                Confirmations = 0;
            else
                Confirmations = Model.height - Tx.lockTime + 1;

            InitializeComponent();

            DataContext = this;
        }

        public TxData Tx { set; get; }
        public walletViewModel Model { set; get; }
        public long Confirmations { set; get; }

        private void change_Click(object sender, RoutedEventArgs e)
        {
            var data = JsonConvert.DeserializeObject<Data>(File.ReadAllText(walletFileSerializer
                .Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath));
            data.txData[Tx.hash].description = Description.Text;
            File.WriteAllText(walletFileSerializer.Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath,
                JsonConvert.SerializeObject(data));
            MainWindow.Model.Update();
        }
    }
}
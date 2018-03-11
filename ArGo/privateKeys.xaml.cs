using System.Collections.Generic;
using System.Windows;
using ArGo.src;
using NBitcoin;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for privateKeys.xaml
    /// </summary>
    public partial class privateKeys : Window
    {
        public privateKeys(walletViewModel model)
        {
            privateKeysList = new List<pKeys>();

            foreach (var addr in model.Addresses)
                privateKeysList.Add(new pKeys
                {
                    address = addr,
                    key = model.Wallet.FindPrivateKey(BitcoinAddress.Create(addr, Network.Main)).PrivateKey
                        .GetBitcoinSecret(Network.Main).ToString()
                });

            InitializeComponent();

            privateKeyDataGrid.ItemsSource = privateKeysList;

            DataContext = this;
        }

        public List<pKeys> privateKeysList { set; get; }
    }

    public class pKeys
    {
        public string address { set; get; }
        public string key { set; get; }
    }
}
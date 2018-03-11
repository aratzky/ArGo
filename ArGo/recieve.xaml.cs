using System.IO;
using System.Windows;
using System.Windows.Controls;
using ArGo.src;
using Newtonsoft.Json;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for recieve.xaml
    /// </summary>
    public partial class recieve : Page
    {
        public recieve(walletViewModel model)
        {
            Model = model;

            InitializeComponent();

            DataContext = this;
        }

        public walletViewModel Model { set; get; }

        private void GenerateNewAddress_Click(object sender, RoutedEventArgs e)
        {
            var data = JsonConvert.DeserializeObject<Data>(File.ReadAllText(walletFileSerializer
                .Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath));
            var Index = data.addresses.receiving.IndexOf(Address.Text);
            try
            {
                while (true)
                {
                    Index++;
                    if (!data.usedAddresses.Contains(data.addresses.receiving[Index]))
                    {
                        Address.Text = data.addresses.receiving[Index];
                        break;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Please Use Old Addresses to Generate New", "Warning", MessageBoxButton.OK);
            }
        }
    }
}
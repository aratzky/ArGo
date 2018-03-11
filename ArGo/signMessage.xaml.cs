using System.Collections.Generic;
using System.Windows;
using ArGo.src;
using NBitcoin;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for signMessage.xaml
    /// </summary>
    public partial class signMessage : Window
    {
        public signMessage(walletViewModel model)
        {
            Model = model;

            InitializeComponent();

            DataContext = this;
        }

        public List<string> Addresses { set; get; }
        public walletViewModel Model { set; get; }

        private void Sign_Button_Click(object sender, RoutedEventArgs e)
        {
            var key = Model.Wallet.FindPrivateKey(BitcoinAddress.Create(Addresses_Combobox.SelectedItem.ToString(),
                Network.Main));
            signature.Text = key.PrivateKey.SignMessage(message.Text);
        }
    }
}
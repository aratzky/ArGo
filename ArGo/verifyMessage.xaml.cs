using System;
using System.Windows;
using NBitcoin;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for verifyMessage.xaml
    /// </summary>
    public partial class verifyMessage : Window
    {
        public verifyMessage()
        {
            InitializeComponent();
        }

        private void Verify_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var address = new BitcoinPubKeyAddress(Address.Text, Network.Main);
                if (address.VerifyMessage(message.Text, signature.Text))
                    MessageBox.Show("Signature Verified", "verify Message", MessageBoxButton.OK);
                else
                    MessageBox.Show("Wrong Signature", "verify Message", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK);
            }
        }
    }
}
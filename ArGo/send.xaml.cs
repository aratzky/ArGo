using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArGo.src;
using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for send.xaml
    /// </summary>
    public partial class send : Page, INotifyPropertyChanged
    {
        public send(walletViewModel model)
        {
            Model = model;
            feeDisplay = string.Format("Low fee, {0}. sat/byte", Model.asyncData.low_fee_per_kb);
            feeValue = Model.asyncData.low_fee_per_kb;

            NotEmptyPrivateKeys = new List<BitcoinExtKey>();
            foreach (var elem in Model.BalancePerAddress)
                NotEmptyPrivateKeys.Add(Model.Wallet.FindPrivateKey(BitcoinAddress.Create(elem.Key)));

            InitializeComponent();

            DataContext = this;
        }

        public string feeDisplay { set; get; }
        public decimal feeValue { set; get; }
        public Money Txfee { set; get; }
        public List<BitcoinExtKey> NotEmptyPrivateKeys { set; get; }

        private walletViewModel Model { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void send_Button_Click(object sender, RoutedEventArgs e)
        {
            if (PayTo.Text != "" && Amount.Text != "")
                try
                {
                    var addressToSend = BitcoinAddress.Create(PayTo.Text, Network.Main);
                    var amountToSend = new Money(Convert.ToDecimal(Amount.Text), MoneyUnit.BTC);
                    var signingKeys = new HashSet<ISecret>();
                    var builder = new TransactionBuilder();
                    var tx = new Transaction();
                    if (Txfee != null)
                    {
                        foreach (var coin in Model.UnspentCoins)
                            NotEmptyPrivateKeys.ForEach(key =>
                            {
                                if (key.ScriptPubKey == coin.ScriptPubKey) signingKeys.Add(key);
                            });
                        tx = builder
                            .AddCoins(Model.UnspentCoins)
                            .AddKeys(signingKeys.ToArray())
                            .Send(addressToSend, amountToSend)
                            .SetChange(Model.changeScriptPubKey)
                            .SendFees(Txfee)
                            .BuildTransaction(true);

                        if (builder.Verify(tx))
                        {
                            var message = string.Format("Amount To Be Sent: {0} BTC\nMining fee: {1} BTC\nProceed?",
                                amountToSend.ToDecimal(MoneyUnit.BTC), Txfee.ToDecimal(MoneyUnit.BTC));
                            var messageBoxResult = MessageBox.Show(message, "Are you sure?", MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {
                                var Url = @"https://api.smartbit.com.au/v1/blockchain/pushtx";
                                var requestFormat = "{\"hex\": \"raw\"}";
                                var json = requestFormat.Replace("raw", tx.ToHex());
                                if (MainWindow.CheckForInternetConnection())
                                    using (var client = new HttpClient())
                                    {
                                        var response = await client.PostAsync(Url,
                                            new StringContent(json, Encoding.UTF8, "application/json"));
                                        var jasondata = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                                        if (jasondata.Value<bool>("success"))
                                        {
                                            MessageBox.Show(string.Format("Payment Sent.\nTx : {0}", tx.GetHash()));
                                            new Thread(() =>
                                            {
                                                Thread.CurrentThread.IsBackground = true;
                                                walletData.Serialize(Model.Wallet);
                                                MainWindow.Model.Update();
                                                Dispatcher.Invoke(() =>
                                                {
                                                    if (Description.Text != "")
                                                    {
                                                        var data = JsonConvert.DeserializeObject<Data>(
                                                            File.ReadAllText(walletFileSerializer
                                                                .Deserialize(Model.Wallet.WalletFilePath)
                                                                .walletTransactionsPath));
                                                        if (data.txData.ContainsKey(tx.GetHash().ToString()))
                                                            data.txData[tx.GetHash().ToString()].description =
                                                                Description.Text;
                                                        File.WriteAllText(
                                                            walletFileSerializer
                                                                .Deserialize(Model.Wallet.WalletFilePath)
                                                                .walletTransactionsPath,
                                                            JsonConvert.SerializeObject(data));
                                                    }
                                                });
                                            }).Start();
                                        }
                                        else
                                        {
                                            MessageBox.Show("Sending Has Failed!", "Warning");
                                        }
                                    }
                                else
                                    MessageBox.Show("No internet Connection To Broadcast Transaction", "Warning");
                            }
                        }
                    }
                    else
                    {
                        bool haveEnough;
                        var fee = Money.Zero;
                        var coinsToSpend = walletData.GetCoinsToSpend(Model.UnspentCoins, feeValue, amountToSend,
                            ref fee, out haveEnough);

                        if (!haveEnough)
                            MessageBox.Show("Not Enough Funds", "Warning", MessageBoxButton.OK);

                        foreach (var coin in coinsToSpend)
                            NotEmptyPrivateKeys.ForEach(key =>
                            {
                                if (key.ScriptPubKey == coin.ScriptPubKey) signingKeys.Add(key);
                            });

                        tx = builder
                            .AddCoins(coinsToSpend)
                            .AddKeys(signingKeys.ToArray())
                            .Send(addressToSend, amountToSend)
                            .SetChange(Model.changeScriptPubKey)
                            .SendFees(fee)
                            .BuildTransaction(true);

                        if (builder.Verify(tx))
                        {
                            var message = string.Format("Amount To Be Sent: {0} BTC\nMining fee: {1} BTC\nProceed?",
                                amountToSend.ToDecimal(MoneyUnit.BTC), fee.ToDecimal(MoneyUnit.BTC));
                            var messageBoxResult = MessageBox.Show(message, "Are you sure?", MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {
                                var Url = @"https://api.smartbit.com.au/v1/blockchain/pushtx";
                                var requestFormat = "{\"hex\": \"raw\"}";
                                var json = requestFormat.Replace("raw", tx.ToHex());
                                if (MainWindow.CheckForInternetConnection())
                                    using (var client = new HttpClient())
                                    {
                                        var response = await client.PostAsync(Url,
                                            new StringContent(json, Encoding.UTF8, "application/json"));
                                        var jasondata = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                                        if (jasondata.Value<bool>("success"))
                                        {
                                            MessageBox.Show(string.Format("Payment Sent.\nTx : {0}", tx.GetHash()));
                                            new Thread(() =>
                                            {
                                                Thread.CurrentThread.IsBackground = true;
                                                walletData.Serialize(Model.Wallet);
                                                MainWindow.Model.Update();
                                                Dispatcher.Invoke(() =>
                                                {
                                                    if (Description.Text != "")
                                                    {
                                                        var data = JsonConvert.DeserializeObject<Data>(
                                                            File.ReadAllText(walletFileSerializer
                                                                .Deserialize(Model.Wallet.WalletFilePath)
                                                                .walletTransactionsPath));
                                                        if (data.txData.ContainsKey(tx.GetHash().ToString()))
                                                            data.txData[tx.GetHash().ToString()].description =
                                                                Description.Text;
                                                        File.WriteAllText(
                                                            walletFileSerializer
                                                                .Deserialize(Model.Wallet.WalletFilePath)
                                                                .walletTransactionsPath,
                                                            JsonConvert.SerializeObject(data));
                                                    }
                                                });
                                            }).Start();
                                        }
                                        else
                                        {
                                            MessageBox.Show("Sending Has Failed!");
                                        }
                                    }
                                else
                                    MessageBox.Show("No internet Connection To Broadcast Transaction", "Warning");
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Please Enter Correct Amount and Pay To Address", "Warning", MessageBoxButton.OK);
                }
        }

        private void feeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch (feeSlider.Value)
            {
                case 0:
                    feeDisplay = string.Format("Low fee, {0}. sat/byte", Model.asyncData.low_fee_per_kb);
                    feeValue = Model.asyncData.low_fee_per_kb;
                    break;
                case 1:
                    feeDisplay = string.Format("Medium fee, {0}. sat/byte", Model.asyncData.medium_fee_per_kb);
                    feeValue = Model.asyncData.medium_fee_per_kb;
                    break;
                case 2:
                    feeDisplay = string.Format("High fee, {0}. sat/byte", Model.asyncData.high_fee_per_kb);
                    feeValue = Model.asyncData.high_fee_per_kb;
                    break;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feeDisplay)));
        }

        private void Amount_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Amount.Text == "BTC")
            {
                Amount.Clear();
                Amount.TextAlignment = TextAlignment.Left;
            }
        }

        private void Max_Button_Click(object sender, RoutedEventArgs e)
        {
            var fee = Money.Zero;
            var AmountToSend = Money.Zero;
            walletData.SpentAllCoins(Model.UnspentCoins, feeValue, ref fee, ref AmountToSend);
            Amount.Text = AmountToSend.ToDecimal(MoneyUnit.BTC).ToString();
            Txfee = fee;
        }

        private void Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            Txfee = null;
            if (Amount.Text != "BTC")
            {
                var AmountToSend = decimal.Zero;
                if (decimal.TryParse(Amount.Text, out AmountToSend))
                    if (AmountToSend > Model.availableBalance)
                    {
                        MessageBox.Show("Not enough funds in Available Balance", "Amount", MessageBoxButton.OK);
                        Amount.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        Amount.Foreground = new SolidColorBrush(Colors.Green);
                    }
            }
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            Amount.Clear();
            PayTo.Clear();
            Description.Clear();
        }
    }
}
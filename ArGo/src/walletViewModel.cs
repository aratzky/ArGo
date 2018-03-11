using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NBitcoin;
using Newtonsoft.Json;
using static ArGo.src.walletData;

namespace ArGo.src
{
    public class walletViewModel : INotifyPropertyChanged
    {
        public walletViewModel(string Password, string walletPath)
        {
            Wallet = walletManagement.Load(Password, walletPath);
        }

        public walletManagement Wallet { set; get; }


        private decimal available { set; get; }
        private decimal pending { set; get; }
        private decimal total { set; get; }
        private List<TxData> txhistory { set; get; }
        private long stored_height { set; get; }
        private string Address { set; get; }
        public AsyncData asyncData { set; get; }
        public bool feeAsyncResult { set; get; }
        private Dictionary<string, Tuple<decimal, decimal>> Balances { set; get; }
        public Script changeScriptPubKey { set; get; }
        public List<Coin> UnspentCoins { set; get; }
        public List<string> Addresses { set; get; }


        public long height
        {
            get => stored_height;
            set
            {
                if (value != stored_height)
                {
                    stored_height = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("stored_height"));
                }
            }
        }

        public List<TxData> TxRecords
        {
            get => txhistory;
            set
            {
                txhistory = value;
                PropertyChanged(this, new PropertyChangedEventArgs("TxDataGrid"));
            }
        }

        public string UnunusedAddress
        {
            get => Address;
            set
            {
                if (value != Address)
                {
                    Address = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("UnunusedAddress"));
                }
            }
        }

        public Dictionary<string, Tuple<decimal, decimal>> BalancePerAddress
        {
            get => Balances;
            set
            {
                if (value != Balances)
                {
                    Balances = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("BalancePerAddress"));
                }
            }
        }

        public decimal availableBalance
        {
            get => available;
            set
            {
                if (value != available)
                {
                    available = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("availableBalance"));
                }
            }
        }

        public decimal pendingBalance
        {
            get => pending;
            set
            {
                if (value != pending)
                {
                    pending = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("pendingBalance"));
                }
            }
        }

        public decimal totalBalance
        {
            get => total;
            set
            {
                if (value != total)
                {
                    total = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("totalBalance"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void Update()
        {
            var data = JsonConvert.DeserializeObject<Data>(
                File.ReadAllText(walletFileSerializer.Deserialize(Wallet.WalletFilePath).walletTransactionsPath));
            var asyncDataFilePath = new FileInfo(Wallet.WalletFilePath).Directory.FullName +
                                    Path.DirectorySeparatorChar + "asyncData.json";
            if (File.Exists(asyncDataFilePath))
            {
                asyncData = JsonConvert.DeserializeObject<AsyncData>(File.ReadAllText(asyncDataFilePath));
                height = asyncData.height;
            }


            BalancePerAddress = GetBalances(data);
            availableBalance = decimal.Zero;
            pendingBalance = decimal.Zero;
            foreach (var elem in BalancePerAddress)
            {
                availableBalance += elem.Value.Item1;
                pendingBalance += elem.Value.Item2;
            }

            totalBalance = availableBalance + pendingBalance;
            TxRecords = data.txData.Values.OrderBy(x => x.date).ToList();

            foreach (var addr in data.addresses.receiving)
                if (!data.usedAddresses.Contains(addr))
                {
                    UnunusedAddress = addr;
                    break;
                }

            foreach (var addr in data.addresses.change)
                if (!data.usedAddresses.Contains(addr))
                {
                    var address = BitcoinAddress.Create(addr);
                    changeScriptPubKey = Wallet.FindPrivateKey(address).ScriptPubKey;
                    break;
                }

            UnspentCoins = GetUnspentCoins(data);
            Addresses = new List<string>();
            Addresses.AddRange(data.addresses.change);
            Addresses.AddRange(data.addresses.receiving);
        }
    }
}
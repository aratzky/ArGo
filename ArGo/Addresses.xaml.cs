using System.Collections.Generic;
using System.Windows;
using ArGo.src;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for Addresses.xaml
    /// </summary>
    public partial class Addresses : Window
    {
        public Addresses(walletViewModel model)
        {
            AddressesData = new List<addrData>();
            foreach (var addr in model.Addresses)
                if (model.BalancePerAddress.ContainsKey(addr))
                    AddressesData.Add(new addrData
                    {
                        address = addr,
                        balance = model.BalancePerAddress[addr].Item1 + model.BalancePerAddress[addr].Item2
                    });
                else
                    AddressesData.Add(new addrData {address = addr, balance = decimal.Zero});

            InitializeComponent();

            AddressesDataGrid.ItemsSource = AddressesData;
        }

        public List<addrData> AddressesData { set; get; }
    }

    public class addrData
    {
        public string address { set; get; }
        public decimal balance { set; get; }
    }
}
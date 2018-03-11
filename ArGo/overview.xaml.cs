using System.Linq;
using System.Windows.Controls;
using ArGo.src;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for overview.xaml
    /// </summary>
    public partial class overview : Page
    {
        public overview(walletViewModel model)
        {
            Model = model;

            InitializeComponent();

            if (Model.TxRecords != null)
                TxDataGrid.ItemsSource = Model.TxRecords.OrderByDescending(x => x.date).ToList();

            DataContext = this;
        }

        public walletViewModel Model { set; get; }
    }
}
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ArGo.src;

namespace ArGo
{
    /// <summary>
    ///     Interaction logic for transactions.xaml
    /// </summary>
    public partial class transactions : Page
    {
        public transactions()
        {
            InitializeComponent();

            if (MainWindow.Model.TxRecords != null)
                TxDataGrid.ItemsSource = MainWindow.Model.TxRecords.OrderByDescending(x => x.date).ToList();

            DataContext = this;
        }

        private void transactionsData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TxDataGrid.SelectedItem == null) return;
            var ClickedTx = TxDataGrid.SelectedItem as TxData;
            new TxInfo(MainWindow.Model, ClickedTx).Show();
        }
    }

    public class negativeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = new SolidColorBrush(Colors.Green);
            var doubleValue = 0.0;
            double.TryParse(value.ToString(), out doubleValue);

            if (doubleValue < 0)
                brush = new SolidColorBrush(Colors.Red);

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
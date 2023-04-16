using Felipe.YoutubeExtractor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Felipe.YoutubeExtractor.Views
{
    public partial class History : Window
    {
        private readonly HistoryService _historyService;

        public History()
        {
            InitializeComponent();

            _historyService = new HistoryService();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var historyList = await _historyService.Get();

            HistoryDataGrid.ItemsSource = historyList;
            TotalLabel.Content = historyList.Count;
        }
    }
}

using Felipe.YoutubeExtractor.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

        private async Task LoadHistory()
        {
            var historyList = await _historyService.Get();

            HistoryDataGrid.ItemsSource = historyList;
            TotalLabel.Content = historyList.Count;

            ResetHistoryBtn.IsEnabled = historyList.Any();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadHistory();
        }

        private async void ResetHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = MessageBox.Show("Reset History will delete ALL your history. Do you want to proceed?", "Reset History", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (response != MessageBoxResult.Yes)
                {
                    return;
                }

                await _historyService.Delete();

                await LoadHistory();

                MessageBox.Show("History Deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Error on deleting history.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

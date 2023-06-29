using Felipe.YoutubeExtractor.Core.Helpers;
using Felipe.YoutubeExtractor.Core.Models;
using Felipe.YoutubeExtractor.Services;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Felipe.YoutubeExtractor.Views
{
    /// <summary>
    /// Lógica interna para Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings(OptionsModel config)
        {
            InitializeComponent();

            LoadComponents(config);
        }

        private void LoadComponents(OptionsModel config)
        {
            YtDlpFilePathTextBox.Text = config.YtdlpPath;
            YtDlpDefaultFolderCheckBox.IsChecked = config.YtdlpPathDefaultFolder;
            FfmpegFilePathTextBox.Text = config.FfmpegPath;
            FfmpegDefaultFolderCheckBox.IsChecked = config.FfmpegPathDefaultFolder;
            OutputPathTextBox.Text = config.OutputPath;
            OutputDefaultFolderCheckBox.IsChecked = config.OutputPathDefaultFolder;

            EnableNotificationsCheckBox.IsChecked = config.EnableNotifications;

            SetUpYtDlpCheckBox();
            SetUpFfmpegCheckBox();
            SetUpOutputCheckBox();
        }

        private void SetUpYtDlpCheckBox()
        {
            var isChecked = YtDlpDefaultFolderCheckBox.IsChecked ?? true;

            if (isChecked)
                YtDlpFilePathTextBox.Text = OptionsModel.GetYtDlpDefaultFolder();
                
            YtDlpFilePathTextBox.IsReadOnly = isChecked;
            YtDlpFilePathBtn.IsEnabled = !isChecked;
        }

        private void SetUpFfmpegCheckBox()
        {
            var isChecked = FfmpegDefaultFolderCheckBox.IsChecked ?? true;

            if (isChecked)
                FfmpegFilePathTextBox.Text = OptionsModel.GetFfmpegDefaultFolder();

            FfmpegFilePathTextBox.IsReadOnly = isChecked;
            FfmpegFilePathBtn.IsEnabled = !isChecked;
        }

        private void SetUpOutputCheckBox()
        {
            var isChecked = OutputDefaultFolderCheckBox.IsChecked ?? true;

            if (isChecked)
                OutputPathTextBox.Text = OptionsModel.GetOutputDefaultFolder();

            OutputPathTextBox.IsReadOnly = isChecked;
            OutputPathBtn.IsEnabled = !isChecked;
        }

        private void YtDlpDefaultFolderCheckBox_Change(object sender, RoutedEventArgs e)
        {
            SetUpYtDlpCheckBox();
        }

        private void FfmpegDefaultFolderCheckBox_Change(object sender, RoutedEventArgs e)
        {
            SetUpFfmpegCheckBox();
        }

        private void OutputDefaultFolderCheckBox_Change(object sender, RoutedEventArgs e)
        {
            SetUpOutputCheckBox();
        }

        private void YtDlpFilePathBtn_Click(object sender, RoutedEventArgs e)
        {
            var initialDirectory = FileHelper.GetFolderPathFromFilePath(YtDlpFilePathTextBox.Text);

            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                ShowReadOnly = true,
                InitialDirectory = initialDirectory ?? string.Empty
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filename = openFileDialog.FileNames.FirstOrDefault();

                YtDlpFilePathTextBox.Text = filename;
            }
        }

        private void FfmpegFilePathBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new VistaFolderBrowserDialog()
            {
                Multiselect = false,
                ShowNewFolderButton = true
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                FfmpegFilePathTextBox.Text = openFolderDialog.SelectedPath;
            }
        }

        private void OutputPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new VistaFolderBrowserDialog()
            {
                Multiselect = false,
                ShowNewFolderButton = true
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                OutputPathTextBox.Text = openFolderDialog.SelectedPath;
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var (config, _) = await ConfigService.StartupConfig();

            config.YtdlpPath = YtDlpFilePathTextBox.Text;
            config.YtdlpPathDefaultFolder = YtDlpDefaultFolderCheckBox.IsChecked ?? true;
            config.FfmpegPath = FfmpegFilePathTextBox.Text;
            config.FfmpegPathDefaultFolder = FfmpegDefaultFolderCheckBox.IsChecked ?? true;
            config.OutputPath = OutputPathTextBox.Text;
            config.OutputPathDefaultFolder = OutputDefaultFolderCheckBox.IsChecked ?? true;
            config.EnableNotifications = EnableNotificationsCheckBox.IsChecked ?? true;

            await ConfigService.UpdateConfig(config);

            MessageBox.Show("Settings saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            Close();
        }

        private async void YtDlpDownloadBtn_Click(object sender, RoutedEventArgs e) 
        {
            if (File.Exists(OptionsModel.GetYtDlpDefaultFolder()))
            {
                var response = MessageBox.Show("File already exists. Do you want to download again?", "Cancelled", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (response != MessageBoxResult.Yes) 
                {
                    return;
                }
            }

            var downloadDialog = new DownloadProgress(progressVisible: true);

            try
            {
                var downloadTask = downloadDialog.StartYtDlpDownload();
                downloadDialog.ShowDialog();
                await downloadTask;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void FfmpegDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(OptionsModel.GetFfmpegDefaultFile()))
            {
                var response = MessageBox.Show("File already exists. Do you want to download again?", "Cancelled", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (response != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var downloadDialog = new DownloadProgress(progressVisible: true);

            try
            {
                var downloadTask = downloadDialog.StartFfmpegDownload();
                downloadDialog.ShowDialog();
                await downloadTask;
            }
            catch (TaskCanceledException) 
            {
                MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

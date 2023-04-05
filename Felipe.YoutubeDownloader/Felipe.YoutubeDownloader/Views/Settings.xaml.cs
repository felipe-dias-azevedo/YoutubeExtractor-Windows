﻿using Felipe.YoutubeDownloader.Services;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using YoutubeDLSharp;

namespace Felipe.YoutubeDownloader.Views
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
            var initialDirectory = FileService.GetFolderPathFromFilePath(YtDlpFilePathTextBox.Text);

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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var config = ConfigService.StartupConfig();

            config.YtdlpPath = YtDlpFilePathTextBox.Text;
            config.YtdlpPathDefaultFolder = YtDlpDefaultFolderCheckBox.IsChecked ?? true;
            config.FfmpegPath = FfmpegFilePathTextBox.Text;
            config.FfmpegPathDefaultFolder = FfmpegDefaultFolderCheckBox.IsChecked ?? true;
            config.OutputPath = OutputPathTextBox.Text;
            config.OutputPathDefaultFolder = OutputDefaultFolderCheckBox.IsChecked ?? true;

            ConfigService.UpdateConfig(config);

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
                await downloadDialog.StartYtDlpDownload();
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
                await downloadDialog.StartFfmpegDownload();
            }
            catch (TaskCanceledException) 
            {
                MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

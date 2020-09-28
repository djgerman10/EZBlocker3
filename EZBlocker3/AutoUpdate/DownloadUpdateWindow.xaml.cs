﻿using EZBlocker3.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace EZBlocker3.AutoUpdate {
    public partial class DownloadUpdateWindow : Window {

        private readonly UpdateInfo Update;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        public DownloadUpdateWindow(UpdateInfo update) {
            Update = update;
            InitializeComponent();

            abortDownloadButton.Click += (_, __) => {
                _cancellationSource.Cancel();
                Close();
            };

            Task.Run(DownloadUpdate, _cancellationSource.Token);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            _cancellationSource.Cancel();
        }

        private async Task DownloadUpdate() {
            var downloader = new UpdateDownloader();
            downloader.Progress += (s, e) => {
                Dispatcher.Invoke(() => {
                    var normalizedPercentage = e.DownloadPercentage;
                    var percentage = normalizedPercentage * 100;
                    downloadProgress.Value = percentage;
                    downloadState.Text = $"Downloading... {Math.Round(percentage)}%";
                    TaskbarItemInfo = new TaskbarItemInfo() {
                        ProgressValue = normalizedPercentage,
                        ProgressState = TaskbarItemProgressState.Normal
                    };
                });
            };

            DownloadedUpdate? downloadedUpdate = null;
            try {
                downloadedUpdate = await downloader.Download(Update, _cancellationSource.Token);
            } catch (Exception e) {
                Logger.LogException("AutoUpdate: Update download failed", e);
                Dispatcher.Invoke(() => {
                    downloadState.Text = $"Download failed";
                });
                return;
            }

            Dispatcher.Invoke(() => {
                downloadState.Text = $"Installing...";
                TaskbarItemInfo = new TaskbarItemInfo() {
                    ProgressState = TaskbarItemProgressState.Indeterminate
                };
                UpdateInstaller.InstallUpdateAndRestart(downloadedUpdate);
                TaskbarItemInfo = new TaskbarItemInfo() {
                    ProgressValue = 0,
                    ProgressState = TaskbarItemProgressState.None
                };
            });
        }
    }
}

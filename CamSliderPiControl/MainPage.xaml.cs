using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.ExtendedExecution.Foreground;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CamSliderPiControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CamSliderCommander.Isaac879SliderControl _slider;

        Windows.System.Display.DisplayRequest _displayRequest;

        private TranslateTransform screensaverTranslation;

        DateTime startupDateTime;

        private DateTime _lastTouchDetected;
        private int _screensaverDelaySeconds = 10;


        public MainPage()
        {
            this.InitializeComponent();

            //don't let the app go idle
            _displayRequest = new Windows.System.Display.DisplayRequest();
            _displayRequest.RequestActive();

            //===================================
            //Get values from config

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            //Some sample config getters from my other project.  A reminder to myself...
            //enableWingsCheckbox.IsChecked = GetConfig_Bool(localSettings, "EnableWings");
            //enableHeadCheckbox.IsChecked = GetConfig_Bool(localSettings, "EnableHead");
            //enableLeftEyeCheckbox.IsChecked = GetConfig_Bool(localSettings, "EnableLeftEye");
            //enableRightEyeCheckbox.IsChecked = GetConfig_Bool(localSettings, "EnableRightEye");

            _slider = new CamSliderCommander.Isaac879SliderControl();

            _slider.DeviceError += _component_DeviceError;
            _slider.MoveCompleted += _component_MoveCompleted;

            _slider.DeviceInfoMessage += _slider_DeviceInfoMessage;

            _slider.Initialize();


        }

        private void _slider_DeviceInfoMessage(object sender, string message)
        {
            SetStatusLabel("DEVICE MESSAGE: " + message);
        }

        private void _component_MoveCompleted(object sender)
        {
            SetStatusLabel("Move Completed");
        }

        private void _component_DeviceError(object sender, string error)
        {
            SetStatusLabel("Error");
        }

        private void SetStatusLabel(string msg)
        {
            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- " + msg;
                //BlinkStatusLight();
            });
        }


        private static bool GetConfig_Bool(Windows.Storage.ApplicationDataContainer localSettings, string st)
        {
            bool b = false;
            Object value = localSettings.Values[st];
            if (value != null)
                b = (bool)value;

            return b;
        }

        private static void SetConfig_Bool(Windows.Storage.ApplicationDataContainer localSettings, string st, bool val)
        {
            localSettings.Values[st] = val;
        }

        private void saveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            //SetConfig_Bool(localSettings, "EnableWings", enableWingsCheckbox.IsChecked.GetValueOrDefault());
            //SetConfig_Bool(localSettings, "EnableHead", enableHeadCheckbox.IsChecked.GetValueOrDefault());
            //SetConfig_Bool(localSettings, "EnableLeftEye", enableLeftEyeCheckbox.IsChecked.GetValueOrDefault());
            //SetConfig_Bool(localSettings, "EnableRightEye", enableRightEyeCheckbox.IsChecked.GetValueOrDefault());

        }


        private void OnSessionRevoked(object sender, ExtendedExecutionForegroundRevokedEventArgs args)
        {
            _slider.Shutdown();
            Application.Current.Exit();
        }

        private void autohomeButton_Click(object sender, RoutedEventArgs e)
        {
            _slider.Recalibrate();
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _slider.DeviceComm.Disconnect("User requested a disconnect");
        }

        private void sliderHomeButton_Click(object sender, RoutedEventArgs e)
        {
            _slider.X_Axis.GoHomePosition();
        }

        private void slider100Button_Click(object sender, RoutedEventArgs e)
        {
            _slider.X_Axis.GotoPosition(100);
        }

        private void getStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    LoadingIndicator.IsActive = true;
                    UpdateControlsWithStatus();
                }
                finally
                {
                    LoadingIndicator.IsActive = false;
                }
            });
        }


        private void sendPositionsToSlider_Click(object sender, RoutedEventArgs e)
        {
            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    LoadingIndicator.IsActive = true;
                    UpdateSliderWithControlValues();
                }
                finally
                {
                    LoadingIndicator.IsActive = false;
                }
            });
        }

        private void UpdateSliderWithControlValues()
        {
            _slider.X_Axis.GotoPosition(sliderSlider.Value);
            _slider.Pan_Axis.GotoPosition(panSlider.Value);
            _slider.Tilt_Axis.GotoPosition(tiltSlider.Value);
        }
        private void UpdateControlsWithStatus()
        {
            _slider.UpdateCurrentPositionFromDevice();
            System.Threading.Thread.Sleep(2000);
            sliderSlider.Value = _slider.X_Axis.CurrentPosition.Position;
            panSlider.Value = _slider.Pan_Axis.CurrentPosition.Position;
            tiltSlider.Value = _slider.Tilt_Axis.CurrentPosition.Position;
        }

        private void sendManualCommandButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingIndicator.IsActive = true;
                if (!String.IsNullOrEmpty(manualCommandTextBox.Text))
                {
                    _slider.DeviceComm.Send(manualCommandTextBox.Text);
                }
            }
            finally
            {
                LoadingIndicator.IsActive = false;
            }
        }

        private async void shutdownPiCommandButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog shutdownConfirmDialog = new ContentDialog
            {
                Title = "Shutdown?",
                Content = "Do you want to shutdown this RaspberryPi device? (The slider will continue to operate.)",
                PrimaryButtonText = "Shutdown",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await shutdownConfirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
        }

        private async void storeButton_Click(object sender, RoutedEventArgs e)
        {
            await _slider.AddPosition(speedSlider.Value);
        }

        private async void clearButton_Click(object sender, RoutedEventArgs e)
        {
            await _slider.ClearEntireArray();
        }

        private async void prevButton_Click(object sender, RoutedEventArgs e)
        {
            await _slider.StepBackward();
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            await _slider.StepForward();
        }

        private async void executeButton_Click(object sender, RoutedEventArgs e)
        {
            await _slider.StartExecuteMoves(1);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentDialog autoHomeConfirmDialog = new ContentDialog
            {
                Title = "AutoHome?",
                Content = "Hi.  Want to Auto-Home your slider now before you start?)",
                PrimaryButtonText = "Autohome Now",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await autoHomeConfirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                _slider.Recalibrate();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Aztec.Internal;
using ZXing.Common;
using ZXing.QrCode;
using Timer = System.Timers.Timer;
using BitcoinATM_Application.TransferObject.Parameter;
using BitcoinATM_Application.Service;

namespace BitcoinATM_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VideoCaptureDevice LocalWebCam;
        FilterInfoCollection LocalWebCamsCollection;
        Bitmap capturedImage;
        QRCodeReader reader = new QRCodeReader();

        private Timer myTimer;
        private Timer currentPriceTimer;
        private int PeriodTimeRun = 10000;

        private string WalletId = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            //SkipStepScanWallet();
        }

        private void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            WalletId = string.Empty;
            try
            {
                Camera_Loaded(sender, e);
                PaneMainScreen.Visibility = Visibility.Hidden;
                PaneLoginScreen.Visibility = Visibility.Visible;
            }
            catch (Exception) {
                MessageBox.Show("Please connect with a webcam");
            }
        }
        private void BtnSell_Click(object sender, RoutedEventArgs e)
        {
            WalletId = string.Empty;
            MessageBox.Show("This action is developing");
        }
        private void BtnLoginScreenBack_Click(object sender, RoutedEventArgs e)
        {
            PaneMainScreen.Visibility = Visibility.Visible;
            PaneLoginScreen.Visibility = Visibility.Hidden;
            DecodeQrCode(capturedImage);
            StopTimer();
            Stop_Camera();
        }
        void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image img = (Bitmap)eventArgs.Frame;
                capturedImage = (Bitmap)eventArgs.Frame.Clone();
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    FrameHolder.Source = bi;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void Camera_Loaded(object sender, RoutedEventArgs e)
        {
            LocalWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LocalWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(Camera_NewFrame);
            Stop_Camera();
            Start_Camera();
        }
        private void Start_Camera()
        {
            if (!LocalWebCam.IsRunning)
            {
                LocalWebCam.Start();
                SetTimer();
            }
        }
        private void Stop_Camera()
        {
            if (LocalWebCam.IsRunning)
            {
                LocalWebCam.Stop();
            }
        }
        private void SetTimer()
        {
            //Auto Call Decode Function Each 1 second
            myTimer = new System.Timers.Timer(PeriodTimeRun);
            // Hook up the Elapsed event for the timer. 
            myTimer.Elapsed += OnTimedEvent;
            myTimer.AutoReset = true;
            myTimer.Enabled = true;
        }
        private void StopTimer()
        {
            myTimer.AutoReset = false;
            myTimer.Enabled = false;
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Bitmap imageWebcam = null;
            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 
                imageWebcam = capturedImage;
                txtWalletAddress.Text = DecodeQrCode(imageWebcam);
                if (txtWalletAddress.Text != "" && txtWalletAddress.Text != "Scanning")
                {
                    GetWallet(txtWalletAddress.Text.Replace("bitcoin:",""));
                }
            }));
        }
        private string DecodeQrCode(Bitmap bitmap)
        {
            try
            {
                if (bitmap == null)
                {
                    return string.Empty;
                }
                using (var memoryStream = new MemoryStream())
                {
                    //bitmap.Save("C:\\Nhap\\BooleanLogicProjects\\Bitcoin\\BitCoinATM\\BitcoinApplication\\Img\\abc.jpg", ImageFormat.Jpeg);
                    bitmap.Save(memoryStream, ImageFormat.Bmp);
                    BarcodeReader reader = new BarcodeReader { AutoRotate = true };
                    Result result = reader.Decode(bitmap);
                    string decoded = result.ToString().Trim();
                    return decoded;
                }
            }
            catch(Exception ex)
            {
                return "Scanning";
            }
        }

        public void GetWallet(string walletId) {
            var param = new BitcoinParameter()
            {
                WalletAddress = walletId
            };
            var service = new BitcoinService();
            var walletObj = service.GetWallet(param);
            if (walletObj != null && walletObj.Status == "success") {
                StopTimer();
                Stop_Camera();
                PaneBuyScreen.Visibility = Visibility.Visible;
                PaneLoginScreen.Visibility = Visibility.Hidden;
                lbAddressBalance.Content = walletObj.Data.Available_Balance.ToString("N8");
                GetCurrentPrice();
                SetTimerGetCurrentPrice();
                WalletId = walletId;
            }
            else
            {
                MessageBox.Show("Can not find your address wallet");
            }
        }
        private void BtnBuyScreenBack_Click(object sender, RoutedEventArgs e)
        {
            StopTimerGetCurrentPrice();
            PaneMainScreen.Visibility = Visibility.Visible;
            PaneBuyScreen.Visibility = Visibility.Hidden;
        }

        private void SetTimerGetCurrentPrice()
        {
            GetCurrentPrice();
            //Auto Call Decode Function Each time period
            currentPriceTimer = new Timer(PeriodTimeRun);
            // Hook up the Elapsed event for the timer. 
            currentPriceTimer.Elapsed += OnTimedEventGetCurrentPrice;
            currentPriceTimer.AutoReset = true;
            currentPriceTimer.Enabled = true;
        }
        private void StopTimerGetCurrentPrice()
        {
            currentPriceTimer.AutoReset = false;
            currentPriceTimer.Enabled = false;
        }
        private void OnTimedEventGetCurrentPrice(Object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                GetCurrentPrice();
            }));
        }

        public void GetCurrentPrice()
        {
            var service = new BitcoinService();
            var currentPriceObj = service.GetLastedPriceOfUsd();
            if (currentPriceObj != null)
            {
                lbCurrentPrice.Content = string.Format("{0} {1}", currentPriceObj.Price.ToString("N8"), currentPriceObj.Price_Base.ToUpper());
            }
            else
            {
                MessageBox.Show("Can not get current price value bitcoin");
            }
        }

        private void BtnBuyScreenEnter_Click(object sender, RoutedEventArgs e)
        {
            var numberMoneyInputStr = txtInputMoney.Text;
            double numberMoney = 0;
            if (string.IsNullOrWhiteSpace(numberMoneyInputStr))
            {
                MessageBox.Show("Please enter money");
            }
            try
            {
                numberMoney = Convert.ToDouble(numberMoneyInputStr);
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter only number");
            }
            var totalMoneyInput = Convert.ToDouble(lbTotalMoney.Content.ToString().Trim().Replace("USD", ""));
            totalMoneyInput = totalMoneyInput + numberMoney;
            lbTotalMoney.Content = string.Format("{0} {1}", totalMoneyInput.ToString("N2"), "USD");
            BuyBitcoin();
        }

        private void BtnBuyScreenSubmit_Click(object sender, RoutedEventArgs e)
        {
            BuyBitcoin();
        }

        private void BuyBitcoin() {
            var totalMoneyInput = string.IsNullOrWhiteSpace(lbTotalMoney.Content.ToString())
                ? 0
                : Convert.ToDouble(lbTotalMoney.Content.ToString().Trim().Replace("USD", ""));
            if (totalMoneyInput < 1)
            {
                MessageBox.Show("Please enter money");
            }
            else
            {
                StopTimerGetCurrentPrice();
                PaneBuyScreen.Visibility = Visibility.Hidden;
                BuyBitcoinResult();
                PanelBuyScreenResul.Visibility = Visibility.Visible;
            }
        }

        private void BuyBitcoinResult()
        {
            PanelBuyScreenResul_lbAddressWallet.Content = WalletId;
            PanelBuyScreenResul_lbBitCoinBalance.Content = lbCurrentPrice.Content;
            PanelBuyScreenResul_lbCashTotal.Content = lbTotalMoney.Content;

            var bitcoinPriceStr = lbCurrentPrice.Content.ToString().Trim().Replace("USD", "");
            double bitcoinPrice = string.IsNullOrWhiteSpace(bitcoinPriceStr) 
                ? 0
                : Convert.ToDouble(bitcoinPriceStr);

            var totalCashStr = lbTotalMoney.Content.ToString().Trim().Replace("USD", "");
            double totalCash = string.IsNullOrWhiteSpace(totalCashStr)
                ? 0
                : Convert.ToDouble(totalCashStr);

            var bitcoinCanbuy = totalCash/bitcoinPrice;
            PanelBuyScreenResul_lbFinalBitcoinBalance.Content = bitcoinCanbuy.ToString("N8");
        }

        private void BtnBuyScreenResultBack_Click(object sender, RoutedEventArgs e)
        {
            StopTimerGetCurrentPrice();
            PaneMainScreen.Visibility = Visibility.Visible;
            PanelBuyScreenResul.Visibility = Visibility.Hidden;
        }

        public void SkipStepScanWallet()
        {
            PaneBuyScreen.Visibility = Visibility.Visible;
            PaneLoginScreen.Visibility = Visibility.Hidden;
            SetTimerGetCurrentPrice();
        }
    }
}

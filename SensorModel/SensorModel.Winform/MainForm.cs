using Accord.Video.VFW;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using SensorModel.Services.Repository;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace SensorModel.Winform
{
    public partial class MainForm : Telerik.WinControls.UI.RadForm
    {
        /// <summary>
        /// Değişken tanımlamalar
        /// </summary>

        #region İlk değer yüklemeleri
        private ResponseRepository _responseRepository = null;
        private FilterInfoCollection _videoCaptureDevices;
        private VideoCaptureDevice _finalVideo;
        private bool _isStart = false;
        private System.Timers.Timer _timer = null;
        private AVIWriter _aviWriter = null;
        private CultureInfo _culture = (CultureInfo)CultureInfo.CurrentCulture.Clone(); 
        #endregion

        /// <summary>
        /// constructor ilk değer yüklemeleri yapılmaktadır.
        /// </summary>
        public MainForm()
        {

            _responseRepository = new ResponseRepository(ConfigurationManager.AppSettings["DbConnection"]);//repository için connection açmaktadır.
            _culture.NumberFormat.NumberDecimalSeparator = ".";
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            #region kamera otomatik kapanması için timer
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Elapsed += Timer_Elapsed;

            #endregion

            #region Aforge kütüphanesi için kamera ayarlamaları
            _videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _finalVideo = new VideoCaptureDevice(_videoCaptureDevices[0].MonikerString);
            _finalVideo.VideoResolution = _finalVideo.VideoCapabilities[3];
            _finalVideo.NewFrame -= FinalVideo_NewFrame;
            _finalVideo.NewFrame += FinalVideo_NewFrame;
            #endregion
            //backgroundWorker1.RunWorkerAsync();

            #region Seri Port ayarlamaları
            var portNames = SerialPort.GetPortNames();
            if (portNames.Any())
            {
                serialPort1.PortName = portNames.First();
                serialPort1.Open();
            }
            #endregion
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            #region Timer her tik olduğunda yapılan işlemler. Kamera kaptılır. Video kaydı kaydedilir.
            _timer.Stop();
            _finalVideo.NewFrame -= new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.SignalToStop();
            _aviWriter.Close();
            _isStart = false;
            radCollapsiblePanel2.HeaderText = "MOTION";
            radCollapsiblePanel2.BackColor = Color.FromArgb(191, 219, 255);
            #endregion
        }

        private void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            #region kameradan her frame geldiğinde picturebox basma ve video dosyasına frme eklenir.
            Bitmap tmpImage = (Bitmap)eventArgs.Frame.Clone();
            var videoTemp = (Bitmap)eventArgs.Frame.Clone();
            pictureBoxPreview.Image = tmpImage;
            _aviWriter.AddFrame(videoTemp);
            #endregion
        }

        private void RadForm1_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region Form kapatılırken işlemler yapılır. Kamera kapatılır ve seri port haberleşme kapatılır.
            _finalVideo.NewFrame -= new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.SignalToStop();
            serialPort1.Close();
            _aviWriter?.Close();
            #endregion
        }

        private void radMenuItem3_Click(object sender, EventArgs e)
        {
            #region Uygulama kapatma ve Form kapatılırken işlemler yapılır. Kamera kapatılır ve seri port haberleşme kapatılır.
            _finalVideo.NewFrame -= new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.SignalToStop();
            serialPort1?.Close();
            _aviWriter?.Close();
            Application.Exit();
            #endregion
        }

        private void radMenuItem5_Click(object sender, EventArgs e)
        {
            #region Dashboard açma
            Dashboard dashboard = new Dashboard();
            dashboard.ShowDialog();
            #endregion
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                var aa = _responseRepository.Last();
                if (aa != null)
                {

                    var feed = aa.feeds.Last();
                    if (feed != null)
                    {
                        //this.Text = "DATA TRACKING - LAST UPDATE:" + feed.created_at.AddHours(+3).ToString("dd.MM.yyyy HH:mm:ss");

                        radRadialGaugeTemperature.Value = float.Parse(feed.field1, _culture);
                        radRadialGaugeHumidity.Value = float.Parse(feed.field2, _culture);
                        radRadialGaugeDistance.Value = float.Parse(feed.field4, _culture);
                        radRadialGaugePpm.Value = float.Parse(feed.field5, _culture);

                    }
                }
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            #region Seri porttan gelen bveriler parse edilir. Parse edilen veriler charta yüklenir. Hareket varsa kamera açılır ve kırmızıya döner.
            try
            {
                if (sender != null)
                {
                    SerialPort sp = (SerialPort)sender;
                    string indata = sp.ReadExisting();
                    Debug.WriteLine("Data read");

                    var splitItems = Regex.Split(indata, "\r\n");

                    // MessageBox.Show("data geldi");

                    foreach (var item in splitItems)
                    {
                        string newValue = item;
                        if (newValue.Contains("M=1"))
                        {
                            if (!_isStart)
                            {
                                _timer.Enabled = true;
                                _timer.Start();
                                _isStart = true;
                                _aviWriter = new AVIWriter();
                                _aviWriter.FrameRate = 60;
                                _aviWriter.Open($"Videos\\arduino_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_sss")}.avi", _finalVideo.VideoResolution.FrameSize.Width, _finalVideo.VideoResolution.FrameSize.Height);
                                _finalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                                _finalVideo.Start();
                                radCollapsiblePanel2.HeaderText += " DETECTED";
                                radCollapsiblePanel2.BackColor = Color.Red;
                            }

                        }
                        else
                        {
                            if (newValue.Contains("temprature"))
                            {
                                var tempTemperatures = newValue.Split('|');
                                if (tempTemperatures.Length > 1)
                                {
                                    var temperatures = tempTemperatures[0].Split(':');
                                    if (temperatures.Length > 1)
                                    {
                                        radRadialGaugeTemperature.Value = float.Parse(temperatures[1], _culture);

                                    }
                                    var humidities = tempTemperatures[1].Split(':');
                                    if (humidities.Length > 1)
                                    {
                                        radRadialGaugeHumidity.Value = float.Parse(humidities[1], _culture);

                                    }
                                }
                            }
                            else if (newValue.Contains("Distance"))
                            {
                                var tempDistances = newValue.Split('|');
                                if (tempDistances.Length > 1)
                                {
                                    var distances = tempDistances[0].Split(':');
                                    if (distances.Length > 1)
                                    {
                                        radRadialGaugeDistance.Value = float.Parse(distances[1], _culture);

                                    }
                                    var airQualities = tempDistances[1].Split(':');
                                    if (airQualities.Length > 1)
                                    {
                                        radRadialGaugePpm.Value = float.Parse(airQualities[1], _culture);

                                    }
                                }
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {

            }
            #endregion
        }


        private void radMenuButtonItemCloseVideo_Click(object sender, EventArgs e)
        {
            #region Kameraya kapat tıklandığında kamerada kapatılır.
            _finalVideo.NewFrame -= new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.SignalToStop();
            _aviWriter.Close();
            _isStart = false;
            radCollapsiblePanel2.HeaderText = "MOTION";
            radCollapsiblePanel2.BackColor = Color.FromArgb(191, 219, 255);
            #endregion
        }
    }
}

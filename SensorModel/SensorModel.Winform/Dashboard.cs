using SensorModel.Services.Repository;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telerik.Charting;
using Telerik.WinControls.UI;


namespace SensorModel.Winform
{
    public partial class Dashboard : Telerik.WinControls.UI.RadForm
    {
        ResponseRepository responseRepository = null;

        #region Axislerin belirlenmesi
        private DateTimeCategoricalAxis categoricalAxisTemprature = null;
        private DateTimeCategoricalAxis categoricalAxisHumidity = null;
        private DateTimeCategoricalAxis categoricalAxisMotion = null;
        private DateTimeCategoricalAxis categoricalAxisDistance = null;
        private DateTimeCategoricalAxis categoricalAxisPpm = null;
        #endregion
        public Dashboard()
        {
            InitializeComponent();

            tableLayoutPanel1.Visible = false;

            #region Tarih değerlerin belirlenmesi
            radDateTimePickerStart.Value = DateTime.Now.AddDays(-1);
            radDateTimePickerEnd.Value = DateTime.Now;
            #endregion

            #region Isı Axis Ayarlamaları
            categoricalAxisTemprature = new DateTimeCategoricalAxis();
            categoricalAxisTemprature.DateTimeComponent = DateTimeComponent.Millisecond;
            categoricalAxisTemprature.PlotMode = AxisPlotMode.BetweenTicks;
            categoricalAxisTemprature.LabelFormat = "{0:HH:mm:ss}";
            categoricalAxisTemprature.Font = new Font("Arial", 6, FontStyle.Bold);
            categoricalAxisTemprature.TitleElement.Font = new Font("Arial", 6, FontStyle.Bold);
            #endregion

            #region Nem Axis Ayarlamaları
            categoricalAxisHumidity = new DateTimeCategoricalAxis();
            categoricalAxisHumidity.DateTimeComponent = DateTimeComponent.Millisecond;
            categoricalAxisHumidity.PlotMode = AxisPlotMode.BetweenTicks;
            categoricalAxisHumidity.LabelFormat = "{0:HH:mm:ss}";
            categoricalAxisHumidity.Font = new Font("Arial", 6, FontStyle.Bold);
            categoricalAxisHumidity.TitleElement.Font = new Font("Arial", 6, FontStyle.Bold);
            #endregion

            #region Hareket var/yok Axis Ayarlamaları
            categoricalAxisMotion = new DateTimeCategoricalAxis();
            categoricalAxisMotion.DateTimeComponent = DateTimeComponent.Millisecond;
            categoricalAxisMotion.PlotMode = AxisPlotMode.OnTicks;
            categoricalAxisMotion.LabelFormat = "{0:HH:mm:ss}";
            categoricalAxisMotion.Font = new Font("Arial", 6, FontStyle.Bold);
            categoricalAxisMotion.TitleElement.Font = new Font("Arial", 6, FontStyle.Bold);
            #endregion

            #region Mesafe Axis Ayarlamaları
            categoricalAxisDistance = new DateTimeCategoricalAxis();
            categoricalAxisDistance.DateTimeComponent = DateTimeComponent.Millisecond;
            categoricalAxisDistance.PlotMode = AxisPlotMode.BetweenTicks;
            categoricalAxisDistance.LabelFormat = "{0:HH:mm:ss}";
            categoricalAxisDistance.Font = new Font("Arial", 6, FontStyle.Bold);
            categoricalAxisDistance.TitleElement.Font = new Font("Arial", 6, FontStyle.Bold);
            #endregion

            #region Hava Kalitesi Axis Ayarlamaları
            categoricalAxisPpm = new DateTimeCategoricalAxis();
            categoricalAxisPpm.DateTimeComponent = DateTimeComponent.Millisecond;
            categoricalAxisPpm.PlotMode = AxisPlotMode.BetweenTicks;
            categoricalAxisPpm.LabelFormat = "{0:HH:mm:ss}";
            categoricalAxisPpm.Font = new Font("Arial", 6, FontStyle.Bold);
            categoricalAxisPpm.TitleElement.Font = new Font("Arial", 6, FontStyle.Bold);
            #endregion

        }

        private void radButtonView_Click(object sender, EventArgs e)
        {

            if (radDropDownListCount.SelectedItem != null && radDropDownListCount.SelectedItem.Text != "Select")
            {

                tableLayoutPanel1.Visible = true;
                radChartTempratureView.Series.Clear();
                radChartHumidityView.Series.Clear();
                radChartViewDistance.Series.Clear();
                radChartViewMotion.Series.Clear();
                radChartViewPpm.Series.Clear();

                responseRepository = new ResponseRepository(ConfigurationManager.AppSettings["DbConnection"]);//mongo db'ye bağlanır.


                int count = int.Parse(radDropDownListCount.SelectedItem.Text);

                var chanell = responseRepository.Last();
                if (chanell != null)
                {
                    #region iki tarih arasında ve seçilen adet kadar mongo db'den veriler getirilir.
                    var list = chanell.feeds.AsQueryable().Where(m => m.created_at >= radDateTimePickerStart.Value.ToUniversalTime() && m.created_at <= radDateTimePickerEnd.Value.ToUniversalTime()).OrderByDescending(m => m.created_at
                                ).Take(count).ToList();
                    list.All(m =>
                    {
                        m.created_at = m.created_at.ToLocalTime();
                        return true;
                    }); 
                    #endregion

                    #region Mongo db'den getirilen ısı verisi ile line serisi oluşturulur ve charta yüklenir.
                    SteplineSeries lineSeriesTemprature = new SteplineSeries();
                    lineSeriesTemprature.HorizontalAxis = categoricalAxisTemprature;
                    lineSeriesTemprature.ValueMember = "field1";
                    lineSeriesTemprature.CategoryMember = "created_at";
                    lineSeriesTemprature.DataSource = list;
                    this.radChartTempratureView.Series.Add(lineSeriesTemprature);
                    this.radChartTempratureView.ChartElement.View.Margin = new Padding(0);
                    this.radChartTempratureView.ChartElement.TitleElement.Font = new Font("Arial", 8, FontStyle.Bold);
                    LinearAxis horizontalTemprature = radChartTempratureView.Axes.Get<LinearAxis>(1);
                    horizontalTemprature.Font = new Font("Arial", 6, FontStyle.Bold);
                    #endregion

                    #region Mongo db'den getirilen nem verisi ile line serisi oluşturulur ve charta yüklenir.
                    SteplineSeries lineSeriesHumidity = new SteplineSeries();
                    lineSeriesHumidity.HorizontalAxis = categoricalAxisHumidity;
                    this.radChartHumidityView.Series.Add(lineSeriesHumidity);
                    lineSeriesHumidity.ValueMember = "field2";
                    lineSeriesHumidity.CategoryMember = "created_at";
                    lineSeriesHumidity.DataSource = list;
                    this.radChartHumidityView.ChartElement.View.Margin = new Padding(0);
                    this.radChartHumidityView.ChartElement.TitleElement.Font = new Font("Arial", 8, FontStyle.Bold);
                    LinearAxis horizontalHumidity = radChartHumidityView.Axes.Get<LinearAxis>(1);
                    horizontalHumidity.Font = new Font("Arial", 6, FontStyle.Bold);
                    #endregion

                    #region Mongo db'den getirilen hareket algılandı verisi ile line serisi oluşturulur ve charta yüklenir.
                    SteplineSeries lineSeriesMotion = new SteplineSeries();
                    lineSeriesMotion.HorizontalAxis = categoricalAxisMotion;
                    this.radChartViewMotion.Series.Add(lineSeriesMotion);
                    LinearAxis verticalAxis = radChartViewMotion.Axes.Get<LinearAxis>(1);
                    verticalAxis.TitleElement.Font = new Font("Arial", 6, FontStyle.Bold);
                    verticalAxis.Minimum = 0;
                    verticalAxis.Maximum = 1;
                    verticalAxis.MajorStep = 1;
                    verticalAxis.Font = new Font("Arial", 6, FontStyle.Bold);
                    lineSeriesMotion.ValueMember = "field3";
                    lineSeriesMotion.CategoryMember = "created_at";
                    lineSeriesMotion.DataSource = list;
                    this.radChartViewMotion.ChartElement.View.Margin = new Padding(0);
                    this.radChartViewMotion.ChartElement.TitleElement.Font = new Font("Arial", 8, FontStyle.Bold);
                    #endregion

                    #region Mongo db'den getirilen uzaklık mesafe verisi ile line serisi oluşturulur ve charta yüklenir.
                    LineSeries lineSeriesDistance = new LineSeries();
                    lineSeriesDistance.HorizontalAxis = categoricalAxisDistance;
                    this.radChartViewDistance.Series.Add(lineSeriesDistance);
                    lineSeriesDistance.ValueMember = "field4";
                    lineSeriesDistance.CategoryMember = "created_at";
                    lineSeriesDistance.DataSource = list;
                    this.radChartViewDistance.ChartElement.View.Margin = new Padding(0);
                    this.radChartViewDistance.ChartElement.TitleElement.Font = new Font("Arial", 8, FontStyle.Bold);
                    LinearAxis horizontalDistance = radChartViewDistance.Axes.Get<LinearAxis>(1);
                    horizontalDistance.Font = new Font("Arial", 6, FontStyle.Bold);
                    #endregion

                    #region  Mongo db'den getirilen hava kalitesi verisi ile line serisi oluşturulur ve charta yüklenir.
                    LineSeries lineSeriesPpm = new LineSeries();
                    lineSeriesPpm.HorizontalAxis = categoricalAxisPpm;
                    this.radChartViewPpm.Series.Add(lineSeriesPpm);
                    lineSeriesPpm.ValueMember = "field5";
                    lineSeriesPpm.CategoryMember = "created_at";
                    lineSeriesPpm.DataSource = list;
                    this.radChartViewPpm.ChartElement.View.Margin = new Padding(0);
                    this.radChartViewPpm.ChartElement.TitleElement.Font = new Font("Arial", 8, FontStyle.Bold);
                    LinearAxis horizontalPpm = radChartViewPpm.Axes.Get<LinearAxis>(1);
                    horizontalPpm.Font = new Font("Arial", 6, FontStyle.Bold); 
                    #endregion

                }

            }
        }


    }
}

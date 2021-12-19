using AdvancedServiceControl;
using log4net;
using SensorModel.Services.Entities;
using SensorModel.Services.Repository;
using SensorModel.WinServices;
using System;
using System.Configuration;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace ServiceModel.WinServices
{
    public class SensorModelComponent : BaseServiceComponent
    {
        /// <summary>
        /// loglama ve mongo repository tanımlamaları yapılır.
        /// </summary>
        public static readonly ILog Log = LogManager.GetLogger(typeof(SensorModelComponent));
        private ResponseRepository responseRepository = null;

        public SensorModelComponent(IServiceComponentContext context) : base(context, "mainLoopTopic")
        {
        }
        protected override void OnStart()
        {
            try
            {
                base.OnStart();
                responseRepository = new ResponseRepository(ConfigurationManager.AppSettings["DbConnection"]);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }
        /// <summary>
        /// windows serviste her zaman çalışan kısımdır.
        /// </summary>
        protected override async void UnitWork()
        {

            #region Thingspeak'den veriler getirilir ve aynı channel id göre kontrol edilerek mongo veri tabanına eklenir.
            Log.Info($"Getting Data From Thingspeak");
            var response = await RestApiCallHelper.Get<Response>();
            if (response != null)
            {
                Log.Info($"Get Data From Thingspeak");
                var channelId = responseRepository.Any(m => m.channel.id == response.channel.id);
                if (channelId)
                    responseRepository.DeleteAll();
                responseRepository.Insert(response);
                Log.Info($"{response.channel.created_at.AddHours(+3)} record inserted");
            } 
            #endregion
        }


    }

}

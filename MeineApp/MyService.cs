using Android.App;
using Android.App.Job;
using Android.Util;
using System.Net.Mqtt;

namespace MeineApp
{
    [Service(Permission = PermissionBind)]
    class MyService : JobService
    {
        private static string TAG = "MyService";
        private MainActivity activity;
        private IMqttClient mqttClient;


        public override bool OnStartJob(JobParameters @params)
        {
            Log.Debug(TAG, "Job started");
            InitializeMqtt(@params);
            return false;
        }


        private byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }

        public override bool OnStopJob(JobParameters @params)
        {
            Log.Debug(TAG, "Job cancelled");
            return true;
        }


        private async void InitializeMqtt(JobParameters @params)
        {
            try
            {
                var configuration = new MqttConfiguration
                {
                    Port = 31395,
                    KeepAliveSecs = 10,
                    WaitTimeoutSecs = 2,
                    MaximumQualityOfService = MqttQualityOfService.ExactlyOnce,
                    AllowWildcardsInTopicFilters = true
                };
                mqttClient = await MqttClient.CreateAsync("piist3.feste-ip.net", configuration);
                await mqttClient.ConnectAsync(new MqttClientCredentials(clientId: "WeckerService"), cleanSession: true);
                MqttApplicationMessage message =
                    new MqttApplicationMessage("home/kitchen/kaffee", StringToByteArray(@params.Extras.GetString("kaffee", "OFF")));
                await mqttClient.PublishAsync(message, MqttQualityOfService.AtMostOnce, true);
                mqttClient.DisconnectAsync();

            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

       
    }
}
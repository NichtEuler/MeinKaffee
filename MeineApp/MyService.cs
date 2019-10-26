using System;
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
        private LogUtils myLog;


        public override bool OnStartJob(JobParameters @params)
        {
            myLog = new LogUtils(true);
            myLog.Log(TAG + "Job started");
            Log.Debug(TAG, "Job started");
            InitializeMqtt(@params);
            JobFinished(@params, false);
            return false;
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
                await mqttClient.ConnectAsync(new MqttClientCredentials(Guid.NewGuid().ToString("N")), cleanSession: true);

                MqttApplicationMessage message =
                            new MqttApplicationMessage(@params.Extras.GetString("path"), StringToByteArray(@params.Extras.GetString("value", "OFF")));
                await mqttClient.PublishAsync(message, MqttQualityOfService.AtLeastOnce, true);

                await mqttClient.DisconnectAsync();

            }
            catch (System.Exception ex)
            {
                myLog.Log(TAG + ex.Message);
                mqttClient.Dispose();
                JobFinished(@params, true);
            }
        }
        private byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }

        public override bool OnStopJob(JobParameters @params)
        {
            myLog.Log(TAG + "Job cancelled");
            Log.Debug(TAG, "Job cancelled");
            return true;
        }
    }
}
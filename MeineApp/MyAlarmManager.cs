using System;
using Android.Content;
using Android.Widget;
using System.Net.Http;
using Android.App;
using Java.Lang;

namespace MeineApp
{
    [BroadcastReceiver]
    class MyAlarmManager : BroadcastReceiver
    {
        HttpClient client = new HttpClient();
        Uri url = new Uri("http://kaffeewecker/toggleled");
        Activity activity = new Activity();
        bool activated = false;

        public bool Activated { get => activated; set => activated = value; }

        public override void OnReceive(Context context, Intent intent)
        {
            if (!activated)
            {
                return;
            }
            //Toast.MakeText(context, "http://192.168.178.55/toggleled", ToastLength.Short).Show();
            PendingResult pendingResult = GoAsync();
            new Thread(async () =>
            {
                try
                { var reqResultAsync = await client.GetStringAsync(url);
                    string checkResult = reqResultAsync.ToString();
                    activity.RunOnUiThread(() =>
                    {
                        Toast.MakeText(context, "Kaffee" + checkResult, ToastLength.Long).Show();

                    });

                    //client.Dispose();
                    pendingResult.Finish();
                }
                catch (System.Exception ex)
                {
                    string checkResult = "Error " + ex.ToString();
                    client.Dispose();
                }
            }).Start();
        }
    }
}
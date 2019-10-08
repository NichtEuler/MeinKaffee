using System;
using Android.Content;
using Android.Widget;
using System.Net.Http;
using Android.App;
using Java.Lang;
using Android.OS;

namespace MeineApp
{
    [BroadcastReceiver]
    class MyAlarmManager : BroadcastReceiver
    {
        HttpClient _client;
        Uri _url;
        Activity activity = new Activity();
        bool activated = true;
        public MyAlarmManager() { }
        public MyAlarmManager(HttpClient client, Uri url)
        {
            this._client = client;
            this._url = url;

        }

        public bool Activated { get => activated; set => activated = value; }

        public override void OnReceive(Context context, Intent intent)
        {
            if (!activated)
            {
                return;
            }

            PendingResult pendingResult = GoAsync();
            new Thread(async () =>
            {
                try
                {
                    string reqResultAsync = await _client.GetStringAsync(_url);
                    
                    activity.RunOnUiThread(() =>
                    {
                        Toast.MakeText(context, "Kaffee" + reqResultAsync, ToastLength.Long).Show();

                    });
                    pendingResult.Finish();
                }
                catch (System.Exception ex)
                {
                    string errorMessage = "Error " + ex.ToString();
                    activity.RunOnUiThread(() =>
                    {
                        Toast.MakeText(context, errorMessage, ToastLength.Long).Show(); // For example
                    });
                    pendingResult.Finish();
                
                }
            }).Start();
        }
    }
}
using System;
using Android.Content;
using Android.Widget;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Android.App;
using Java.Lang;
using Android.OS;
using Xamarin.Forms;

namespace MeineApp
{
    [BroadcastReceiver]
    class MyAlarmManager : BroadcastReceiver
    {
        bool activated = true;
        PowerManager.WakeLock wlo;
        Activity activity = new Activity();

        public bool Activated { get => activated; set => activated = value; }

        
        public override void OnReceive(Context context, Intent intent)
        {
            if (!activated)
            {
                return;
            }
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            PowerManager.WakeLock wl = pm.NewWakeLock(WakeLockFlags.Full, "");
            wlo = wl;
            wlo.Acquire(5000);

            PendingResult pendingResult = GoAsync();
            new Thread(async () =>
            {
                try
                {
                    
                    pendingResult.Finish();
                    taskEnd();
                }
                catch (System.Exception ex)
                {
                    string errorMessage = "Error " + ex.ToString();
                    activity.RunOnUiThread(() =>
                    {
                        Toast.MakeText(context, errorMessage, ToastLength.Long).Show(); // For example
                    });
                    pendingResult.Finish();
                    taskEnd();
                }
            }).Start();


        }
        private void taskEnd()
        {
            wlo.Release();
        }
    }
}
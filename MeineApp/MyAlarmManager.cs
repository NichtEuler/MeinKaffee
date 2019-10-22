using System;
using Android.Content;
using Android.Widget;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Android.App;
using Android.App.Job;
using Java.Lang;
using Android.OS;
using Xamarin.Forms;

namespace MeineApp
{
    [BroadcastReceiver]
    class MyAlarmManager : BroadcastReceiver
    {
        bool activated = true;
        //PowerManager.WakeLock wlo;
        Activity activity = new Activity();
        private Context _context;
        private int i = 1;

        public bool Activated { get => activated; set => activated = value; }


        public override void OnReceive(Context context, Intent intent)
        {
            _context = context;
            if (!activated)
            {
                return;
            }
            //PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            //PowerManager.WakeLock wl = pm.NewWakeLock(WakeLockFlags.Full, "");
            //wlo = wl;
            //wlo.Acquire(5000);
            try
            {
                JobBuilder("home/kitchen/kaffee", "ON");
                JobBuilder("home/kitchen/lights", "ON");
                JobBuilder("home/kitchen/kaffee", "OFF", 5);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
            //wlo.Release();
        }


        private void JobBuilder(string path, string value, int scheduleInMinutes = 0)
        {

            JobScheduler jobScheduler = (JobScheduler)_context.GetSystemService(Context.JobSchedulerService);
            JobInfo.Builder builder =
                new JobInfo.Builder(i++, new ComponentName(_context, Java.Lang.Class.FromType(typeof(MyService))));


            PersistableBundle bundle = new PersistableBundle();
            bundle.PutString("path", path);
            bundle.PutString("value", value);
            builder.SetPersisted(false);
            builder.SetRequiredNetworkType(NetworkType.Any);
            builder.SetExtras(bundle);
            if (scheduleInMinutes != 0)
            {
                builder.SetMinimumLatency(scheduleInMinutes * 1000 * 60);
            }

            jobScheduler.Schedule(builder.Build());
        }
    }
}
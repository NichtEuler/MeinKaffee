using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Java.Lang;

namespace MeineApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            //InititalizeButton();
            this.InitializeAlarm();
        }

        private void InititalizeButton()
        {
            Button button = FindViewById<Button>(Resource.Id.sendIntent);
            button.Click += Button_Click;
        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            Intent myIntent = new Intent(this, typeof(MyAlarmManager)); 
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent,0);

            AlarmManager alarmManager = (AlarmManager) GetSystemService(AlarmService);
            alarmManager.Set(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis()+(10*1000), pendingIntent);
            Toast.MakeText(this, "Sending Intent", ToastLength.Short).Show();

        }

        private void InitializeAlarm()
        {
            MyAlarmManager manager = new MyAlarmManager();
            this.RegisterReceiver(manager, new IntentFilter("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO"));
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, new Intent("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO"), 0);
            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
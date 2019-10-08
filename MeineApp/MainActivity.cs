using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using Java.Lang;
using System;
using System.Net.Http;

namespace MeineApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        MyAlarmManager kaffeeManager;
        SwitchCompat actvKaffee;
        Button button;
        EditText kaffeeTime;
        HttpClient client;
        Uri url;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            client = new HttpClient();
            client.Timeout = new System.TimeSpan(0, 0, 4);
            url = new Uri("http://kaffeewecker/relais2");
            kaffeeManager = new MyAlarmManager(client, url);

            InititalizeButtons();
            InitializeAlarm();
        }
        private void InititalizeButtons()
        {
            actvKaffee = FindViewById<SwitchCompat>(Resource.Id.activateKaffee);
            button = FindViewById<Button>(Resource.Id.sendIntent);
            kaffeeTime = FindViewById<EditText>(Resource.Id.kaffeTime);
            button.Click += Button_Click;
            actvKaffee.Click += ActvKaffee_Click;
            actvKaffee.Checked = kaffeeManager.Activated;
            kaffeeTime.Click += KaffeeTime_Click;
            kaffeeTime.SetOnKeyListener(null);

        }
        private void KaffeeTime_Click(object sender, System.EventArgs e)
        {
            DateTime dateTime;
            TimePickerFragment frag = TimePickerFragment.NewInstance(
                delegate (DateTime time)
                {
                    dateTime = time;
                    kaffeeTime.Text = time.ToString("HH:mm");
                });
            frag.Show(this.FragmentManager, TimePickerFragment.TAG);

        }

        private void ActvKaffee_Click(object sender, System.EventArgs e)
        {
            kaffeeManager.Activated = actvKaffee.Checked;
        }

        private void Button_Click(object sender, System.EventArgs e)
        {

            Toast.MakeText(this, "Sending Intent", ToastLength.Short).Show();
            Uri uri = new Uri("http://kaffeewecker/toggleled");
            SendInstruction(uri);
            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
            Intent myIntent = new Intent("testitest");
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
            alarmManager.Set(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis(), pendingIntent);

        }

        private void SendInstruction(Uri uri)
        {
            new Thread(async () =>
            {
                try
                {
                    var reqResultAsync = await client.GetStringAsync(uri);
                    string checkResult = reqResultAsync.ToString();

                    this.RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Kaffee" + checkResult, ToastLength.Long).Show();

                    });
                }
                catch (System.Exception ex)
                {
                    string checkResult = "Error " + ex.ToString();
                    client.Dispose();
                }
            }).Start();
        }

        private void InitializeAlarm()
        {
            this.RegisterReceiver(kaffeeManager, new IntentFilter("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO"));
            MyAlarmManager myAlarm = new MyAlarmManager(client, new Uri("http://kaffeewecker/relais1"));
            RegisterReceiver(myAlarm, new IntentFilter("testitest"));
            //Intent intent = new Intent("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO");
            //PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, new Intent("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO"), 0);
            //AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
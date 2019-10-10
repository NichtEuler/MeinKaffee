using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Java.Lang;

namespace MeineApp
{
    [Activity(Icon = "@drawable/icon", Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        MyAlarmManager kaffeeManager;
        SwitchCompat actvKaffee;
        Button kaffeeToggle;
        Button lichtToggle;
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
            client.Timeout = new TimeSpan(0, 0, 4);
            url = new Uri("http://kaffeewecker/lichttoggle");
            kaffeeManager = new MyAlarmManager(client, url);

            InititalizeButtons();
            InitializeAlarm();
            InititalizeSubscriptions();
        }


        private async void LichtToggle_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("http://kaffeewecker/lichttoggle");
            lichtToggle.Text = await SendInstruction(uri);

        }

        private async void KaffeeToggle_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("http://kaffeewecker/kaffeetoggle");
            kaffeeToggle.Text = await SendInstruction(uri);
        }

        private void KaffeeTime_Click(object sender, EventArgs e)
        {
            TimePickerFragment frag = TimePickerFragment.NewInstance(
                delegate (DateTime time)
                {
                    kaffeeTime.Text = time.ToString("HH:mm");
                });
            frag.Show(this.FragmentManager, TimePickerFragment.TAG);

        }

        private void ActvKaffee_Click(object sender, EventArgs e)
        {
            kaffeeManager.Activated = actvKaffee.Checked;
        }

        //private async void Button_Click(object sender, System.EventArgs e)
        //{
        //    Toast.MakeText(this, "Sending Intent", ToastLength.Short).Show();
        //    AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
        //    Intent myIntent = new Intent("testitest");
        //    PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
        //    alarmManager.Set(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis() + (50 * 1000), pendingIntent);
        //}

        private async Task<string> SendInstruction(Uri uri)
        {
            string reqResultAsync = null;
            try
            {
                reqResultAsync = await Task.Run(() => client.GetStringAsync(uri));

                this.RunOnUiThread(() =>
                {
                    Toast.MakeText(this, reqResultAsync, ToastLength.Long).Show();

                });
                return reqResultAsync;
            }
            catch (System.Exception)
            {
                string error = "Error";
                this.RunOnUiThread(() =>
                {
                    Toast.MakeText(this, error, ToastLength.Long).Show();
                });
                return error;
            }
        }

        #region Initialization
        private void InititalizeButtons()
        {
            actvKaffee = FindViewById<SwitchCompat>(Resource.Id.activateKaffee);
            actvKaffee.Click += ActvKaffee_Click;
            actvKaffee.Checked = kaffeeManager.Activated;

            kaffeeTime = FindViewById<EditText>(Resource.Id.kaffeTime);
            kaffeeTime.Click += KaffeeTime_Click;
            //kaffeeTime.SetOnKeyListener(null);

            kaffeeToggle = FindViewById<Button>(Resource.Id.kaffeToggle);
            kaffeeToggle.Click += KaffeeToggle_Click;

            lichtToggle = FindViewById<Button>(Resource.Id.lichtToggle);
            lichtToggle.Click += LichtToggle_Click;
        }
        private void InitializeAlarm()
        {
            RegisterReceiver(kaffeeManager, new IntentFilter("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO"), ActivityFlags.ReceiverForeground);
            //MyAlarmManager myAlarm = new MyAlarmManager(client, new Uri("http://kaffeewecker/lichttoggle"));
            //RegisterReceiver(myAlarm, new IntentFilter("testitest"));

        }

        private void InititalizeSubscriptions()
        {
            Xamarin.Forms.MessagingCenter.Subscribe<MyAlarmManager, string>(this, "kaffeetoggle", (sender, e) =>
            {
                this.RunOnUiThread(() =>
                {
                    kaffeeToggle.Text = e;
                });
            });
            Xamarin.Forms.MessagingCenter.Subscribe<MyAlarmManager, string>(this, "lichttoggle", (sender, e) =>
            {
                this.RunOnUiThread(() =>
                {
                    lichtToggle.Text = e;
                });
            });
        }
        #endregion

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
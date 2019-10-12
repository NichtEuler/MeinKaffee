using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using System;
using System.Net.Http;
using System.Net.Mqtt;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Java.Lang;
using Xamarin.Essentials;

namespace MeineApp
{
    [Activity(Icon = "@drawable/icon", Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MyAlarmManager kaffeeManager;
        private SwitchCompat actvKaffee;
        private Button kaffeeToggle;
        private Button lichtToggle;
        private Button testButton;
        private EditText kaffeeTime;
        private HttpClient client;
        private Uri url;
        private IMqttClient mqttClient;
        private SessionState sessionState;

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
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            InititalizeButtons();
            InitializeMQTT();
            InitializeAlarm();
            //InititalizeSubscriptions();
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            InitializeMQTT();
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
        private async void TestButton_Click(object sender, EventArgs e)
        {
            try
            {

                MqttApplicationMessage message = new MqttApplicationMessage("home/garden/fountain", StringToByteArray("Hallo Vom Handy"));

                await mqttClient.PublishAsync(message, MqttQualityOfService.AtLeastOnce, true);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        private byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }

        private string ByteArrayToString(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }

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

            testButton = FindViewById<Button>(Resource.Id.testButton);
            testButton.Click += TestButton_Click;
        }

        private async void InitializeMQTT()
        {
            var configuration = new MqttConfiguration
            {
                Port = 31395,
                KeepAliveSecs = 10,
                WaitTimeoutSecs = 2,
                MaximumQualityOfService = MqttQualityOfService.AtMostOnce,
                AllowWildcardsInTopicFilters = true
            };
            mqttClient = await MqttClient.CreateAsync("piist3.feste-ip.net", configuration);
            sessionState = await mqttClient.ConnectAsync(new MqttClientCredentials(clientId: "foo"), cleanSession: true);
            InititalizeSubscriptions();
        }


        private void InitializeAlarm()
        {
            RegisterReceiver(kaffeeManager, new IntentFilter("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO"), ActivityFlags.ReceiverForeground);
            //MyAlarmManager myAlarm = new MyAlarmManager(client, new Uri("http://kaffeewecker/lichttoggle"));
            //RegisterReceiver(myAlarm, new IntentFilter("testitest"));

        }

        private async void InititalizeSubscriptions()
        {
            await mqttClient.SubscribeAsync("/home/lights/kitchen", MqttQualityOfService.AtMostOnce);
            await mqttClient.SubscribeAsync("/home/lights/kitchen1", MqttQualityOfService.AtMostOnce);

            mqttClient.MessageStream.Where(msg => msg.Topic == "/home/lights/kitchen").Subscribe(kitchen);
            mqttClient.MessageStream.Where(msg => msg.Topic == "/home/lights/kitchen1").Subscribe(kitchen1);

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


        private void kitchen1(MqttApplicationMessage obj)
        {
            this.RunOnUiThread(() =>
            {
                kaffeeToggle.Text = "Kaffeemaschine :" + ByteArrayToString(obj.Payload);
            });
        }

        private void kitchen(MqttApplicationMessage obj)
        {
            this.RunOnUiThread(() =>
            {
                kaffeeToggle.Text = "Eyyyy";
            });
            this.RunOnUiThread(() => Toast.MakeText(this, obj.Topic, ToastLength.Long).Show());
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
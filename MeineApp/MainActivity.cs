using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mqtt;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App.Job;
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
        private SessionState sessionState;
        private MyService myService;
        private LogUtils myLogger;
        public IMqttClient mqttClient;

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            myLogger = new LogUtils();
            kaffeeManager = new MyAlarmManager();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;


            InititalizeButtons();
            InitializeAlarm();
        }



        //private void MqttClient_Disconnected(object sender, MqttEndpointDisconnected e)
        //{
        //    myLogger.Log("Client disconnected, retrying");
        //    InitializeMQTT();
        //}

        //also works with clients disconnect handler
        protected override void OnResume()
        {
            myLogger.Log("Resuming app");
            base.OnResume();
            try
            {
                if (mqttClient == null)
                {
                    myLogger.Log("Client not connected");
                    InitializeMQTT();
                }
                else if (!mqttClient.IsConnected)
                {

                    myLogger.Log("Client reconnecting");
                    mqttConnect();
                }
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
            }
        }


        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            myLogger.Log("Connectivity changed");
            if (!mqttClient.IsConnected)
            {
                mqttClient.DisconnectAsync();
                myLogger.Log("Connection to MQTT lost, reconnecting");
                mqttConnect();
            }
        }

        private async void LichtToggle_Click(object sender, EventArgs e)
        {
            try
            {
                MqttApplicationMessage message = new MqttApplicationMessage("home/kitchen/lights", StringToByteArray("toggle"));

                await mqttClient.PublishAsync(message, MqttQualityOfService.AtLeastOnce, false);
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
            }

        }

        private async void KaffeeToggle_Click(object sender, EventArgs e)
        {
            try
            {
                MqttApplicationMessage message = new MqttApplicationMessage("home/kitchen/kaffee", StringToByteArray("toggle"));
                await mqttClient.PublishAsync(message, MqttQualityOfService.AtLeastOnce, false);
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
            }
        }

        private void KaffeeTime_Click(object sender, EventArgs e)
        {
            try
            {
                TimePickerFragment frag = TimePickerFragment.NewInstance(
                        delegate (DateTime time)
                        {
                            kaffeeTime.Text = time.ToString("HH:mm");
                        });
                frag.Show(this.FragmentManager, TimePickerFragment.TAG);

            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
            }
        }

        private void ActvKaffee_Click(object sender, EventArgs e)
        {
            kaffeeManager.Activated = actvKaffee.Checked;
        }


        private async void TestButton_Click(object sender, EventArgs e)
        {
            try
            {

                MqttApplicationMessage message = new MqttApplicationMessage("home/garden/fountain", StringToByteArray("Hallo Vom Handy"));
                await mqttClient.PublishAsync(message, MqttQualityOfService.AtLeastOnce, false);

                AlarmManager alarmManager = (AlarmManager) GetSystemService(AlarmService);
                Intent myIntent = new Intent("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO");
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, myIntent, 0);
                alarmManager.Set(AlarmType.RtcWakeup, JavaSystem.CurrentTimeMillis(), pendingIntent);
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
            }
        }

        private byte[] StringToByteArray(string str)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetBytes(str);
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
                return null;
            }
        }

        private string ByteArrayToString(byte[] arr)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetString(arr);
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
                return null;
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

            myLogger.Log("Buttons Initialized");
        }

        private async void InitializeMQTT()
        {
            await semaphoreSlim.WaitAsync();
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
                await mqttConnect();

                InititalizeSubscriptions();
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task mqttConnect()
        {
            MqttLastWill lastWill = new MqttLastWill("home/kitchen/lights", MqttQualityOfService.AtLeastOnce, true,
                StringToByteArray("OFF"));
            sessionState = await mqttClient.ConnectAsync(new MqttClientCredentials(Guid.NewGuid().ToString("N")), lastWill,
                cleanSession: true);
            myLogger.Log("Session created, Sessionstate: " + sessionState.ToString());
        }


        private void InitializeAlarm()
        {
            RegisterReceiver(kaffeeManager, new IntentFilter("com.urbandroid.sleep.alarmclock.ALARM_ALERT_START_AUTO"), ActivityFlags.ReceiverForeground);
        }

        private async void InititalizeSubscriptions()
        {
            try
            {
                mqttClient.MessageStream.Where(msg => msg.Topic == "home/kitchen/lights").Subscribe(kitchenLightReceived);
                mqttClient.MessageStream.Where(msg => msg.Topic == "home/kitchen/kaffee").Subscribe(kaffeeReceived);

                await mqttClient.SubscribeAsync("home/kitchen/lights", MqttQualityOfService.AtMostOnce);
                await mqttClient.SubscribeAsync("home/kitchen/kaffee", MqttQualityOfService.AtMostOnce);

            }
            catch (System.Exception ex)
            {

                myLogger.Log(ex.Message);
                throw ex;
            }

        }

        private void kaffeeReceived(MqttApplicationMessage obj)
        {
            try
            {
                string value = ByteArrayToString(obj.Payload);

                if (!value.Equals("toggle"))
                {
                    this.RunOnUiThread(() => { kaffeeToggle.Text = "Kaffeemaschine: " + value; });
                }
            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);

                throw ex;
            }

        }
        #endregion


        private void kitchenLightReceived(MqttApplicationMessage obj)
        {
            try
            {
                string value = ByteArrayToString(obj.Payload);
                if (!value.Equals("toggle"))
                {
                    this.RunOnUiThread(() => { lichtToggle.Text = "Licht: " + value; });
                }

            }
            catch (System.Exception ex)
            {
                myLogger.Log(ex.Message);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MeineApp
{
    class MyService : IntentService
    {
        
        protected override void OnHandleIntent(Intent intent)
        {
            string input = intent.GetStringExtra("inputExtra");
        }
    }
}
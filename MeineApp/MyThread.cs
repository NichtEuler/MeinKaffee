using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MeineApp
{
    class MyThread
    {
        string result;
        Thread thread;
        public string Result
        {
            get
            {
                thread.Join();
                return result;
            }
        }
        public void Start()
        {

        }
    }
}
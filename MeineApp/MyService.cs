using Android.App.Job;

namespace MeineApp
{
    class MyService : JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            throw new System.NotImplementedException();
        }
    }
}
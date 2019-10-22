using System;
namespace MeineApp
{
    public class LogUtils
    {
        String logFilePath = null;

        public LogUtils()
        {
            String path = Android.OS.Environment.ExternalStorageDirectory.Path;
            logFilePath = System.IO.Path.Combine(path, "MeineApp.log.txt");
            if (System.IO.File.Exists(logFilePath))
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilePath, false))
                {
                    writer.WriteLine("Starting logging at " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }
        }

        public void Log(String message)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilePath, true))
            {
                writer.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : " + message);
            }
        }
    }
}
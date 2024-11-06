using Peak.Can.Basic;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace CanGPSLogger
{
    internal class FileWriter: IEnableLogger, IFileWriter
    {
        private StreamWriter? writer;
        string path = @".\DataLogger\";
        string fileName = String.Empty;
        string timestamp = String.Empty;
        int counter = 0;

        public FileWriter()
        {
            timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = @$".\DataLogger\{timestamp}\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                NewFile();
            }
            this.Log().Warn($"New DataLogger started");
        }

        public void LogCanMessage(PcanMessage message) 
        {

            message.ID.ToString('H');
            writer?.WriteLine(line);
            writer?.Flush();
        }

        public void LogPosition(NmeaParser.Messages.Rmc rmc)
        {
            writer?.WriteLine(line);
            writer?.Flush();
        }


        private void NewFile()
        {
            try
            {
                writer?.Flush();
                writer?.Close();
            }
            catch (System.ObjectDisposedException ex)
            {

            }


            if (File.Exists(path + fileName))
            {
                string new_name = fileName.Substring(0, fileName.Length - 1);
                File.Move(path + fileName, path + new_name);
                this.Log().Warn($"DataLogger file created {new_name}");

            }
            fileName = $"{timestamp}_DataLogger_{counter}.csv_";
            writer = new(path + fileName);
            counter++;

        }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (-(date - DateTime.Now)).TotalSeconds;

            var res = $"{secsAgo}";

            return res;

        }
    }
}

using MCU_CAN_AV.Devices;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MCU_CAN_AV.utils
{
    internal class DataLogger : IDataLogger, IEnableLogger
    {

        private string? _Log_header = null;
        private StreamWriter? writer;
        private int log_file_cnt = 0;
        string fileName = String.Empty;
        string timestamp = String.Empty;
        int counter = 0;
        IDisposable? Disposable;
        string path = @".\DataLogger\";

        bool _IsPaused = false;

        public DataLogger()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void close()
        {
            if(writer != null)
            {
                this.Log().Warn($"DataLogger closed");
            }
            Disposable?.Dispose();
            counter = 0;
            _IsPaused = true;
            writer?.Close();
            path = @".\DataLogger\";
            
        }

        public void pause()
        {
            _IsPaused = true;   
        }

        public void resume()
        {
            _IsPaused = false;
        }

        public void start(IDevice Device)
        {
            close();
            timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = @$".\DataLogger\{timestamp}\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            NewFile();
            _IsPaused = false;
            this.Log().Warn($"New DataLogger started");
            Disposable = Device.RxData.Subscribe(

             (_) =>
             {
                 if (!_IsPaused)
                 {
                     if (_Log_header == null)
                     {
                         foreach (var item in Device.DeviceDescription)
                         {
                             if (_Log_header != null) _Log_header += "; ";
                             _Log_header += $"{item.Name}";
                         }

                         writer?.WriteLine(_Log_header);
                     }

                     string? line = null;
                     foreach (var item in Device.DeviceDescription)
                     {
                         if (line != null) line += "; ";
                         line += item.ValueNow.ToString();
                     }
                     writer?.WriteLine(line);
                     writer?.Flush();

                     long fileSizeibBytes = utils.GetFileSize(path + fileName);
                     if (fileSizeibBytes > 5242880)
                     {
                         NewFile();
                     }
                 }
             });
        }

        private void NewFile()
        {
            try {
                writer?.Flush();
                writer?.Close();
            }
            catch (System.ObjectDisposedException ex) { 
                
            }
            

            if (File.Exists(path + fileName))
            {
                string new_name = fileName.Substring(0, fileName.Length - 1);
                File.Move(path + fileName, path + new_name);
                this.Log().Warn($"DataLogger new file created {new_name}");
            }
            fileName = $"{timestamp}_DataLogger_{counter}.csv_";
            writer = new(path+fileName);
            counter++;
            
        }
    }
}

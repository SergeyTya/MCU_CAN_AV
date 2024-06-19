using MCU_CAN_AV.Devices;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
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


            if (writer != null)
            {
                this.Log().Warn($"DataLogger closed");
            }
            Disposable?.Dispose();
            counter = 0;
            _IsPaused = true;
            writer?.Close();


            if (File.Exists(path + fileName))
            {
                string new_name = fileName.Substring(0, fileName.Length - 1);
                File.Move(path + fileName, path + new_name);
                this.Log().Warn($"DataLogger file created {new_name}");
            }

            path = @".\DataLogger\";
            _Log_header = null;

        }

        public void pause()
        {
            _IsPaused = true;   
        }

        public void resume()
        {
            _IsPaused = false;
        }

        private DateTime start_point;
        private int tras_id = -1;
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
                     if (_.transaction_id > 0) { // check if it is the same transaktion
                         if (_.transaction_id == tras_id) return;
                         tras_id = _.transaction_id;
                     }

                     if (_Log_header == null)
                     {
                         _Log_header = "Time, s";
                         foreach (var item in Device.DeviceDescription)
                         {
                             if (_Log_header != null) _Log_header += "; ";
                             _Log_header += $"{item.Name}";
                         }

                         writer?.WriteLine(_Log_header);
                         start_point = DateTime.Now;
  
                     }



                     string? line = Formatter(start_point);

                     //Debug.WriteLine("Logger tiks -> "+line);
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
                         _Log_header = null;
                         NewFile();
                     }
                 }
             });
        }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (-(date - DateTime.Now)).TotalSeconds;

            var res = $"{secsAgo}";

            return res;
           
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
                this.Log().Warn($"DataLogger file created {new_name}");
            }
            fileName = $"{timestamp}_DataLogger_{counter}.csv_";
            writer = new(path+fileName);
            counter++;
            
        }

        public async void  openPath() {

            var pathx = path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using Process fileOpener = new Process();
                fileOpener.StartInfo.FileName = "explorer";
                fileOpener.StartInfo.Arguments = "/select," + pathx + "\"";
                fileOpener.Start();
                await fileOpener.WaitForExitAsync();
                return;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                using Process fileOpener = new Process();
                fileOpener.StartInfo.FileName = "explorer";
                fileOpener.StartInfo.Arguments = "-R " + pathx;
                fileOpener.Start();
                await fileOpener.WaitForExitAsync();
                return;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using Process dbusShowItemsProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dbus-send",
                        Arguments = "--print-reply --dest=org.freedesktop.FileManager1 /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"file://" + path + "\" string:\"\"",
                        UseShellExecute = true
                    }
                };
                dbusShowItemsProcess.Start();
                await dbusShowItemsProcess.WaitForExitAsync();

                if (dbusShowItemsProcess.ExitCode == 0)
                {
                    // The dbus invocation can fail for a variety of reasons:
                    // - dbus is not available
                    // - no programs implement the service,
                    // - ...
                    return;
                }
            }

            using Process folderOpener = new Process();
            folderOpener.StartInfo.FileName = Path.GetDirectoryName(path);
            folderOpener.StartInfo.UseShellExecute = true;
            folderOpener.Start();
            await folderOpener.WaitForExitAsync();
        }
    }
}

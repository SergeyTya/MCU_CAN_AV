using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using MCU_CAN_AV.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using DynamicData.Binding;
//using ReactiveUI;
using System.Reactive.Subjects;

namespace MCU_CAN_AV.DeviceDescriprion
{


    //public class DeviceDescriptionReader
    //{
        

    //    private static readonly DeviceDescriptionReader instance = new DeviceDescriptionReader();

    //    private DeviceDescriptionReader()
    //    {
    //        Init();
    //    }

    //    private static string read_jsonres(byte[] res)
    //    {
    //        MemoryStream MS = new MemoryStream(res);
    //        StreamReader sr = new StreamReader(MS);
    //        string fileContents = sr.ReadToEnd();
    //        sr.Close();
    //        return fileContents;
    //    }

        
        
    //    //public static void Init()
    //    //{
    //    //    List<DeviceParameter> tmp = new();
    //    //    try
    //    //    {
    //    //        string fileContents = read_jsonres(Resources.shanghai_description);
    //    //        tmp =  JsonConvert.DeserializeObject<List<DeviceParameter>>(fileContents);

    //    //        IDeviceReader.DeviceDescription = new ObservableCollection<IDeviceParameter>(tmp);
    //    //    }
    //    //    catch (JsonReaderException e)
    //    //    {
    //    //        Debug.WriteLine(e);
    //    //    }
    //    //}
    //}

   
}

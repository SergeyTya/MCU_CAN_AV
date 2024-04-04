using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.utils
{
    public class utils
    {
        public static string ReadJsonFromResources(byte[] res)
        {
            MemoryStream MS = new MemoryStream(res);
            StreamReader sr = new StreamReader(MS);
            string fileContents = sr.ReadToEnd();
            sr.Close();
            return fileContents;
        }

        public static long GetFileSize(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                return new FileInfo(FilePath).Length;
            }
            return 0;
        }

        public static bool CopySliceTo(BitArray dst, int dst_offset, BitArray src, int src_offset, int length)
        {

            for (int i = 0; i < length; i++)
            {
                dst[dst_offset + i] = src[src_offset + i];
            }

            return true;
        }


        public static BitArray CopySlice(BitArray source, int offset, int length)
        {
            // Urgh: no CopyTo which only copies part of the BitArray
            BitArray ret = new BitArray(length);
            for (int i = 0; i < length; i++)
            {
                ret[i] = source[offset + i];
            }
            return ret;
        }



    }
}

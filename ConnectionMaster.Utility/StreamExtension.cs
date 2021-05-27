using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionMaster.Utility
{
    public static class StreamExtension
    {
        public static async Task<byte[]> ReadBlockAsync(this Stream stream,int length)
        {
            var buffer = new byte[length];
            int offset = 0;
            int readed;
            while(offset < length)
            {
                readed = await stream.ReadAsync(buffer, offset, buffer.Length - offset);
                if(readed == 0)
                {
                    break;
                }
                offset += readed;
            }
            var result = new byte[offset];
            Array.Copy(buffer, result, result.Length);
            return result;
        }
    }
}

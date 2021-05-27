using ConnectionMaster.Nat.Messages;
using ConnectionMaster.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionMaster.Nat
{
    public static class MessageTranslator
    {
        private static readonly ByteConverter converter = ByteConverter.BigEndianConverter;

        public static byte[] TranslateMessage(NatMessage message)
        {
            var bytes = Enumerable.Empty<byte>()
                .Append((byte)message.MessageType);
            switch(message.MessageType)
            {
                case NatMessageType.Join:
                    var joinMessage = (JoinMessage)message;
                    bytes = bytes.Concat(Translate(joinMessage.ClientId));
                    break;
                case NatMessageType.FindClient:
                    var findClientMessage = (FindClientMessage)message;
                    bytes = bytes.Concat(Translate(findClientMessage.ClientId));
                    break;
                case NatMessageType.FindClientResponse:
                    var findClientResponseMessage = (FindClientResponseMessage)message;
                    if (findClientResponseMessage.EndPoint == null)
                    {
                        bytes = bytes.Append((byte)0);
                    }
                    else
                    {
                        bytes = bytes.Append((byte)1)
                            .Concat(Translate(findClientResponseMessage.EndPoint));
                    };
                    break;
                case NatMessageType.ConnectClient:
                    var connectMessage = (ConnectClientMessage)message;
                    bytes = bytes.Concat(Translate(connectMessage.ClientId));
                    break;
            }
            return bytes.ToArray();
        }

        public static async Task<NatMessage> TranslateStreamAsync(Stream stream)
        {
            var messageType = (NatMessageType)stream.ReadByte();
            NatMessage message;
            switch(messageType)
            {
                case NatMessageType.Join:
                    message = new JoinMessage(await TranslateStringAsync(stream));
                    break;
                case NatMessageType.FindClient:
                    message = new FindClientMessage(await TranslateStringAsync(stream));
                    break;
                case NatMessageType.FindClientResponse:
                    var hasResult = stream.ReadByte() == 1;
                    var endPoint = hasResult ? await TranslateEndPointAsync(stream) : null;
                    message = new FindClientResponseMessage(endPoint);
                    break;
                case NatMessageType.ConnectClient:
                    message = new ConnectClientMessage(await TranslateStringAsync(stream));
                    break;
                default:
                    throw new UnknowMessageException();
            }
            return message;
        }

        private static async Task<int> TranslateInt32Async(Stream stream)
        {
            var bytes = await stream.ReadBlockAsync(4);
            return converter.ToInt32(bytes);
        }

        private static async Task<string> TranslateStringAsync(Stream stream)
        {
            var length = await TranslateInt32Async(stream);
            if(length == 0)
            {
                return null;
            }
            var bytes = await stream.ReadBlockAsync(length);
            return Encoding.UTF8.GetString(bytes);
        }

        private static string TranslateString(byte[] bytes,ref int offset)
        {
            var length = converter.ToInt32(bytes, offset);
            offset += 4;
            if(length ==0)
            {
                return null;
            }
            var text = Encoding.UTF8.GetString(bytes, offset, length);
            offset += length;
            return text;
        }

        private static IPEndPoint TranslateEndPoint(byte[] data,ref int offset)
        {
            var ip = IPAddress.Parse(string.Join(".",data.Skip(offset).Take(4)));
            offset += 4;
            var port = converter.ToInt16(data, offset);
            offset += 2;
            return new IPEndPoint(ip, port);
        }

        private static async Task<IPEndPoint> TranslateEndPointAsync(Stream stream)
        {
            var ipBytes =await stream.ReadBlockAsync(4);
            var ip = IPAddress.Parse(string.Join(".", ipBytes));
            var portBytes =await stream.ReadBlockAsync(2);
            var port = converter.ToInt16(portBytes);
            return new IPEndPoint(ip, port);
        }

        private static IEnumerable<byte> Translate(string text)
        {
            var bytes = Enumerable.Empty<byte>();
            if (string.IsNullOrEmpty(text))
            {
                bytes = bytes.Concat(Translate(0));
            }
            else
            {
                bytes = bytes
                    .Concat(Translate(text.Length))
                    .Concat(Encoding.UTF8.GetBytes(text));
            }
            return bytes;
        }

        private static IEnumerable<byte> Translate(int value)
        {
            return converter.GetBytes(value);
        }

        private static IEnumerable<byte> Translate(IPEndPoint point)
        {
            return point.Address.MapToIPv4().ToString().Split('.').Select(text=>byte.Parse(text))
                .Concat(converter.GetBytes((short)point.Port));
        }
    }
}

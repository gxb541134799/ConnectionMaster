using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.Converters
{
    public class HandshakeConverter : EnumTypeConverter<Handshake>
    {
        protected override void ConfigMap()
        {
            Maps.Add(Handshake.None, "无");
            Maps.Add(Handshake.XOnXOff, Handshake.XOnXOff.ToString());
            Maps.Add(Handshake.RequestToSend, Handshake.RequestToSend.ToString());
            Maps.Add(Handshake.RequestToSendXOnXOff, Handshake.RequestToSendXOnXOff.ToString());
        }
    }
}

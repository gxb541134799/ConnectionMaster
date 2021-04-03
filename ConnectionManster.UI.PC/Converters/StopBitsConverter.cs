using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.Converters
{
    public class StopBitsConverter : EnumTypeConverter<StopBits>
    {
        protected override void ConfigMap()
        {
            Maps.Add(StopBits.One, "1");
            Maps.Add(StopBits.OnePointFive, "1.5");
            Maps.Add(StopBits.Two, "2");
        }
    }
}

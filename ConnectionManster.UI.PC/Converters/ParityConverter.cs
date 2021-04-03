using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.Converters
{
    public class ParityConverter : EnumTypeConverter<Parity>
    {
        protected override void ConfigMap()
        {
            Maps.Add(Parity.None, "无");
            Maps.Add(Parity.Odd, "奇校验");
            Maps.Add(Parity.Even, "偶校验");
            Maps.Add(Parity.Mark, "标记");
            Maps.Add(Parity.Space, "空白");
        }
    }
}

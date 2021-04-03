using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC
{
    public static class Validator
    {
        public static bool IsIP(string text)
        {
            return !string.IsNullOrEmpty(text) && Regex.IsMatch(text, @"^\d{1,3}(\.\d{1,3}){3}$");
        }

        public static bool IsValidPort(int port)
        {
            return port >=IPEndPoint.MinPort && port <=IPEndPoint.MaxPort;
        }
    }
}

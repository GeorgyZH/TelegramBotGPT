using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestChat2
{
    public static class AppSettingsExtentions
    {
        public static string[] ToFormatedString(this AppSettings appSettings)
        {
            return new string[] {appSettings.keyGPT, appSettings.keyTG };
        }
    }
}

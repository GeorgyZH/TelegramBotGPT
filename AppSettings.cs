using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestChat2
{
    public class AppSettings
    {
        
        public string keyGPT { get; set; } = "";

        
        public string keyTG { get; set; } = "";
    }
}

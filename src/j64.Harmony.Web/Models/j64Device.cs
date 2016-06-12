using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.Harmony.Web.Models
{
    public class j64Device
    {
        public string Name { get; set; }
        public string DeviceValue { get; set; }

        public j64DeviceType DeviceType { get; set; }
    }

    public enum j64DeviceType
    {
        Volume,
        Surfing,
        Channel,
        VCR
    }
}

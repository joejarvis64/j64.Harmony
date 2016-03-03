using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.Harmony.Xmpp
{
    public class Device
    {
        public string label { get; set; }
        public int id { get; set; }
        public List<ControlGroup> controlGroup { get; set; }
    }
}

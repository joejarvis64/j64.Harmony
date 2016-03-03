using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace j64.Harmony.Xmpp
{
    public class Function
    {
        public string name { get; set; }
        public string label { get; set; }
        public string action { get; set; }
        public Action jsonAction
        {
            get
            {
                return JsonConvert.DeserializeObject<Action>(action);
            }
        }
    }
}

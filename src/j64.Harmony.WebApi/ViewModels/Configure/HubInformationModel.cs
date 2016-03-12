using System;

namespace j64.Harmony.WebApi.ViewModels.Configure
{
    public class HubInformationModel
    {
        // Harmony Hub Address/Login Info
        public String HarmonyHubAddress { get; set; } = "HarmonyHub.attlocal.net";
        public int HarmonyHubPort { get; set; } = 5222;
        public String HarmonyEmail { get; set; }
        public String HarmonyPassword { get; set; }


        // Smart Things Hub Address
        public String StHubAddress { get; set; } = "";
        public Int32 StHubPort { get; set; } = 0;


        // j64 Server Info
        public String j64Address { get; set; } = "";
        public Int32 j64Port { get; set; } = 2065;
        public String j64AppId { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace j64.Harmony.WebApi.ViewModels.Configure
{
    public class HubsAndGatewaysViewModel
    {
        // Harmony Hub Address/Login Info
        [Required]
        public String HarmonyHubAddress { get; set; }

        [Required]
        public int HarmonyHubPort { get; set; }

        [Required]
        [EmailAddress]
        public String HarmonyEmail { get; set; }

        public String HarmonyPassword { get; set; }


        // Smart Things Hub Address
        [Required]
        public String StHubAddress { get; set; }
        [Required]
        public Int32 StHubPort { get; set; }


        // j64 Server Info
        [Required]
        public String j64Address { get; set; }
        [Required]
        public Int32 j64Port { get; set; }
    }
}

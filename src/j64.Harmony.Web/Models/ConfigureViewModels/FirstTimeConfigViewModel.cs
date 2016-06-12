using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace j64.Harmony.Web.ViewModels.Configure
{
    public class FirstTimeConfigViewModel
    {
        [Required]
        public String HarmonyHubAddress { get; set; }

        [Required]
        public int HarmonyHubPort { get; set; }

        [Required]
        [EmailAddress]
        public String HarmonyEmail { get; set; }

        public String HarmonyPassword { get; set; }

        public bool IsConnected { get; set; } = false;

        public String VolumeDevice { get; set; }

        public String ChannelDevice { get; set; }

        #region Helpers for the device drop downs
        public List<SelectListItem> VolumeDeviceList { get; set; }
        public List<SelectListItem> ChannelDeviceList { get; set; }
        #endregion
    }
}

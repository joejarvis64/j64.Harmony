using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace j64.Harmony.WebApi.ViewModels.Configure
{
    public class DeviceNamesViewModel
    {
        #region Harmony Devices to Use
        [Required]
        public String VolumeDevice { get; set; }

        [Required]
        public String ChannelDevice { get; set; }

        [Required]
        public Int32 ChanneKeyPauseInterval { get; set; }
        #endregion

        #region Device Names
        [Required]
        public String SoundDeviceName { get; set; }

        [Required]
        public String ChannelSurfDeviceName { get; set; }

        [Required]
        public String LastChannelDeviceName { get; set; }

        [Required]
        public String VcrPauseDeviceName { get; set; }
        #endregion

        #region Helpers for the device drop downs
        public List<SelectListItem> VolumeDeviceList { get; set; }
        public List<SelectListItem> ChannelDeviceList { get; set; }
        #endregion
    }
}

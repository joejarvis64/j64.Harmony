using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace j64.Harmony.WebApi.ViewModels.Configure
{
    public class FavoriteChannelViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string ChannelNumber { get; set; }
    }
}

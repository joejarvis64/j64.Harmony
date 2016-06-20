using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace j64.Harmony.Web.ViewModels.Configure
{
    public class CustomCommandViewModel
    {
        public string OriginalCommandName { get; set; }

        /// <summary>
        /// The name of the command
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string CommandName { get; set; }

        /// <summary>
        /// The list of commands to issue as part of this custom command
        /// </summary>
        [Required]
        public List<CustomCommand> commands = new List<CustomCommand>();
    }

    public class CustomActionViewModel
    {
        public string Command { get; set; }
        public int Sequence { get; set; }
        [Required]
        public string Device { get; set; }
        [Required]
        public string ControlGroup { get; set; }
        [Required]
        public string Function { get; set; }


        // ***********************
        // -- drop down helpers --
        // ***********************
        public List<j64.Harmony.Xmpp.Device> Devices { get; set; }
    }

    public class CustomCommand
    { 
        public int Sequence { get; set; }
        [Required]
        public string Device { get; set; }
        [Required]
        public string ControlGroup { get; set; }
        [Required]
        public string Function{ get; set; }
    }
}

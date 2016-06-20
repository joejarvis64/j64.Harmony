using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using j64.Harmony.Web.ViewModels.Configure;
using j64.Harmony.Web.Models;
using j64.Harmony.Xmpp;
using j64.Harmony.Web.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace j64.Harmony.Web.Controllers
{
    public class CustomCommandController : Controller
    {
        private j64HarmonyGateway j64Config = null;
        private Hub myHub = null;

        #region Constructor
        public CustomCommandController(j64HarmonyGateway j64Config, Hub hub)
        {
            this.j64Config = j64Config;
            myHub = hub;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            // If not connected to the smart hub go to the configure page
            if (myHub.hubConfig == null || String.IsNullOrEmpty(j64Config.ChannelDevice) || String.IsNullOrEmpty(j64Config.VolumeDevice))
                return RedirectToAction("Edit", "FirstTimeConfig");

            if (String.IsNullOrEmpty(OauthRepository.Get().accessToken))
                return RedirectToAction("Index", "Oauth");

            return View(CreateCustomCommandView());
        }

        private List<CustomCommandViewModel> CreateCustomCommandView()
        {
            List<CustomCommandViewModel> l = new List<CustomCommandViewModel>();
            foreach (var customCommand in this.j64Config.CustomCommands)
            {
                var ccvm = new CustomCommandViewModel()
                {
                    CommandName = customCommand.CommandName
                };

                int i = 1;
                foreach (var ca in customCommand.Actions)
                {
                    var cc = new ViewModels.Configure.CustomCommand()
                    {
                        Sequence = i,
                        Device = ca.Device,
                        ControlGroup = ca.Group,
                        Function = ca.Function
                    };
                    ccvm.commands.Add(cc);
                    i++;
                }

                l.Add(ccvm);
            }

            return l;
        }
        #endregion

        #region Add new command
        public IActionResult CreateCommand(string command)
        {
            int i = 0;
            foreach (var cmd in j64Config.CustomCommands)
            {
                if (cmd.CommandName.StartsWith("New Command"))
                    i++;
            }
            string cmdName = "New Command";
            if (i > 0)
                cmdName += " " + i.ToString();

            var cc = new CustomCommandViewModel()
            {
                OriginalCommandName = cmdName,
                CommandName = cmdName
            };

            return View("EditCommand", cc);
        }
        #endregion

        #region Edit Command
        public IActionResult EditCommand(string command)
        {
            var customCommand = j64Config.CustomCommands.Find(x => x.CommandName == command);
            if (customCommand == null)
                return View("Index", CreateCustomCommandView());

            var cc = new CustomCommandViewModel()
            {
                OriginalCommandName = customCommand.CommandName,
                CommandName = customCommand.CommandName
            };
            return View(cc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCommand(CustomCommandViewModel command)
        {
            var customCommand = j64Config.CustomCommands.Find(x => x.CommandName == command.OriginalCommandName);
            if (customCommand == null)
            {
                customCommand = new Models.CustomCommand()
                {
                    CommandName = command.CommandName
                };
                j64Config.CustomCommands.Add(customCommand);
            }
            customCommand.CommandName = command.CommandName;

            for (int i = 0; i < customCommand.Actions.Count; i++)
                customCommand.Actions[i].Sequence = i + 1;
            j64HarmonyGatewayRepository.Save(j64Config);

            return View("Index", CreateCustomCommandView());
        }
        #endregion

        #region Delete Command
        public IActionResult DeleteCommand(string command)
        {
            var customCommand = j64Config.CustomCommands.Find(x => x.CommandName == command);
            if (customCommand != null)
                j64Config.CustomCommands.Remove(customCommand);

            return View("Index", CreateCustomCommandView());
        }
        #endregion

        #region Add Action
        public IActionResult AddNewAction(string command, int sequence)
        {
            var customCommand = j64Config.CustomCommands.Find(x => x.CommandName == command);

            var cav = new CustomActionViewModel()
            {
                Command = command,
                Sequence = sequence,
            };

            cav.Devices = myHub.hubConfig.device;

            return View(cav);
        }

        public IActionResult SelectNewAction(string command, int sequence, string device, string group, string function)
        {
            var selectedDevice = myHub.hubConfig.device.Find(x => x.label == device);
            var selectedGroup = selectedDevice?.controlGroup.Find(x => x.name == group);
            var selectedFunction = selectedGroup?.function.Find(x => x.label == function);

            var customCommand = j64Config.CustomCommands.Find(x => x.CommandName == command);
            var customActionIndex = customCommand?.Actions.FindIndex(x => x.Sequence == sequence);

            customCommand.Actions.Insert(sequence, new CustomAction()
            {
                Device = device,
                Group = group,
                Function = function,
                Command = "press",
            });

            for (int i = 0; i < customCommand.Actions.Count; i++)
                customCommand.Actions[i].Sequence = i + 1;
            j64HarmonyGatewayRepository.Save(j64Config);

            return View("Index", CreateCustomCommandView());
        }
        #endregion

        #region Delete Action
        public IActionResult DeleteAction(string command, int sequence)
        {
            var customCommand = j64Config.CustomCommands.Find(x => x.CommandName == command);
            var customActionIndex = customCommand.Actions.FindIndex(x => x.Sequence == sequence);
            if (customActionIndex >= 0)
            {
                customCommand.Actions.RemoveAt(customActionIndex);
                for (int i = 0; i < customCommand.Actions.Count; i++)
                    customCommand.Actions[i].Sequence = i + 1;
                j64HarmonyGatewayRepository.Save(j64Config);
            }
            return View("Index", CreateCustomCommandView());
        }
        #endregion

        #region Move Action
        public IActionResult MoveActionUp(string command, int sequence)
        {
            var customCommand = j64Config.CustomCommands.Find(x => x.CommandName == command);
            var customActionIndex = customCommand.Actions.FindIndex(x => x.Sequence == sequence);
            if (customActionIndex > 0)
            {
                var customAction = customCommand.Actions[customActionIndex];
                customCommand.Actions.RemoveAt(customActionIndex);
                customCommand.Actions.Insert(customActionIndex - 1, customAction);
                for (int i = 0; i < customCommand.Actions.Count; i++)
                    customCommand.Actions[i].Sequence = i + 1;
                j64HarmonyGatewayRepository.Save(j64Config);
            }
            return View("Index", CreateCustomCommandView());
        }
        #endregion

        public IActionResult SyncSmartThings()
        {
            SmartThingsRepository.InstallDevices(j64Config, Request.Host.Value);
            return View("Index", CreateCustomCommandView());
        }
    }
}
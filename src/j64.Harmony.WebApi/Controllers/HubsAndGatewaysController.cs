using System;
using Microsoft.AspNet.Mvc;
using j64.Harmony.WebApi.ViewModels.Configure;
using j64.Harmony.Xmpp;
using j64.Harmony.WebApi.Models;
using j64.Harmony.WebApi.Repository;

namespace j64.Harmony.WebApi.Controllers
{
    public class HubsAndGatewaysController : Controller
    {
        private j64HarmonyGateway j64Config = null;
        private Hub myHub = null;

        public HubsAndGatewaysController(j64HarmonyGateway j64Config, Hub hub)
        {
            this.j64Config = j64Config;
            this.myHub = hub;
        }

        #region Hub Information
        public IActionResult Edit()
        {
            // If not connected to the smart hub go to the configure page
            if (myHub.hubConfig == null || String.IsNullOrEmpty(j64Config.ChannelDevice) || String.IsNullOrEmpty(j64Config.VolumeDevice))
                return RedirectToAction("Edit", "FirstTimeConfig");

            return View(HnGModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(HubsAndGatewaysViewModel hubInfoModel)
        {
            if (ModelState.IsValid)
            {
                j64Config.Email = hubInfoModel.HarmonyEmail;
                if (!String.IsNullOrEmpty(hubInfoModel.HarmonyPassword))
                    j64Config.Password = hubInfoModel.HarmonyPassword;
                j64Config.HubAddress = hubInfoModel.HarmonyHubAddress;
                j64Config.HubPort = hubInfoModel.HarmonyHubPort;

                j64Config.STHubAddress = hubInfoModel.StHubAddress;
                j64Config.STHubPort = hubInfoModel.StHubPort;

                j64Config.j64Address = hubInfoModel.j64Address;
                j64Config.j64Port = hubInfoModel.j64Port;

                // Refresh the connection
                try
                {
                    myHub.StartNewConnection(j64Config.Email, j64Config.Password, j64Config.HubAddress, j64Config.HubPort);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("HarmonyPassword", "Could not authenticate with harmony cloud service");
                    return View(hubInfoModel);
                }

                // Save the new data
                j64HarmonyGatewayRepository.Save(j64Config);
            }

            return View(hubInfoModel);
        }
        #endregion

        public IActionResult Findj64Address()
        {
            SmartThingsRepository.Determinej64Address(Request.Host.Value, j64Config);
            j64HarmonyGatewayRepository.Save(j64Config);
            return View("Edit", HnGModel());
        }

        private HubsAndGatewaysViewModel HnGModel()
        {
            return new HubsAndGatewaysViewModel()
            {
                HarmonyHubAddress = j64Config.HubAddress,
                HarmonyHubPort = j64Config.HubPort,
                HarmonyEmail = j64Config.Email,
                HarmonyPassword = j64Config.Password,
                StHubAddress = j64Config.STHubAddress,
                StHubPort = j64Config.STHubPort,
                j64Address = j64Config.j64Address,
                j64Port = j64Config.j64Port
            };
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using j64.Harmony.WebApi.Models;
using j64.Harmony.WebApi.Repository;
using j64.Harmony.WebApi.ViewModels.Configure;
using j64.Harmony.Xmpp;
using Microsoft.AspNet.Mvc.Rendering;

namespace j64.Harmony.WebApi.Controllers
{
    public class FirstTimeConfigController : Controller
    {
        private j64HarmonyGateway hubConfig = null;
        private Hub myHub = null;

        public FirstTimeConfigController(j64HarmonyGateway hubConfig, Hub hub)
        {
            this.hubConfig = hubConfig;
            this.myHub = hub;
        }

        #region Hub Information
        public IActionResult Edit()
        {
            return View(ftcModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FirstTimeConfigViewModel ftcViewModel)
        {
            if (ModelState.IsValid)
            {
                hubConfig.Email = ftcViewModel.HarmonyEmail;
                if (!String.IsNullOrEmpty(ftcViewModel.HarmonyPassword))
                    hubConfig.Password = ftcViewModel.HarmonyPassword;
                hubConfig.HubAddress = ftcViewModel.HarmonyHubAddress;
                hubConfig.HubPort = ftcViewModel.HarmonyHubPort;

                // Refresh the connection
                ftcViewModel.IsConnected = myHub.StartNewConnection(hubConfig.Email, hubConfig.Password, hubConfig.HubAddress, hubConfig.HubPort);
                if (ftcViewModel.IsConnected == false)
                {
                    ModelState.AddModelError("HarmonyPassword", "Could not authenticate with harmony cloud service");
                    return View(ftcModel());
                }

                hubConfig.VolumeDevice = ftcViewModel.VolumeDevice;
                hubConfig.ChannelDevice = ftcViewModel.ChannelDevice;

                // Save the new data
                j64HarmonyGatewayRepository.Save(hubConfig);
            }

            // Redirect to the home page once we are done with first time configuration
            if (ftcViewModel.IsConnected && !String.IsNullOrEmpty(ftcViewModel.VolumeDevice) && !String.IsNullOrEmpty(ftcViewModel.ChannelDevice))
                return RedirectToAction("Index", "Home");

            return View(ftcModel());
        }
        #endregion

        private FirstTimeConfigViewModel ftcModel()
        {
            var ftcm = new FirstTimeConfigViewModel()
            {
                HarmonyHubAddress = hubConfig.HubAddress,
                HarmonyHubPort = hubConfig.HubPort,
                HarmonyEmail = hubConfig.Email,
                HarmonyPassword = hubConfig.Password,
                VolumeDevice = hubConfig.VolumeDevice,
                ChannelDevice = hubConfig.ChannelDevice
            };
            if (myHub.hubConfig != null)
                ftcm.IsConnected = true;

            ftcm.VolumeDeviceList = GetDeviceList(ftcm.VolumeDevice);
            ftcm.ChannelDeviceList = GetDeviceList(ftcm.ChannelDevice);

            return ftcm;
        }

        private List<SelectListItem> GetDeviceList(string selectectValue)
        {
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new Microsoft.AspNet.Mvc.Rendering.SelectListItem() { Text = "" });

            myHub.hubConfig?.device.ForEach(x =>
            {
                var sli = new Microsoft.AspNet.Mvc.Rendering.SelectListItem()
                {
                    Text = x.label,
                    Selected = (x.label == selectectValue)
                };

                l.Add(sli);
            });

            return l;
        }
    }
}
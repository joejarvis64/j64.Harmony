using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using j64.Harmony.WebApi.ViewModels.Configure;
using j64.Harmony.Xmpp;
using Microsoft.AspNet.Mvc.Rendering;
using j64.Harmony.WebApi.Models;
using j64.Harmony.WebApi.Repository;

namespace j64.Harmony.WebApi.Controllers
{
    public class DeviceNamesController : Controller
    {
        private Hub myHub;
        private j64HarmonyGateway myj64Config;

        public DeviceNamesController(j64HarmonyGateway j64Config, Hub hub)
        {
            myj64Config = j64Config;
            myHub = hub;
        }

        #region Edit
        public IActionResult Edit()
        {
            // If not connected to the smart hub go to the configure page
            if (myHub.hubConfig == null || String.IsNullOrEmpty(myj64Config.ChannelDevice) || String.IsNullOrEmpty(myj64Config.VolumeDevice))
                return RedirectToAction("Edit", "FirstTimeConfig");

            if (String.IsNullOrEmpty(OauthRepository.Get().accessToken))
                return RedirectToAction("Index", "Oauth");

            return View(CreateViewMode());
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DeviceNamesViewModel deviceNames)
        {
            if (ModelState.IsValid)
            {
                myj64Config.SoundDeviceName = deviceNames.SoundDeviceName;
                myj64Config.ChannelSurfDeviceName = deviceNames.ChannelSurfDeviceName;
                myj64Config.LastChannelDeviceName = deviceNames.LastChannelDeviceName;
                myj64Config.VcrPauseDeviceName = deviceNames.VcrPauseDeviceName;

                myj64Config.VolumeDevice = deviceNames.VolumeDevice;
                myj64Config.ChannelDevice = deviceNames.ChannelDevice;
                myj64Config.ChanneKeyPauseInterval = deviceNames.ChanneKeyPauseInterval;

                j64HarmonyGatewayRepository.Save(myj64Config);
            }

            deviceNames.VolumeDeviceList = GetDeviceList(deviceNames.VolumeDevice);
            deviceNames.ChannelDeviceList = GetDeviceList(deviceNames.ChannelDevice);
            return View(deviceNames);
        }

        #endregion


        public IActionResult SyncSmartThings()
        {
            SmartThingsRepository.InstallDevices(myj64Config, Request.Host.Value);
            return View("Edit", CreateViewMode());
        }
        
        private DeviceNamesViewModel CreateViewMode()
        {
            var dvm = new DeviceNamesViewModel()
            {
                ChanneKeyPauseInterval = myj64Config.ChanneKeyPauseInterval,
                ChannelDevice = myj64Config.ChannelDevice,
                ChannelSurfDeviceName = myj64Config.ChannelSurfDeviceName,
                LastChannelDeviceName = myj64Config.LastChannelDeviceName,
                SoundDeviceName = myj64Config.SoundDeviceName,
                VcrPauseDeviceName = myj64Config.VcrPauseDeviceName,
                VolumeDevice = myj64Config.VolumeDevice,
            };

            dvm.VolumeDeviceList = GetDeviceList(dvm.VolumeDevice);
            dvm.ChannelDeviceList = GetDeviceList(dvm.ChannelDevice);
            
            return dvm;
        }
    }
}
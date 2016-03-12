using System;
using Microsoft.AspNet.Mvc;
using j64.Harmony.WebApi.ViewModels.Config;
using j64.Harmony.WebApi.ViewModels.Configure;
using j64.Harmony.Xmpp;

namespace j64.Harmony.WebApi.Controllers
{
    public class ConfigureController : Controller
    {
        private HarmonyHubConfiguration hubConfig = null;
        private Hub myHub = null;

        public ConfigureController(HarmonyHubConfiguration hubConfig, Hub hub)
        {
            this.hubConfig = hubConfig;
            this.myHub = hub;
        }

        public IActionResult HubInformation()
        {
            return View(GetHubInfo());
        }
        
        public IActionResult SaveChanges(HubInformationModel hi)
        {
            hubConfig.Email = hi.HarmonyEmail;
            if (!String.IsNullOrEmpty(hi.HarmonyPassword))
                hubConfig.Password = hi.HarmonyPassword;
            hubConfig.HubAddress = hi.HarmonyHubAddress;
            hubConfig.HubPort = hi.HarmonyHubPort;
            
            hubConfig.STHubAddress = hi.StHubAddress;
            hubConfig.STHubPort = hi.StHubPort;
            
            hubConfig.j64Address = hi.j64Address;
            hubConfig.j64Port = hi.j64Port;
            hubConfig.j64AppId = hi.j64AppId;

            // Refresh the connection
            myHub.StartNewConnection(hubConfig.Email, hubConfig.Password, hubConfig.HubAddress, hubConfig.HubPort);

            // We always have to update the device list on the Hub Configuration after we get the config info
            hubConfig.DeviceList.Clear();
            myHub.hubConfig?.device.ForEach(x => hubConfig.DeviceList.Add(new Microsoft.AspNet.Mvc.Rendering.SelectListItem() { Text = x.label }));

            // Set a default if necessary
            if (String.IsNullOrEmpty(hubConfig.ChannelDevice))
            {
                hubConfig.ChannelDevice = myHub.hubConfig.device?[0].label;
                hubConfig.VolumeDevice = myHub.hubConfig.device?[0].label;
            }

            // Save the new data
            HarmonyHubConfiguration.Save(hubConfig);

            return View("Index", hubConfig);
        }
        
        private HubInformationModel GetHubInfo()
        {
            return new HubInformationModel()
            {
                HarmonyHubAddress = hubConfig.HubAddress,
                HarmonyHubPort = hubConfig.HubPort,
                HarmonyEmail = hubConfig.Email,
                HarmonyPassword = hubConfig.Password,
                StHubAddress = hubConfig.STHubAddress,
                StHubPort = hubConfig.STHubPort,
                j64Address = hubConfig.j64Address,
                j64Port = hubConfig.j64Port,
                j64AppId = hubConfig.j64AppId
            };
        }
    }
}
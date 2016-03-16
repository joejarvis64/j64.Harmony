using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using j64.Harmony.WebApi.ViewModels.Configure;
using j64.Harmony.WebApi.Models;
using j64.Harmony.Xmpp;
using j64.Harmony.WebApi.Repository;

namespace j64.Harmony.WebApi.Controllers
{
    public class FavoriteChannelController : Controller
    {
        private j64HarmonyGateway j64Config = null;
        private Hub myHub = null;

        public FavoriteChannelController(j64HarmonyGateway j64Config, Hub hub)
        {
            this.j64Config = j64Config;
            myHub = hub;
        }

        public IActionResult Index()
        {
            // If not connected to the smart hub go to the configure page
            if (myHub.hubConfig == null || String.IsNullOrEmpty(j64Config.ChannelDevice) || String.IsNullOrEmpty(j64Config.VolumeDevice))
                return RedirectToAction("Edit", "FirstTimeConfig");

            if (String.IsNullOrEmpty(OauthRepository.Get().accessToken))
                return RedirectToAction("Index", "Oauth");

            List<FavoriteChannelViewModel> l = new List<FavoriteChannelViewModel>();
            j64Config.FavoriteChannels.ForEach(x => l.Add(new FavoriteChannelViewModel() { ChannelNumber = x.Channel, Name = x.Name }));

            return View(l);
        }

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FavoriteChannelViewModel favoriteChannel)
        {
            // Test for a pre-existing channel!
            var fcx = j64Config.FavoriteChannels.Find(x => x.Channel == favoriteChannel.ChannelNumber);
            if (fcx != null)
                ModelState.AddModelError("ChannelNumber", "This channel number already exists");

            fcx = j64Config.FavoriteChannels.Find(x => x.Name == favoriteChannel.Name);
            if (fcx != null)
                ModelState.AddModelError("Name", "This channel name already exists");

            if (ModelState.IsValid == false)
                return View(favoriteChannel);

            // Add this channel to the config
            j64Config.FavoriteChannels.Add(new FavoriteChannel()
            {
                Channel = favoriteChannel.ChannelNumber,
                Name = favoriteChannel.Name
            });
            j64HarmonyGatewayRepository.Save(j64Config);

            // Return to the list
            return RedirectToAction("Index");
        }
        #endregion

        #region Edit
        public IActionResult Edit(string channel)
        {
            if (channel == null)
                return HttpNotFound();

            var hubFc = j64Config.FavoriteChannels.Find(x => x.Channel == channel);
            if (hubFc == null)
                return HttpNotFound();

            // Return the view model entry
            FavoriteChannelViewModel fc = new FavoriteChannelViewModel()
            {
                ChannelNumber = hubFc.Channel,
                Name = hubFc.Name
            };

            return View(fc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FavoriteChannelViewModel favoriteChannel)
        {
            var hubFc = j64Config.FavoriteChannels.Find(x => x.Channel == favoriteChannel.ChannelNumber);
            if (hubFc == null)
                ModelState.AddModelError("ChannelNumber", "This channel no longer exists");

            var hubFcn = j64Config.FavoriteChannels.Find(x => x.Name == favoriteChannel.Name);
            if (hubFcn != null)
                ModelState.AddModelError("Name", "This channel name already exists");


            if (ModelState.IsValid == false)
                return View(favoriteChannel);

            hubFc.Name = favoriteChannel.Name;
            j64HarmonyGatewayRepository.Save(j64Config);

            return RedirectToAction("Index");
        }
        #endregion

        #region Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string channel)
        {
            var fcx = j64Config.FavoriteChannels.Find(x => x.Channel == channel);
            if (fcx != null)
                j64Config.FavoriteChannels.Remove(fcx);

            return RedirectToAction("Index");
        }
        #endregion

        public IActionResult SyncSmartThings()
        {
            SmartThingsRepository.InstallDevices(j64Config, Request.Host.Value);
            return View("Index", j64Config);
        }
    }
}
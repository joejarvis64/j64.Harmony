using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using j64.Harmony.WebApi.Models;
using j64.Harmony.WebApi.Services;
using j64.Harmony.WebApi.ViewModels.Config;
using j64.Harmony.Xmpp;

namespace j64.Harmony.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            HarmonyHubConfiguration.HarmonyHubConfigurationFile = env.MapPath("HarmonyHubConfiguration.json");
            OauthRepository.RepositoryFile = env.MapPath("SmartThings.json");
            
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Get the configuration info
            HarmonyHubConfiguration hubConfig = HarmonyHubConfiguration.Read();
            services.AddInstance<HarmonyHubConfiguration>(hubConfig);

            // Get an auth token from the harmony "cloud"
            Hub myHub = new Hub();
            if (hubConfig.Email != null && hubConfig.Password != null && hubConfig.HubAddress != null && hubConfig.HubPort != 0)
            {
                try
                {
                    myHub.StartNewConnection(hubConfig.Email, hubConfig.Password, hubConfig.HubAddress, hubConfig.HubPort);
                }
                catch (Exception)
                {
                    // Reset the devices since we could not get a connect
                    hubConfig.VolumeDevice = null;
                    hubConfig.ChannelDevice = null;
                }
            }

            // We always have to update the device list on the Hub Configuration after we get the config info
            hubConfig.DeviceList.Clear();
            myHub.hubConfig?.device.ForEach(x => hubConfig.DeviceList.Add(new Microsoft.AspNet.Mvc.Rendering.SelectListItem() { Text = x.label }));

            // Add the hub as a service available to all of the controllers
            services.AddInstance<Hub>(myHub);

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                    }
                }
                catch { }
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}

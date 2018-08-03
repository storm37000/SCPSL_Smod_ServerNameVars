﻿using Smod2;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;

namespace Smod.TestPlugin
{
    [PluginDetails(
        author = "storm37000",
        name = "ServerNameVars",
        description = "Readds server name statistics plus some new ones",
        id = "s37k.servernamevars",
        version = "1.0.0",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 0
        )]
    class Default : Plugin
    {
        public override void OnDisable()
        {
            this.Info(this.Details.name + " has been disabled.");
        }
        public override void OnEnable()
        {
            bool SSLerr = false;
            this.Info(this.Details.name + " has been enabled.");
            while (true)
            {
                try
                {
                    string host = "https://storm37000.tk/addons/";
                    if (SSLerr) { host = "http://74.91.115.126/addons/"; }
                    ushort version = ushort.Parse(this.Details.version.Replace(".", string.Empty));
                    ushort fileContentV = ushort.Parse(new System.Net.WebClient().DownloadString(host + this.Details.name + ".ver"));
                    if (fileContentV > version)
                    {
                        this.Info("Your version is out of date, please visit the Smod discord and download the newest version");
                    }
                    break;
                }
                catch (System.Exception e)
                {
                    if (SSLerr == false && e.Message.Contains("The authentication or decryption has failed."))
                    {
                        SSLerr = true;
                        continue;
                    }
                    this.Error("Could not fetch latest version txt: " + e.Message);
                    break;
                }
            }
        }

        public override void Register()
        {
            // Register Events
            EventHandler events = new EventHandler(this);
            this.AddEventHandler(typeof(IEventHandlerSetServerName), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerRoundStart), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPlayerDie), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerCheckEscape), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerSetRole), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerWarheadDetonate), events, Priority.Highest);
        }
    }
}

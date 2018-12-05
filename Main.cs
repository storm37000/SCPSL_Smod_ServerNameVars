﻿using Smod2;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;
using System;

namespace ServerNameVars
{
    [PluginDetails(
        author = "storm37000",
        name = "ServerNameVars",
        description = "Readds server name statistics plus some new ones",
        id = "s37k.servernamevars",
        version = "1.0.2",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 0
        )]
    class Main : Plugin
    {
        public override void OnDisable()
        {
            this.Info(this.Details.name + " has been disabled.");
        }
		public override void OnEnable()
		{
			this.Info(this.Details.name + " has been enabled.");
			string[] hosts = { "https://storm37k.com/addons/", "http://74.91.115.126/addons/" };
			ushort version = ushort.Parse(this.Details.version.Replace(".", string.Empty));
			bool fail = true;
			string errorMSG = "";
			foreach (string host in hosts)
			{
				using (UnityEngine.WWW req = new UnityEngine.WWW(host + this.Details.name + ".ver"))
				{
					while (!req.isDone) { }
					errorMSG = req.error;
					if (string.IsNullOrEmpty(req.error))
					{
						ushort fileContentV = 0;
						if (!ushort.TryParse(req.text, out fileContentV))
						{
							errorMSG = "Parse Failure";
							continue;
						}
						if (fileContentV > version)
						{
							this.Error("Your version is out of date, please visit the Smod discord and download the newest version");
						}
						fail = false;
						break;
					}
				}
			}
			if (fail)
			{
				this.Error("Could not fetch latest version txt: " + errorMSG);
			}
		}

		private EventHandler events;

        public void addCustomVar(string varname, Func<string> callback, Plugin source)
        {
            events.addCustomVar(varname, callback, source);
        }

        public string testy()
        {
            return "Hello world";
        }

        public override void Register()
        {
            events = new EventHandler(this);
            // Register Events
            this.AddEventHandler(typeof(IEventHandlerSetServerName), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerRoundStart), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerRoundEnd), events, Priority.High);
            this.AddEventHandler(typeof(IEventHandlerPlayerDie), events, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerCheckEscape), events, Priority.Highest);
            if (PluginManager.Manager.FindEnabledPlugins("LaterJoin").Count > 0)
            {
                this.Info("Laterjoin integration enabled.");
                this.AddEventHandler(typeof(IEventHandlerSetRole), events, Priority.Highest);
            }
            this.AddEventHandler(typeof(IEventHandlerWarheadDetonate), events, Priority.Highest);

            Func<string> callback = testy;
            this.addCustomVar("test", callback, this);
        }
    }
}
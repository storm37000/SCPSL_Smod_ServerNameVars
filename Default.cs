using Smod2;
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
        version = "1.0.1",
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
			string hostfile = "";
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Unix:
					hostfile = "http://pastebin.com/raw/9VQi53JQ";
					break;

				case PlatformID.MacOSX:
					hostfile = "http://pastebin.com/raw/9VQi53JQ";
					break;

				default:
					hostfile = "https://pastebin.com/raw/9VQi53JQ";
					break;
			}
			string[] hosts = new System.Net.WebClient().DownloadString(hostfile).Split('\n');
			while (true)
			{
				try
				{
					string host = hosts[0];
					if (SSLerr) { host = hosts[1]; }
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
					if (SSLerr == false)
					{
						SSLerr = true;
						continue;
					}
					this.Error("Could not fetch latest version txt: " + e.Message);
					break;
				}
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

using Smod2;
using Smod2.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using MEC;
using System.Linq;

namespace ServerNameVars
{
	[PluginDetails(
		author = "storm37000",
		name = "ServerNameVars",
		description = "Adds new server name variables.",
		id = "s37k.servernamevars",
		version = "1.1.0",
		SmodMajor = 3,
		SmodMinor = 2,
		SmodRevision = 2
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
		}

		public bool UpToDate { get; private set; } = true;

		public void outdatedmsg()
		{
			this.Error("Your version is out of date, please update the plugin and restart your server when it is convenient for you.");
		}

		IEnumerator<float> UpdateChecker()
		{
			string[] hosts = { "https://storm37k.com/addons/", "http://74.91.115.126/addons/" };
			bool fail = true;
			string errorMSG = "";
			foreach (string host in hosts)
			{
				using (UnityWebRequest webRequest = UnityWebRequest.Get(host + this.Details.name + ".ver"))
				{
					// Request and wait for the desired page.
					yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

					if (webRequest.isNetworkError || webRequest.isHttpError)
					{
						errorMSG = webRequest.error;
					}
					else
					{
						if (webRequest.downloadHandler.text != this.Details.version)
						{
							outdatedmsg();
							UpToDate = false;
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

		public EventHandler events;

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
			this.AddEventHandlers(events);

			Func<string> callback = testy;
			this.addCustomVar("test", callback, this);

			try
			{
				string file = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Smod2.ConfigManager.Manager.Config.GetConfigPath()), "s37k_g_disableVcheck*", System.IO.SearchOption.TopDirectoryOnly).FirstOrDefault();
				if (file == null)
				{
					Timing.RunCoroutine(UpdateChecker());
				}
				else
				{
					this.Info("Version checker is disabled.");
				}
			}
			catch (System.Exception)
			{
				Timing.RunCoroutine(UpdateChecker());
			}
		}
	}
}

using Smod2;
using Smod2.Events;
using Smod2.EventHandlers;
using System;
using Smod2.EventSystem.Events;

namespace ServerNameVars
{
	class EventHandler : IEventHandlerSetServerName, IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerPlayerDie, IEventHandlerSetRole, IEventHandlerWarheadDetonate, IEventHandlerSCP914Activate, IEventHandlerDecideTeamRespawnQueue
	{
		private Main plugin;
		private System.Collections.Generic.List<string> blklst = new System.Collections.Generic.List<string>();
		private System.Collections.Generic.Dictionary<string, Func<string>> cmdtable = new System.Collections.Generic.Dictionary<string, Func<string>>();
		private ushort altroundnumber = 0;

		private WebsocketServer wssvr;

		public ushort RoundNumber { get; private set; } = 0;
		public ushort SCPKills { get; private set; } = 0;
		public ushort ClassDStart { get; private set; } = 0;
		public ushort ScientistStart { get; private set; } = 0;
		public ushort SCPStart { get; private set; } = 0;
		public bool WarheadDetonated { get; private set; } = false;
		public ushort scp914activates { get; private set; } = 0;
		public string ServerName { get; private set; }

		public EventHandler(Main plugin)
		{
			this.plugin = plugin;
			if (plugin.GetConfigBool("srvnamevars_WebSocketServer_Enabled"))
			{
				ushort port;
				if (!ushort.TryParse(plugin.GetConfigInt("srvnamevars_WebSocketServer_Port").ToString(), out port))
				{
					plugin.Error("Invalid port number specified in config file.");
					return;
				}
				if (port <= 2000)
				{
					plugin.Error("Invalid port number specified in config file.");
					return;
				}
				plugin.Info("attempting to start websocket server...");
				try
				{
					wssvr = new WebsocketServer(this, port, plugin.GetConfigString("srvnamevars_WebSocketServer_ServiceName"));
				}
				catch
				{
					plugin.Error("Failed to start websocket server! (are you missing the dependency dll?)");
				}
			}
		}

		public void addCustomVar(string varname, Func<string> callback, Plugin source)
		{
			cmdtable[varname] = callback;
			plugin.Info("Added custom var: $[" + varname + "]" + " From plugin: " + source.Details.id);
		}

		private void reset()
		{
			blklst.Clear();
			SCPKills = 0;
			ClassDStart = 0;
			ScientistStart = 0;
			SCPStart = 0;
			WarheadDetonated = false;
			scp914activates = 0;
		}

//        private string Counter(SetServerNameEvent ev)
//        {
//            if(ev.Server.NumPlayers/ev.Server.MaxPlayers == 1)
//            {
//                return "FULL";
//            }
//            else
//            {
//                return ev.Server.NumPlayers + "/" + ev.Server.MaxPlayers;
//            }
//        }

		private string Counter(ushort current, ushort max)
		{
			return current + "/" + max;
		}

		public void OnSetServerName(SetServerNameEvent ev)
		{
			string cfgname = ConfigManager.Manager.Config.GetStringValue("sm_server_name", ev.ServerName);

			//cfgname = cfgname.Replace("$player_count", "" + ev.Server.NumPlayers);
			//cfgname = cfgname.Replace("$max_players", "" + ev.Server.MaxPlayers);
			//cfgname = cfgname.Replace("$full_player_count", Counter(ev));
			//cfgname = cfgname.Replace("$port", "" + ev.Server.Port);
			//cfgname = cfgname.Replace("$ip", ev.Server.IpAddress);
			//cfgname = cfgname.Replace("$number", "" + (ev.Server.Port - ConfigFile.GetIntList("port_queue")[0] + 1));
			//cfgname = cfgname.Replace("$lobby_id", "-");
			//cfgname = cfgname.Replace("$version", "-");
			//cfgname = cfgname.Replace("$sm_version", smodtrackstr);
			cfgname = cfgname.Replace("$classd_alive", "" + ev.Server.Round.Stats.ClassDAlive);
			cfgname = cfgname.Replace("$classd_escape", "" + ev.Server.Round.Stats.ClassDEscaped);
			cfgname = cfgname.Replace("$classd_start", "" + ClassDStart);
			cfgname = cfgname.Replace("$classd_dead", "" + ev.Server.Round.Stats.ClassDDead);
			cfgname = cfgname.Replace("$classd_counter", "" + Counter((ushort)ev.Server.Round.Stats.ClassDEscaped, ClassDStart));
			cfgname = cfgname.Replace("$scientists_alive", "" + ev.Server.Round.Stats.ScientistsAlive);
			cfgname = cfgname.Replace("$scientists_escape", "" + ev.Server.Round.Stats.ScientistsEscaped);
			cfgname = cfgname.Replace("$scientists_start", "" + ScientistStart);
			cfgname = cfgname.Replace("$scientists_dead", "" + ev.Server.Round.Stats.ScientistsDead);
			cfgname = cfgname.Replace("$scientists_counter", "" + Counter((ushort)ev.Server.Round.Stats.ScientistsEscaped, ScientistStart));
			cfgname = cfgname.Replace("$scp_alive", "" + ev.Server.Round.Stats.SCPAlive);
			cfgname = cfgname.Replace("$scp_start", "" + SCPStart);
			cfgname = cfgname.Replace("$scp_dead", "" + ev.Server.Round.Stats.SCPDead);
			cfgname = cfgname.Replace("$scp_zombies", "" + ev.Server.Round.Stats.Zombies);
			cfgname = cfgname.Replace("$scp_kills", "" + SCPKills);
			cfgname = cfgname.Replace("$scp_counter", "" + Counter((ushort)ev.Server.Round.Stats.SCPAlive, SCPStart));
			cfgname = cfgname.Replace("$ntf_alive", "" + ev.Server.Round.Stats.NTFAlive);
			cfgname = cfgname.Replace("$ci_alive", "" + ev.Server.Round.Stats.CiAlive);
			cfgname = cfgname.Replace("$warhead_detonated", WarheadDetonated ? "☢ WARHEAD DETONATED ☢" : "" );
			cfgname = cfgname.Replace("$round_duration", "" + ev.Server.Round.Duration/60);
			cfgname = cfgname.Replace("$round_number", "" + Math.Max(RoundNumber, altroundnumber));
			cfgname = cfgname.Replace("$914uses", "" + scp914activates);

			foreach (System.Collections.Generic.KeyValuePair<string, Func<string>> entry in cmdtable)
			{
				cfgname = cfgname.Replace("$[" + entry.Key + "]", "" + entry.Value());
			}

			ev.ServerName = cfgname;

			ServerName = ev.ServerName + "<br>" + "Players: " + (ev.Server.NumPlayers - 1) + "/" + ev.Server.MaxPlayers;

			try
			{
				wssvr.sendmsg(ServerName);
			}
			catch
			{
			}
		}

		public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
		{
			if (altroundnumber > RoundNumber) { plugin.Error("OnRoundStart event is not working! Using alternate event for $round_number"); }
			altroundnumber++;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (!plugin.UpToDate)
			{
				plugin.outdatedmsg();
			}
			RoundNumber++;
		}

		public void OnRoundEnd(RoundEndEvent ev)
		{
			if (ev.Round.Duration >= 3)
			{
				reset();
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (ev.Killer.TeamRole.Team == Smod2.API.Team.SCP)
			{
				SCPKills++;
			}
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if ((ev.TeamRole.Role != Smod2.API.Role.SPECTATOR || ev.Player.TeamRole.Team != Smod2.API.Team.NONE) && (!blklst.Contains(ev.Player.SteamId)))
			{
				if (ev.TeamRole.Team == Smod2.API.Team.CLASSD)
				{
					ClassDStart++;
				}
				if (ev.TeamRole.Team == Smod2.API.Team.SCIENTIST)
				{
					ScientistStart++;
				}
				if (ev.TeamRole.Team == Smod2.API.Team.SCP && ev.TeamRole.Role != Smod2.API.Role.SCP_049_2)
				{
					SCPStart++;
				}
				blklst.Add(ev.Player.SteamId);
			}
		}

		public void OnDetonate()
		{
			WarheadDetonated = true;
		}

		public void OnSCP914Activate(SCP914ActivateEvent ev)
		{
			scp914activates++;
		}
	}
}
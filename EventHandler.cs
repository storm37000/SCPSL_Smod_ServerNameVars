using Smod2;
using Smod2.Events;
using Smod2.EventHandlers;

namespace Smod.TestPlugin
{
    class EventHandler : IEventHandlerSetServerName, IEventHandlerRoundStart
    {
        private Plugin plugin;
        public uint RoundNumber { get; private set; } = 0;

        public EventHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

        private string fullPlayerCount(SetServerNameEvent ev)
        {
            if(ev.Server.NumPlayers/ev.Server.MaxPlayers == 1)
            {
                return "FULL";
            }
            else
            {
                return ev.Server.NumPlayers + "/" + ev.Server.MaxPlayers;
            }
        }

        private string warheadDetonated(SetServerNameEvent ev)
        {
            if (ev.Server.Round.Stats.WarheadDetonated)
            {
                return "☢ WARHEAD DETONATED ☢";
            }
            else
            {
                return "";
            }
        }

        public void OnSetServerName(SetServerNameEvent ev)
        {
            string cfgname = ConfigManager.Manager.Config.GetStringValue("sm_server_name", ev.ServerName);

            cfgname = cfgname.Replace("$player_count", "" + ev.Server.NumPlayers);
            cfgname = cfgname.Replace("$max_players", "" + ev.Server.MaxPlayers);
            cfgname = cfgname.Replace("$full_player_count", fullPlayerCount(ev));
            cfgname = cfgname.Replace("$port", "" + ev.Server.Port);
            cfgname = cfgname.Replace("$ip", ev.Server.IpAddress);
            //cfgname = cfgname.Replace("$number", "" + (ev.Server.Port - ConfigFile.GetIntList("port_queue")[0] + 1));
            //cfgname = cfgname.Replace("$lobby_id", "-");
            //cfgname = cfgname.Replace("$version", "-");
            //cfgname = cfgname.Replace("$sm_version", smodtrackstr);
            cfgname = cfgname.Replace("$classd_alive", "" + ev.Server.Round.Stats.ClassDAlive);
            cfgname = cfgname.Replace("$classd_escape", "" + ev.Server.Round.Stats.ClassDEscaped);
            cfgname = cfgname.Replace("$classd_start", "" + ev.Server.Round.Stats.ClassDStart);
            cfgname = cfgname.Replace("$classd_dead", "" + ev.Server.Round.Stats.ClassDDead);
            //cfgname = cfgname.Replace("$classd_counter", "" + ev.Server.NumPlayers);
            cfgname = cfgname.Replace("$scientists_alive", "" + ev.Server.Round.Stats.ScientistsAlive);
            cfgname = cfgname.Replace("$scientists_escape", "" + ev.Server.Round.Stats.ScientistsEscaped);
            cfgname = cfgname.Replace("$scientists_start", "" + ev.Server.Round.Stats.ScientistsStart);
            cfgname = cfgname.Replace("$scientists_dead", "" + ev.Server.Round.Stats.ScientistsDead);
            //cfgname = cfgname.Replace("$scientists_counter", "" + ev.Server.NumPlayers);
            cfgname = cfgname.Replace("$scp_alive_z", "" + ev.Server.Round.Stats.SCPAlive);
            cfgname = cfgname.Replace("$scp_alive_noz", "" + (ev.Server.Round.Stats.SCPAlive - ev.Server.Round.Stats.Zombies));
            cfgname = cfgname.Replace("$scp_start", "" + ev.Server.Round.Stats.SCPStart);
            cfgname = cfgname.Replace("$scp_dead", "" + ev.Server.Round.Stats.SCPDead);
            cfgname = cfgname.Replace("$scp_zombies", "" + ev.Server.Round.Stats.Zombies);
            cfgname = cfgname.Replace("$scp_kills", "" + ev.Server.Round.Stats.SCPKills);
            //cfgname = cfgname.Replace("$scp_counter", "" + ev.Server.Round.Stats);
            cfgname = cfgname.Replace("$warhead_detonated", warheadDetonated(ev));
            cfgname = cfgname.Replace("$round_duration", "" + ev.Server.Round.Duration/60);
            cfgname = cfgname.Replace("$round_number", "" + RoundNumber);

            ev.ServerName = cfgname;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            RoundNumber++;
        }
    }
}
using Smod2;
using Smod2.Events;
using Smod2.EventHandlers;

namespace Smod.TestPlugin
{
    class EventHandler : IEventHandlerSetServerName, IEventHandlerRoundStart, IEventHandlerPlayerDie, IEventHandlerCheckEscape, IEventHandlerSetRole, IEventHandlerWarheadDetonate
    {
        private Plugin plugin;
        public uint RoundNumber { get; private set; } = 0;
        public uint SCPKills { get; private set; } = 0;
        public uint ClassDEscapes { get; private set; } = 0;
        public uint ScientistEscapes { get; private set; } = 0;
        public uint ClassDStart { get; private set; } = 0;
        public uint ScientistStart { get; private set; } = 0;
        public uint SCPStart { get; private set; } = 0;
        public bool WarheadDetonated { get; private set; } = false;

        public EventHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

        private void cleanup()
        {
            SCPKills = 0;
            ClassDEscapes = 0;
            ScientistEscapes = 0;
            ClassDStart = 0;
            ScientistStart = 0;
            SCPStart = 0;
            WarheadDetonated = false;
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
            if (WarheadDetonated)
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
            cfgname = cfgname.Replace("$classd_escape", "" + ClassDEscapes);
            cfgname = cfgname.Replace("$classd_start", "" + ClassDStart);
            cfgname = cfgname.Replace("$classd_dead", "" + ev.Server.Round.Stats.ClassDDead);
            //cfgname = cfgname.Replace("$classd_counter", "" + ev.Server.NumPlayers);
            cfgname = cfgname.Replace("$scientists_alive", "" + ev.Server.Round.Stats.ScientistsAlive);
            cfgname = cfgname.Replace("$scientists_escape", "" + ScientistEscapes);
            cfgname = cfgname.Replace("$scientists_start", "" + ScientistStart);
            cfgname = cfgname.Replace("$scientists_dead", "" + ev.Server.Round.Stats.ScientistsDead);
            //cfgname = cfgname.Replace("$scientists_counter", "" + ev.Server.NumPlayers);
            cfgname = cfgname.Replace("$scp_alive", "" + ev.Server.Round.Stats.SCPAlive);
            cfgname = cfgname.Replace("$scp_start", "" + SCPStart);
            cfgname = cfgname.Replace("$scp_dead", "" + ev.Server.Round.Stats.SCPDead);
            cfgname = cfgname.Replace("$scp_zombies", "" + ev.Server.Round.Stats.Zombies);
            cfgname = cfgname.Replace("$scp_kills", "" + SCPKills);
            //cfgname = cfgname.Replace("$scp_counter", "" + ev.Server.Round.Stats);
            cfgname = cfgname.Replace("$ntf_alive", "" + ev.Server.Round.Stats.NTFAlive);
            cfgname = cfgname.Replace("$warhead_detonated", warheadDetonated(ev));
            cfgname = cfgname.Replace("$round_duration", "" + ev.Server.Round.Duration/60);
            cfgname = cfgname.Replace("$round_number", "" + RoundNumber);

            ev.ServerName = cfgname;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            cleanup();
            RoundNumber++;
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            if (ev.Killer.TeamRole.Team == Smod2.API.Team.SCP)
            {
                SCPKills++;
            }
        }

        public void OnCheckEscape(PlayerCheckEscapeEvent ev)
        {
            if (ev.Player.TeamRole.Role == Smod2.API.Role.CLASSD)
            {
                ClassDEscapes++;
            }
            if (ev.Player.TeamRole.Role == Smod2.API.Role.SCIENTIST)
            {
                ScientistEscapes++;
            }
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            plugin.Info("player " + ev.Player.Name + "set as " + (Smod2.API.Team)ev.TeamRole.Team);
            if (ev.TeamRole.Team == Smod2.API.Team.CLASSD)
            {
                ClassDStart++;
            }
            if (ev.TeamRole.Team == Smod2.API.Team.SCIENTISTS)
            {
                ScientistStart++;
            }
            if (ev.TeamRole.Team == Smod2.API.Team.SCP && ev.Role != Smod2.API.Role.SCP_049_2)
            {
                SCPStart++;
            }
        }

        public void OnDetonate()
        {
            WarheadDetonated = true;
        }
    }
}
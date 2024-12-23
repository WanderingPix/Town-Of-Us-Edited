﻿using System;
using System.Linq;
using UnityEngine;
using TownOfUs.Extensions;

namespace TownOfUs.Roles
{
    public class Maul : Role
    {
        private KillButton _rampageButton;
        public bool Enabled;
        public bool WerewolfWins;
        public PlayerControl ClosestPlayer;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public float RampageCooldown;
        public bool RampagecoolingDown => RampageCooldown > 0f;
        public float TimeRemaining;


        public Maul(PlayerControl player) : base(player)
        {
            Name = "Maul";
            ImpostorText = () => "Rampage To Kill Everyone";
            TaskText = () => "Rampage to kill everyone\nFake Tasks:";
            Color = Patches.Colors.Werewolf;
            RoleType = RoleEnum.Maul;
            RampageCooldown = CustomGameOptions.RampageCd;
            Cooldown = CustomGameOptions.RampageKillCd;
            AddToRoleHistory(RoleType);
            Faction = Faction.NeutralKilling;
        }

        public KillButton RampageButton
        {
            get => _rampageButton;
            set
            {
                _rampageButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        internal override void NeutralWin(LogicGameFlowNormal __instance)
        {
            if (Player.Data.IsDead || Player.Data.Disconnected) return;

            if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected) <= 2 &&
                    PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected &&
                    (x.Data.IsImpostor() || x.Is(Faction.NeutralKilling) || x.Is(Faction.Coven) || x.IsCrewKiller())) == 1)
            {
                Utils.Rpc(CustomRPC.WerewolfWin, Player.PlayerId);
                Wins();
                Utils.EndGame();
                System.Console.WriteLine("GAME OVER REASON: Maul Win");
                return;
            }

            return;
        }

        public void Wins()
        {
            WerewolfWins = true;
        }

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var werewolfTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            werewolfTeam.Add(PlayerControl.LocalPlayer);
            __instance.teamToShow = werewolfTeam;
        }
        public bool Rampaged => TimeRemaining > 0f;

        public float RampageTimer()
        {
            if (!RampagecoolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                RampageCooldown -= Time.deltaTime;
                return RampageCooldown;
            }
            else return RampageCooldown;
        }

        public void Rampage()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            if (Player.Data.IsDead)
            {
                TimeRemaining = 0f;
            }
        }

        public void Unrampage()
        {
            Enabled = false;
            RampageCooldown = CustomGameOptions.RampageCd;
        }

        public float KillTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }
    }
}

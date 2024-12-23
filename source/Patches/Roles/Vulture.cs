using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles
{
    public class Vulture : Role
    {
        public Vulture(PlayerControl player) : base(player)
        {
            Name = "Vulture";
            ImpostorText = () => "Eat All Bodies";
            TaskText = () => $"Eat {CustomGameOptions.VultureBodies} Dead Bodies to win!";
            Color = Patches.Colors.Vulture;
            Cooldown = CustomGameOptions.VultureCD;
            RoleType = RoleEnum.Vulture;
            AddToRoleHistory(RoleType);
            Faction = Faction.NeutralEvil;
        }

        public Dictionary<byte, ArrowBehaviour> BodyArrows = new Dictionary<byte, ArrowBehaviour>();
        public DeadBody CurrentTarget;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public int BodiesEaten = 0;
        public bool VultureWins = false;

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var vultureTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            vultureTeam.Add(PlayerControl.LocalPlayer);
            __instance.teamToShow = vultureTeam;
        }

        public float EatTimer()
        {
            if (!coolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                Cooldown -= Time.deltaTime;
                return Cooldown;
            }
            else return Cooldown;
        }

        public void Wins()
        {
            VultureWins = true;
            Utils.Rpc(CustomRPC.VultureWin, Player.PlayerId);
        }

        public void DestroyArrow(byte targetPlayerId)
        {
            var arrow = BodyArrows.FirstOrDefault(x => x.Key == targetPlayerId);
            if (arrow.Value != null)
                Object.Destroy(arrow.Value);
            if (arrow.Value.gameObject != null)
                Object.Destroy(arrow.Value.gameObject);
            BodyArrows.Remove(arrow.Key);
        }
    }
}
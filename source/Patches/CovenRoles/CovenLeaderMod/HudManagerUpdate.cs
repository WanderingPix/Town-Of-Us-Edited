using System.Linq;
using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CovenRoles.CovenLeaderMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.CovenLeader)) return;

            var role = Role.GetRole<CovenLeader>(PlayerControl.LocalPlayer);
            var killButton = __instance.KillButton;

            if (role.SabotageButton == null)
            {
                role.SabotageButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.SabotageButton.graphic.enabled = true;
                role.SabotageButton.gameObject.SetActive(false);
            }

            if (role.RecruitButton == null)
            {
                role.RecruitButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.RecruitButton.graphic.enabled = true;
                role.RecruitButton.gameObject.SetActive(false);
            }

            killButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            role.SabotageButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            role.RecruitButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started
                    && !role.Converted);
                    
            role.SabotageButton.graphic.sprite = TownOfUs.SabotageCoven;
            role.RecruitButton.graphic.sprite = TownOfUs.Recruit;
            role.RecruitButton.transform.localPosition = new Vector3(-2f, 1f, 0f);
            if (!PlayerControl.LocalPlayer.Data.IsDead)
            {
                role.SabotageButton.transform.localPosition = new Vector3(-1f, 1f, 0f);
            }
            else
            {
                var position = __instance.KillButton.transform.localPosition;
                role.SabotageButton.transform.localPosition = new Vector3(position.x,
                position.y, position.z);
            }

            // Set KillButton's cooldown
            killButton.SetCoolDown(role.KillTimer(), CustomGameOptions.CovenKCD);
            role.RecruitButton.SetCoolDown(role.Cooldown, CustomGameOptions.CovenKCD);
            role.SabotageButton.SetCoolDown(0f, 1f);

            // Set the closest player for the Kill Button's targeting
            var notcoven = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => !x.Is(Faction.Coven) && !x.Data.IsDead)
                .ToList();

            Utils.SetTarget(ref role.ClosestPlayer, role.RecruitButton, float.NaN, notcoven);

            if ((CamouflageUnCamouflage.IsCamoed && CustomGameOptions.CamoCommsKillAnyone) || PlayerControl.LocalPlayer.IsHypnotised()) Utils.SetTarget(ref role.ClosestPlayer, __instance.KillButton);
            else if (PlayerControl.LocalPlayer.IsLover() && CustomGameOptions.ImpLoverKillTeammate) Utils.SetTarget(ref role.ClosestPlayer, __instance.KillButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover()).ToList());
            else if (PlayerControl.LocalPlayer.IsLover()) Utils.SetTarget(ref role.ClosestPlayer, __instance.KillButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Coven)).ToList());
            else Utils.SetTarget(ref role.ClosestPlayer, __instance.KillButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Is(Faction.Coven)).ToList());
            
            role.SabotageButton.graphic.color = Palette.EnabledColor;
            role.SabotageButton.graphic.material.SetFloat("_Desat", 0f);
        }
    }
}
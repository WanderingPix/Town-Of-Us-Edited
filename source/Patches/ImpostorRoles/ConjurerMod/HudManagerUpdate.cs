using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.ImpostorRoles.ConjurerMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Conjurer)) return;
            var role = Role.GetRole<Conjurer>(PlayerControl.LocalPlayer);

            var Kill = role.Kill;

            if (role.CurseButton == null)
            {
                role.CurseButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.CurseButton.graphic.enabled = true;
                role.CurseButton.graphic.sprite = TownOfUs.Curse;
                role.CurseButton.gameObject.SetActive(false);
            }

            if (role.CurseButton.graphic.sprite != TownOfUs.Curse &&
                role.CurseButton.graphic.sprite != Kill)
                role.CurseButton.graphic.sprite = TownOfUs.Curse;

            if (role.CurseButton.graphic.sprite == Kill && (role.CursedPlayer == null || role.CursedPlayer.Data.IsDead))
                role.CurseButton.graphic.sprite = TownOfUs.Curse;

            if (role.CursedPlayer != null && !role.CursedPlayer.Data.IsDead)
            {
                role.CurseButton.graphic.sprite = Kill;
            }

            if (role.LabelText == null)
            {
                role.LabelText = Object.Instantiate(__instance.KillButton.buttonLabelText, __instance.KillButton.buttonLabelText.transform.parent);
                role.LabelText.SetOutlineColor(Palette.Purple);
                role.LabelText.transform.localPosition = new Vector3(-2.84f, -0.55f, 0f);
                role.LabelText.gameObject.SetActive(false);
            }

            role.LabelText.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started
                    && role.CurseButton.graphic.sprite == Kill);
            role.CurseButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            role.CurseButton.SetCoolDown(role.KillCooldown, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);

            role.CurseButton.transform.localPosition = new Vector3(-2f, 1f, 0f);

            var notimps = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => !x.Is(Faction.Impostors))
                .ToList();

            if (role.CurseButton.graphic.sprite == TownOfUs.Curse)
            {
                var killButton = role.CurseButton;
                if ((CamouflageUnCamouflage.IsCamoed && CustomGameOptions.CamoCommsKillAnyone) || PlayerControl.LocalPlayer.IsHypnotised()) Utils.SetTarget(ref role.ClosestPlayer, killButton);
                else if (PlayerControl.LocalPlayer.IsLover() && CustomGameOptions.ImpLoverKillTeammate) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover()).ToList());
                else if (PlayerControl.LocalPlayer.IsLover() && !CustomGameOptions.MadmateKillEachOther) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Impostors) && !x.Is(Faction.Madmates)).ToList());
                else if (PlayerControl.LocalPlayer.IsLover()) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Impostors)).ToList());
                else if (!CustomGameOptions.MadmateKillEachOther) Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.IsLover() && !x.Is(Faction.Impostors) && !x.Is(Faction.Madmates)).ToList());
                else Utils.SetTarget(ref role.ClosestPlayer, killButton, float.NaN, PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Is(Faction.Impostors)).ToList());
            }
            else
            {
                if (!role.CursedPlayer.Data.IsDead && !MeetingHud.Instance)
                {
                    var notdead = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead).ToList();
                    var maxDistance = GameOptionsData.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
                    var playerToDie = Utils.GetClosestPlayer(role.CursedPlayer, notdead);
                    if (playerToDie != null && !playerToDie.Is(Faction.Impostors) && Vector2.Distance(playerToDie.GetTruePosition(),
                    role.CursedPlayer.GetTruePosition()) < maxDistance)
                    {
                        var renderer = role.CurseButton.graphic;
                        renderer.color = Palette.EnabledColor;
                        renderer.material.SetFloat("_Desat", 0f);
                        var labelrender = role.LabelText;
                        labelrender.color = Palette.EnabledColor;
                        labelrender.material.SetFloat("_Desat", 0f);
                    }
                    else
                    {
                        var renderer = role.CurseButton.graphic;
                        renderer.color = Palette.DisabledClear;
                        renderer.material.SetFloat("_Desat", 1f);
                        var labelrender = role.LabelText;
                        labelrender.color = Palette.DisabledClear;
                        labelrender.material.SetFloat("_Desat", 1f);
                    }
                }
            }
        }
    }
}
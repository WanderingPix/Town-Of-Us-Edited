using AmongUs.GameOptions;
using HarmonyLib;
using System.Linq;
using TownOfUs.CrewmateRoles.MedicMod;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.AvengerMod
{
    [HarmonyPatch(typeof(HudManager))]
    public class HudManagerUpdate
    {
        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix(HudManager __instance)
        {
            UpdateKillButtons(__instance);
        }

        public static void UpdateKillButtons(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Avenger)) return;

            var role = Role.GetRole<Avenger>(PlayerControl.LocalPlayer);

            if (role.killer != null)
            {
                var data2 = role.killer.Data;
                if (data2 == null || data2.Disconnected || data2.IsDead || PlayerControl.LocalPlayer.Data.IsDead) return;
                role.killer.nameText().color = Color.black;
            }

            if (role.AvengeButton == null)
            {
                role.AvengeButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.AvengeButton.graphic.enabled = true;
                role.AvengeButton.gameObject.SetActive(false);
            }

            role.AvengeButton.graphic.sprite = TownOfUs.Avenge;
            role.AvengeButton.transform.localPosition = new Vector3(-2f, 0f, 0f);

            role.AvengeButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started
                    && !role.Avenging);

            var killButton = __instance.KillButton;

            __instance.KillButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            __instance.KillButton.buttonLabelText.text = "Avenge";

            var renderer = killButton.graphic;
            var buttontextrender = killButton.buttonLabelText;
            if (role.killer != null && !role.killer.Data.IsDead)
            {
                var killer = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId == role.killer.PlayerId).ToList();
                Utils.SetTarget(ref role.ClosestPlayer, __instance.KillButton, float.NaN, killer);
                if (role.ClosestPlayer != null && role.ClosestPlayer == role.killer)
                {
                    buttontextrender.color = Palette.EnabledColor;
                    buttontextrender.material.SetFloat("_Desat", 0f);
                    renderer.color = Palette.EnabledColor;
                    renderer.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    buttontextrender.color = Palette.DisabledClear;
                    buttontextrender.material.SetFloat("_Desat", 1f);
                    renderer.color = Palette.DisabledClear;
                    renderer.material.SetFloat("_Desat", 1f);
                }
            }

            if (role.killer == null)
            {
                buttontextrender.color = Palette.DisabledClear;
                buttontextrender.material.SetFloat("_Desat", 1f);
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
            }

            if (PlayerControl.LocalPlayer.Data.IsDead && role.Avenging)
            {
                role.Avenging = false;
                role.killer = null;
            }

            if (role.Avenging && role.killer.Data.IsDead)
            {
                role.Avenging = false;
                role.killer = null;
            }

            if (PlayerControl.LocalPlayer.Is(Faction.Madmates) && role.Avenging && (role.killer.Is(Faction.Impostors) || role.killer.Is(Faction.Madmates)))
            {
                role.Avenging = false;
                role.killer = null;
            }

            var data = PlayerControl.LocalPlayer.Data;
            var isDead = data.IsDead;
            var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            var maxDistance = GameOptionsData.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            var flag = (GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks || !data.IsDead) &&
                       (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) &&
                       PlayerControl.LocalPlayer.CanMove;
            var allocs = Physics2D.OverlapCircleAll(truePosition, maxDistance,
                LayerMask.GetMask(new[] { "Players", "Ghost" }));

            DeadBody closestBody = null;
            var closestDistance = float.MaxValue;

            foreach (var collider2D in allocs)
            {
                if (!flag || isDead || collider2D.tag != "DeadBody") continue;
                var component = collider2D.GetComponent<DeadBody>();


                if (!(Vector2.Distance(truePosition, component.TruePosition) <=
                      maxDistance)) continue;

                var distance = Vector2.Distance(truePosition, component.TruePosition);
                if (!(distance < closestDistance)) continue;
                closestBody = component;
                closestDistance = distance;
            }

            killButton.SetCoolDown(0f, 1f);
            role.AvengeButton.SetCoolDown(0f, 1f);

            foreach (var deadPlayer in Murder.KilledPlayers)
            {
                if (closestBody != null && deadPlayer.PlayerId == closestBody.ParentId)
                {
                    var killerplayer = Utils.PlayerById(deadPlayer.KillerId);
                    if ((deadPlayer.KillerId == PlayerControl.LocalPlayer.PlayerId ||
                    killerplayer.Data.IsDead || ((killerplayer.Is(Faction.Impostors) || killerplayer.Is(Faction.Madmates)) &&
                    PlayerControl.LocalPlayer.Is(Faction.Madmates))) && !closestBody.IsDouble()) return;
                    else if (closestBody.IsDouble())
                    {
                        var matches = Murder.KilledPlayers.ToArray().Where(x => x.KillerId == closestBody.ParentId && x.isDoppel == true).ToList();
                        if (matches.Any())
                        {
                            foreach (var role2 in Role.GetRoles(RoleEnum.Doppelganger))
                            {
                                var doppel = (Doppelganger)role2;
                                if (doppel.Player.Data.IsDead || doppel.Player.Data.Disconnected) return;
                            }
                        }
                    }
                }
            }
            KillButtonTarget.SetTarget(role.AvengeButton, closestBody, role);
        }
    }
}
﻿using System;
using HarmonyLib;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using TownOfUs.Modifiers.AssassinMod;
using TownOfUs.Patches;

namespace TownOfUs.CovenRoles.RitualistMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class AddButton
    {
        private static Sprite CycleBackSprite => TownOfUs.CycleBackSprite;
        private static Sprite CycleForwardSprite => TownOfUs.CycleForwardSprite;

        private static Sprite GuessSprite => TownOfUs.GuessSprite;

        private static bool IsExempt(PlayerVoteArea voteArea)
        {
            if (voteArea.AmDead) return true;
            var player = Utils.PlayerById(voteArea.TargetPlayerId);
            if (
                player == null ||
                player.Is(Faction.Coven) ||
                player.Data.IsDead ||
                player.Data.Disconnected
            )
            return true;
            var role = Role.GetRole(player);
            return role != null && role.Criteria();
        }


        public static void GenButton(Ritualist role, PlayerVoteArea voteArea)
        {
            var targetId = voteArea.TargetPlayerId;
            if (IsExempt(voteArea))
            {
                role.Buttons[targetId] = (null, null, null, null);
                return;
            }

            var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;
            var parent = confirmButton.transform.parent.parent;
            
            var nameText = Object.Instantiate(voteArea.NameText, voteArea.transform);
            voteArea.NameText.transform.localPosition = new Vector3(0.55f, 0.12f, -0.1f);
            nameText.transform.localPosition = new Vector3(0.55f, -0.12f, -0.1f);
            nameText.text = "Guess";

            var cycleBack = Object.Instantiate(confirmButton, voteArea.transform);
            var cycleRendererBack = cycleBack.GetComponent<SpriteRenderer>();
            cycleRendererBack.sprite = CycleBackSprite;
            cycleBack.transform.localPosition = new Vector3(-0.5f, 0.15f, -2f);
            cycleBack.transform.localScale = new Vector3(0.23f, 0.23f, 0.23f);
            cycleBack.layer = 5;
            cycleBack.transform.parent = parent;
            var cycleEventBack = new Button.ButtonClickedEvent();
            cycleEventBack.AddListener(Cycle(role, voteArea, nameText, false));
            cycleBack.GetComponent<PassiveButton>().OnClick = cycleEventBack;
            var cycleColliderBack = cycleBack.GetComponent<BoxCollider2D>();
            cycleColliderBack.size = cycleRendererBack.sprite.bounds.size;
            cycleColliderBack.offset = Vector2.zero;
            cycleBack.transform.GetChild(0).gameObject.Destroy();

            var cycleForward = Object.Instantiate(confirmButton, voteArea.transform);
            var cycleRendererForward = cycleForward.GetComponent<SpriteRenderer>();
            cycleRendererForward.sprite = CycleForwardSprite;
            cycleForward.transform.localPosition = new Vector3(-0.2f, 0.15f, -2f);
            cycleForward.transform.localScale = new Vector3(0.23f, 0.23f, 0.23f);
            cycleForward.layer = 5;
            cycleForward.transform.parent = parent;
            var cycleEventForward = new Button.ButtonClickedEvent();
            cycleEventForward.AddListener(Cycle(role, voteArea, nameText, true));
            cycleForward.GetComponent<PassiveButton>().OnClick = cycleEventForward;
            var cycleColliderForward = cycleForward.GetComponent<BoxCollider2D>();
            cycleColliderForward.size = cycleRendererForward.sprite.bounds.size;
            cycleColliderForward.offset = Vector2.zero;
            cycleForward.transform.GetChild(0).gameObject.Destroy();

            var guess = Object.Instantiate(confirmButton, voteArea.transform);
            var guessRenderer = guess.GetComponent<SpriteRenderer>();
            guessRenderer.sprite = GuessSprite;
            guess.transform.localPosition = new Vector3(-0.35f, -0.15f, -2f);
            guess.transform.localScale = new Vector3(0.23f, 0.23f, 0.23f);
            guess.layer = 5;
            guess.transform.parent = parent;
            var guessEvent = new Button.ButtonClickedEvent();
            guessEvent.AddListener(Guess(role, voteArea));
            guess.GetComponent<PassiveButton>().OnClick = guessEvent;
            var bounds = guess.GetComponent<SpriteRenderer>().bounds;
            bounds.size = new Vector3(0.52f, 0.3f, 0.16f);
            var guessCollider = guess.GetComponent<BoxCollider2D>();
            guessCollider.size = guessRenderer.sprite.bounds.size;
            guessCollider.offset = Vector2.zero;
            guess.transform.GetChild(0).gameObject.Destroy();

            role.Guesses.Add(targetId, "None");
            role.Buttons[targetId] = (cycleBack, cycleForward, guess, nameText);
        }

        private static Action Cycle(Ritualist role, PlayerVoteArea voteArea, TextMeshPro nameText, bool forwardsCycle = true)
        {
            void Listener()
            {
                if (MeetingHud.Instance.state == MeetingHud.VoteStates.Discussion) return;
                var currentGuess = role.Guesses[voteArea.TargetPlayerId];
                var guessIndex = currentGuess == "None"
                    ? -1
                    : role.PossibleGuesses.IndexOf(currentGuess);
                if (forwardsCycle)
                {
                    if (++guessIndex >= role.PossibleGuesses.Count)
                        guessIndex = 0;
                }
                else
                {
                    if (--guessIndex < 0)
                        guessIndex = role.PossibleGuesses.Count - 1;
                }

                var newGuess = role.Guesses[voteArea.TargetPlayerId] = role.PossibleGuesses[guessIndex];

                nameText.text = newGuess == "None"
                    ? "Guess"
                    : $"<color=#{role.SortedColorMapping[newGuess].ToHtmlStringRGBA()}>{newGuess}</color>";
            }

            return Listener;
        }

        private static Action Guess(Ritualist role, PlayerVoteArea voteArea)
        {
            void Listener()
            {
                if (
                    MeetingHud.Instance.state == MeetingHud.VoteStates.Discussion ||
                    IsExempt(voteArea) || PlayerControl.LocalPlayer.Data.IsDead
                ) return;
                var targetId = voteArea.TargetPlayerId;
                var currentGuess = role.Guesses[targetId];
                if (currentGuess == "None") return;

                var playerRole = Role.GetRole(voteArea);
                var playerModifier = Modifier.GetModifier(voteArea);

                var toDie = ((playerRole.Name.Contains(currentGuess) && !playerRole.Player.Is(RoleEnum.Hunter)) || playerRole.Name == currentGuess || playerRole.Name.Contains("Mad") && currentGuess == "Madmate") ? playerRole.Player : role.Player;
                if (playerModifier != null)
                    toDie = (playerRole.Name == currentGuess || playerModifier.Name == currentGuess || playerRole.Name.Contains("Mad") && currentGuess == "Madmate") ? playerRole.Player : role.Player;

                var fortified = toDie.IsFortified() && PlayerControl.LocalPlayer != toDie;

                if ((!toDie.Is(RoleEnum.Pestilence) || PlayerControl.LocalPlayer.Is(RoleEnum.Pestilence)) && !fortified)
                {
                    if (PlayerControl.LocalPlayer.Is(ModifierEnum.DoubleShot) && toDie == PlayerControl.LocalPlayer)
                    {
                        var modifier = Modifier.GetModifier<DoubleShot>(PlayerControl.LocalPlayer);
                        if (modifier.LifeUsed == false)
                        {
                            modifier.LifeUsed = true;
                            Coroutines.Start(Utils.FlashCoroutine(Color.red, 1f));
                            ShowHideButtonsRitualist.HideSingle(role, targetId, false, true);
                        }
                        else
                        {
                            AssassinKill.RpcMurderPlayer(toDie, PlayerControl.LocalPlayer);
                            role.RemainingKills--;
                            ShowHideButtonsRitualist.HideSingle(role, targetId, toDie == role.Player);
                            if (toDie.IsLover() && CustomGameOptions.BothLoversDie)
                            {
                                var lover = ((Lover)playerModifier).OtherLover.Player;
                                if (!lover.Is(RoleEnum.Pestilence)) ShowHideButtonsRitualist.HideSingle(role, lover.PlayerId, false);
                            }
                        }
                    }
                    else
                    {
                        AssassinKill.RpcMurderPlayer(toDie, PlayerControl.LocalPlayer);
                        role.RemainingKills--;
                        ShowHideButtonsRitualist.HideSingle(role, targetId, toDie == role.Player);
                        if (toDie.IsLover() && CustomGameOptions.BothLoversDie)
                        {
                            var lover = ((Lover)playerModifier).OtherLover.Player;
                            if (!lover.Is(RoleEnum.Pestilence)) ShowHideButtonsRitualist.HideSingle(role, lover.PlayerId, false);
                        }
                    }
                }
                else
                {
                    ShowHideButtonsRitualist.HideSingle(role, targetId, toDie == role.Player);
                    Coroutines.Start(Utils.FlashCoroutine(Colors.Warden));
                    if (toDie.IsFortified()) Utils.Rpc(CustomRPC.Fortify, (byte)1, toDie.GetWarden().Player.PlayerId);
                }
            }

            return Listener;
        }

        public static void Postfix(MeetingHud __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Ritualist))
            {
                var ritualist = (Ritualist) role;
                ritualist.Guesses.Clear();
                ritualist.Buttons.Clear();
                ritualist.GuessedThisMeeting = false;
            }

            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Ritualist)) return;

            var ritualistRole = Role.GetRole<Ritualist>(PlayerControl.LocalPlayer);
            foreach (var voteArea in __instance.playerStates)
            {
                GenButton(ritualistRole, voteArea);
            }
        }
    }
}

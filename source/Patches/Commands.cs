using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using TownOfUs.CrewmateRoles.MedicMod;
using TownOfUs.Extensions;
using TownOfUs.Roles;

namespace TownOfUs.Patches
{
    public class Commands
    {
        public static bool host = false;
        public static bool error = false;
        public static bool system = false;
        public static bool noaccess = false;

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public class SpecialChat
        {
            public static bool CheckRole(string role)
            {
                return role == "Chameleon" || role == "Altruist" || role == "Amnesiac" || role == "Arsonist" ||
                (role == "Assassin" && CustomGameOptions.AssassinImpostorRole) || role == "Astral" ||
                role == "Attacker" || role == "Aurial" || role == "Avenger" || role == "Blackmailer" ||
                role == "Bodyguard" || role == "Bomber" || role == "BountyHunter" || role == "Captain" ||
                role == "Conjurer" || role == "Converter" || role == "Coven" || role == "CovenLeader" ||
                role == "Crewmate" || role == "Crusader" || role == "Deputy" || role == "Detective" ||
                role == "Doctor" || role == "Doomsayer" || role == "Doppelganger" || role == "Engineer" ||
                role == "Escapist" || role == "Executioner" || role == "Fighter" || role == "Glitch"
                || role == "Grenadier" || role == "Guard" || role == "GuardianAngel" || role == "Impostor"
                || role == "HexMaster" || role == "Hunter" || role == "Hypnotist" || role == "Imitator"
                || role == "Infectious" || role == "Informant" || role == "Investigator" || role == "Jailor"
                || role == "Janitor" || role == "Jester" || role == "Juggernaut" || role == "Knight"
                || role == "Lighter" || role == "Mafioso" || role == "Manipulator" || role == "Maul"
                || role == "Mayor" || role == "Medic" || role == "Medium" || role == "Miner"
                || role == "Morphling" || role == "Mutant" || role == "Mystic" || role == "Oracle"
                || role == "Paranoïac" || role == "Plaguebearer" || role == "Poisoner" || role == "Politician"
                || role == "PotionMaster" || role == "Prosecutor" || role == "Reviver" || role == "Ritualist"
                || role == "Seer" || role == "SerialKiller" || role == "Sheriff" || role == "Shifter"
                || role == "Shooter" || role == "Snitch" || role == "SoulCollector" || role == "Superstar"
                || role == "SoulCatcher" || role == "BlackWolf" || role == "Spiritualist" || role == "Spy"
                || role == "Survivor" || role == "Swapper" || role == "Swooper" || role == "TalkativeWolf"
                || role == "TimeLord" || role == "Tracker" || role == "Transporter" || role == "Trapper"
                || role == "Troll" || role == "Undertaker" || role == "Vampire" || role == "Vigilante"
                || role == "Villager" || role == "Vulture" || role == "Warden" || role == "Warlock"
                || role == "Werewolf" || role == "WhiteWolf" || role == "Witch" || role == "Sorcerer"
                || role == "Veteran";
            }
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer , ref string chatText)
            {
                if (__instance != HudManager.Instance.Chat)
                    return true;

                if (chatText.ToLower().StartsWith("/help"))
                {
                    if (sourcePlayer.IsDev() && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "<b><size=3>COMMANDS</size></b>\n\nEveryone:\n\n/help - Displays this menu\n/infoup - See infos about /up command\n/up - Choose any role in the game (if enabled)\n/allup - See all currently chosen roles\n/death - Shows your death reason\n\nHost only:\n\nShift + G + ENTER - Force the game to end\n/msg [message] - Send a message as host\n/id - See players ids\n/kick [id] - Kick a player by its id\n/ban [id] - Ban a player by its id\n\nDev Commands:\n\n/fly - Disable / Enable Colliders in Lobby.\n/dmsg [message] - Sends a message that everyone will see.\n/dev - Get a list of all current devs & friendcodes.\n/changecode [friend code] - Changes your friend code account. (Will be reset to normal if you restart Among Us)\n/randomise - Gives you a completely new random username and appearance\n/default - Reset your outfit to default\n/status - Show / Hide your dev status";
                        system = true;
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "<b><size=3>COMMANDS</size></b>\n\nEveryone:\n\n/help - Displays this menu\n/infoup - See infos about /up command\n/up - Choose any role in the game (if enabled)\n/allup - See all currently chosen roles\n/death - Shows your death reason\n\nHost only:\n\nShift + G + ENTER - Force the game to end\n/msg [message] - Send a message as host\n/id - See players ids\n/kick [id] - Kick a player by its id\n/ban [id] - Ban a player by its id";
                        system = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("/secret") && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId) // Real secret fr fr
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley",
                        UseShellExecute = true
                    });
                }

                if (chatText.ToLower().StartsWith("/msg "))
                {
                    if (GameData.Instance.GetHost() == sourcePlayer.Data)
                    {
                        var message = chatText[5..];
                        host = true;
                        DestroyableSingleton<ChatController>.Instance.AddChat(sourcePlayer, message, false);
                        return false;
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "You don't have access to this command!";
                        noaccess = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("/allup"))
                {
                    if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        string message = "All currently chosen roles:\n";
                        foreach (var playerRole in RpcHandling.Upped.Values.ToArray().OrderBy(x => Guid.NewGuid()))
                        {
                            message += $"{playerRole}\n";
                        }
                        message.Remove(message.Length - 1, 1);
                        if (!RpcHandling.Upped.Any(x => x.Key == PlayerControl.AllPlayerControls.ToArray().Any()))
                        {
                            message = "No roles have been selected yet.";
                        }
                        chatText = message;
                        system = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("/infoup"))
                {
                    if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "<b><size=3>UP</size></b>\n\nThe command \"/up\" allows you to choose any role in the game.\nThe host can freely enable / disable this command whenever they want using settings.\nBut, this command only works in Classic / Werewolf gamemodes. It also won't apply if the selected role isn't turned on by the host.\nAll players can see which roles have been chosen during the game using \"/allup\".\nTo use this command, you need to make sure to type the name of the role correctly and without space, the role must have a maj (ex: SerialKiller, Sheriff...).";
                        system = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("/up "))
                {
                    if (CustomGameOptions.AllowUp && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        if (!LobbyBehaviour.Instance)
                        {
                            chatText = "You can only use this command in lobby!";
                            error = true;
                        }
                        else if (CustomGameOptions.GameMode != GameMode.Classic && CustomGameOptions.GameMode != GameMode.Werewolf)
                        {
                            chatText = "This command can only be used on Classic / Werewolf gamemodes.";
                            error = true;
                        }
                        else if (CheckRole(chatText[4..]))
                        {
                            if (!RpcHandling.Upped.ContainsValue(chatText[4..]))
                            {
                                if (!RpcHandling.Upped.ContainsKey(sourcePlayer))
                                {
                                    RpcHandling.Upped.Add(sourcePlayer, chatText[4..]);
                                    Utils.Rpc(CustomRPC.AddUp, sourcePlayer.PlayerId, chatText[4..]);
                                }
                                else
                                {
                                    RpcHandling.Upped.Remove(sourcePlayer);
                                    RpcHandling.Upped.Add(sourcePlayer, chatText[4..]);
                                    Utils.Rpc(CustomRPC.AddUp, sourcePlayer.PlayerId, chatText[4..]);
                                }
                                chatText = $"You have chosen {chatText[4..]} role for the next game. Type \"Cancel\" to cancel.";
                                system = true;
                            }
                            else
                            {
                                chatText = "Sorry, this role has already been taken. Please try another role.";
                                error = true;
                            }
                        }
                        else if (chatText[4..] == "Cancel")
                        {
                            if (!RpcHandling.Upped.ContainsKey(sourcePlayer))
                            {
                                chatText = "You don't have any selected role for the next game.";
                                error = true;
                            }
                            else
                            {
                                RpcHandling.Upped.Remove(sourcePlayer);
                                Utils.Rpc(CustomRPC.AddUp, sourcePlayer.PlayerId, "Cancel");
                                chatText = "Current selected role removed.";
                                system = true;
                            }
                        }
                        else
                        {
                            chatText = "Make sure you typed the role correctly.";
                            error = true;
                        }
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "This command was disabled by the host.";
                        noaccess = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("/id"))
                {
                    if (GameData.Instance.GetHost() == sourcePlayer.Data && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        string message = "All players ids:\n";
                        foreach (var player in PlayerControl.AllPlayerControls.ToArray().OrderBy(x => Guid.NewGuid()))
                        {
                            message += $"{player.GetDefaultOutfit().PlayerName}: {player.PlayerId}\n";
                        }
                        message.Remove(message.Length - 1, 1);
                        chatText = message;
                        system = true;
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "You don't have access to this command!";
                        noaccess = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("/kick "))
                {
                    if (GameData.Instance.GetHost() == sourcePlayer.Data && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        if (chatText[6..].IsNullOrWhiteSpace())
                        {
                            chatText = "You must specify a player id as parameter, use /id to get the list of all players ids.";
                            error = true;
                        }
                        if (Convert.ToByte(chatText[6..]) == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You can't kick yourself!";
                            error = true;
                        }
                        else
                        {
                            var id = Convert.ToByte(chatText[6..]);
                            var playerWithId = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId == id).ToList();
                            if (!playerWithId.Any())
                            {
                                chatText = "You must give a valid player id, use /id to get the list of all players ids.";
                                error = true;
                            }
                            else
                            {
                                foreach (var player in playerWithId)
                                {
                                    AmongUsClient.Instance.KickWithReason(player.OwnerId, "You were kicked from the lobby.");
                                }
                                chatText = "The player was kicked successfully.";
                                system = true;
                            }
                        }
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "You don't have access to this command!";
                        noaccess = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("/ban "))
                {
                    if (GameData.Instance.GetHost() == sourcePlayer.Data && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        if (chatText[5..].IsNullOrWhiteSpace())
                        {
                            chatText = "You must specify a player id as parameter, use /id to get the list of all players ids.";
                            error = true;
                        }
                        if (Convert.ToByte(chatText[5..]) == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You can't ban yourself!";
                            error = true;
                        }
                        else
                        {
                            var id = Convert.ToByte(chatText[5..]);
                            var playerWithId = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId == id).ToList();
                            if (!playerWithId.Any())
                            {
                                chatText = "You must give a valid player id, use /id to get the list of all players ids.";
                                error = true;
                            }
                            else
                            {
                                foreach (var player in playerWithId)
                                {
                                    AmongUsClient.Instance.Ban(player.OwnerId);
                                }
                                chatText = "The player was banned successfully.";
                                system = true;
                            }
                        }
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "You don't have access to this command!";
                        noaccess = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }
                
                if (chatText.ToLower().StartsWith("/death"))
                {
                    if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        var playerRole = Role.GetRole(PlayerControl.LocalPlayer);
                        if (!sourcePlayer.Data.IsDead)
                        {
                            if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                            {
                                chatText = "You can only use this command after death!";
                                error = true;
                            }
                        }
                        else if (playerRole.DeathReason == DeathReasons.Spectator && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You are the spectator, your died because your role can't influence the game.";
                            system = true;
                        }
                        else if (playerRole.DeathReason == DeathReasons.Exiled && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You died because you were ejected.";
                            system = true;
                        }
                        else if (playerRole.DeathReason == DeathReasons.Misfired && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You died because you misfired.";
                            system = true;
                        }
                        else if (playerRole.DeathReason == DeathReasons.Suicided && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You died because you suicided.";
                            system = true;
                        }
                        else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        {
                            foreach (var deadPlayer in Murder.KilledPlayers)
                            {
                                if (deadPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                                {
                                    var killerPlayer = Utils.PlayerById(deadPlayer.KillerId);
                                    var roleName = killerPlayer.Is(RoleEnum.Glitch) ? $"The {Role.GetRole(killerPlayer).Name}" : Role.GetRole(killerPlayer).Name;
                                    if (CustomGameOptions.DeadSeeRoles && Utils.ShowDeadBodies)
                                    {
                                        chatText = $"You were killed by {killerPlayer.GetDefaultOutfit().PlayerName}.Their role is {roleName}.\nYour death reason is: {playerRole.DeathReason}.";
                                    }
                                    else chatText = $"You were killed by {killerPlayer.GetDefaultOutfit().PlayerName}.\nYour death reason is: {playerRole.DeathReason}.";
                                }
                                system = true;
                            }
                        }
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "You can only use this command in game!";
                        error = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public class ChatFixName
        {
            public static bool Prefix(ChatBubble __instance)
            {
                if (host)
                {
                    __instance.NameText.text = "HOST MESSAGE";
                    __instance.NameText.color = Palette.Orange;
                    __instance.NameText.ForceMeshUpdate(true, true);
                    __instance.Xmark.enabled = false;
			        __instance.Background.color = Palette.White;
                    __instance.votedMark.enabled = false;
                    host = false;
                    return false;
                }
                else if (error)
                {
                    __instance.NameText.text = "ERROR";
                    __instance.NameText.color = Palette.ImpostorRed;
                    __instance.NameText.ForceMeshUpdate(true, true);
                    __instance.Xmark.enabled = true;
			        __instance.Background.color = Palette.White;
                    __instance.votedMark.enabled = false;
                    error = false;
                    return false;
                }
                else if (system)
                {
                    __instance.NameText.text = "SYSTEM MESSAGE";
                    __instance.NameText.color = Palette.CrewmateBlue;
                    __instance.NameText.ForceMeshUpdate(true, true);
                    __instance.Xmark.enabled = false;
			        __instance.Background.color = Palette.White;
                    __instance.votedMark.enabled = false;
                    system = false;
                    return false;
                }
                else if (noaccess)
                {
                    __instance.NameText.text = "NO ACCESS";
                    __instance.NameText.color = Palette.Blue;
                    __instance.NameText.ForceMeshUpdate(true, true);
                    __instance.Xmark.enabled = true;
			        __instance.Background.color = Palette.White;
                    __instance.votedMark.enabled = false;
                    noaccess = false;
                    return false;
                }
                else return true;
            }
        }
    }
}
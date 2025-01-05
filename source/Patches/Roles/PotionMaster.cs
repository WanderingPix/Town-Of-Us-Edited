using System.Collections.Generic;
using System.Linq;
using TownOfUs.Extensions;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class PotionMaster : Role
    {
        public PotionMaster(PlayerControl owner) : base(owner)
        {
            Name = "Potion Master";
            ImpostorText = () => "What A Good Drink";
            TaskText = () => "Create Potions to get special abilities\nCurrent Potion: " + PotionType + "\nFake Tasks:";
            Color = Patches.Colors.Coven;
            RoleType = RoleEnum.PotionMaster;
            AddToRoleHistory(RoleType);
            Faction = Faction.Coven;
            Cooldown = CustomGameOptions.CovenKCD;
            PotionCooldown = CustomGameOptions.PotionCD;
        }

        public KillButton _sabotageButton;
        public KillButton _potionButton;
        public PlayerControl ClosestPlayer;
        public PlayerControl ControlledPlayer;
        public float Cooldown;
        public bool coolingDown => Cooldown > 0f;
        public float PotionCooldown;
        public bool PotioncoolingDown => PotionCooldown > 0f;
        public float TimeRemaining;
        public bool UsingPotion => TimeRemaining > 0f;
        public string Potion = "null";
        public string PotionType = "None";
        public bool Enabled;

        public void Kill(PlayerControl target)
        {
            // Check if the Coven can kill
            if (Cooldown > 0)
                return;

            if (target.Is(Faction.Coven))
                return;

            Utils.Interact(PlayerControl.LocalPlayer, target, true);

            // Set the last kill time
            if (UsingPotion && Potion == "Strength")
            {
                Cooldown = CustomGameOptions.StrengthKCD;
            }
            else Cooldown = CustomGameOptions.CovenKCD;
        }
        public void UsePotion()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            if (Potion == "Invisibility")
            {
                Swoop();
            }
        }
        public void StopPotion()
        {
            Enabled = false;
            PotionCooldown = CustomGameOptions.PotionCD;
            if (Potion == "Invisibility")
            {
                UnSwoop();
            }
            Potion = "null";
        }
        public void GetPotion()
        {
            var random = new System.Random();
            var randomPotion = new List<string>{"Speed", "Strength", "Invisibility", "Shield"};
            int index = random.Next(randomPotion.Count);
            var chosenPotion = randomPotion[index];
            randomPotion.RemoveAt(index);
            Potion = chosenPotion;
        }
        public void Swoop()
        {
            Utils.Swoop(Player);
            if (Player.Data.IsDead)
            {
                TimeRemaining = 0f;
            }
        }


        public void UnSwoop()
        {
            Utils.Unmorph(Player);
            Player.myRend().color = Color.white;
        }
        public KillButton SabotageButton
        {
            get => _sabotageButton;
            set
            {
                _sabotageButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }
        public KillButton PotionButton
        {
            get => _potionButton;
            set
            {
                _potionButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
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
        public float PotionTimer()
        {
            if (!PotioncoolingDown) return 0f;
            else if (!PlayerControl.LocalPlayer.inVent)
            {
                PotionCooldown -= Time.deltaTime;
                return PotionCooldown;
            }
            else return PotionCooldown;
        }

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var covenTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            covenTeam.Add(PlayerControl.LocalPlayer);
            var toAdd = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Coven) && x != PlayerControl.LocalPlayer).ToList();
            foreach (var player in toAdd)
            {
                covenTeam.Add(player);
            }
            __instance.teamToShow = covenTeam;
        }
    }
}
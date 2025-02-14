using System.Collections.Generic;
using System.Linq;
using TownOfUs.Extensions;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Swapper : Role
    {
        public readonly List<GameObject> Buttons = new List<GameObject>();

        public readonly List<bool> ListOfActives = new List<bool>();


        public Swapper(PlayerControl player) : base(player)
        {
            Name = "Swapper";
            ImpostorText = () => "Swap The Votes Of Two People";
            TaskText = () => "Swap two people's votes to save the Crew!";
            Color = Patches.Colors.Swapper;
            RoleType = RoleEnum.Swapper;
            Faction = Faction.Crewmates;
            Alignment = Alignment.CrewmateSupport;
            AddToRoleHistory(RoleType);
        }
    }
}
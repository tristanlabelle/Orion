using System.Linq;
using OpenTK.Math;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.GameLogic.Skills;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class HarvestUserCommand : UserInputCommand
    {
        public HarvestUserCommand(UserInputManager inputManager)
            : base(inputManager) {}

        public override void OnClick(Vector2 location)
        {
            ResourceNode resourceNode = World.Entities
                .Intersecting(location)
                .OfType<ResourceNode>()
                .FirstOrDefault(node => node.IsHarvestableByFaction(LocalFaction));
            if (resourceNode == null) return;

            InputManager.LaunchHarvest(resourceNode);
        }
    }
}

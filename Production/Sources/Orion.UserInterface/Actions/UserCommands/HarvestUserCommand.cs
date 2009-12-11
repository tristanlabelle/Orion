using System.Linq;
using OpenTK.Math;
using Orion.Commandment;
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
                .OfType<ResourceNode>()
                .FirstOrDefault(node => node.BoundingRectangle.ContainsPoint(location));
            if (resourceNode == null || !resourceNode.IsHarvestableByFaction(LocalFaction))
                return;

            InputManager.LaunchHarvest(resourceNode);
        }
    }
}

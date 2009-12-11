using System.Linq;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class HarvestUserCommand : UserInputCommand
    {
        private UserInputManager inputManager;

        public HarvestUserCommand(UserInputManager manager)
        {
            inputManager = manager;
        }

        public override void Execute(Entity entity)
        {
            ResourceNode resourceNode = entity as ResourceNode;
            if (resourceNode != null)
            {
                inputManager.LaunchHarvest(resourceNode);
                return;
            }

            Unit unit = entity as Unit;
            if (unit != null && unit.HasSkill<ExtractAlageneSkill>())
            {
                ResourceNode alageneNode = inputManager.World.Entities
                    .OfType<ResourceNode>()
                    .First(node => node.Position == unit.Position);
                if (alageneNode.IsHarvestableByFaction(inputManager.LocalFaction))
                    inputManager.LaunchHarvest(alageneNode);
            }
        }

        public override void Execute(Vector2 point)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;
using Skills = Orion.GameLogic.Skills;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;
using OpenTK.Math;

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
            if (entity is ResourceNode)
                inputManager.LaunchHarvest((ResourceNode)entity);
            else
                if (entity is Unit)
                    if (((Unit)entity).HasSkill<Skills.ExtractAlageneSkill>())
                    {
                        ResourceNode alageneNode = inputManager.Commander.Faction.World.Entities
                            .OfType<ResourceNode>()
                            .First(node => node.Position == ((Unit)entity).Position);
                        if (alageneNode.IsHarvestableByFaction(inputManager.Commander.Faction))
                            inputManager.LaunchHarvest(alageneNode);
                    }
        }

        public override void Execute(Vector2 point)
        {
            // todo: don't silently fail
        }
    }
}

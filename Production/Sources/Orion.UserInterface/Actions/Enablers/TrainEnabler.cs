﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Skills = Orion.GameLogic.Skills;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;

namespace Orion.UserInterface.Actions.Enablers
{
    public class TrainEnabler : ActionEnabler
    {
        private class TrainUserCommand : ImmediateUserCommand
        {
            private UnitType type;
            private UserInputManager inputManager;

            public TrainUserCommand(UserInputManager manager, UnitType type)
            {
                this.type = type;
                inputManager = manager;
            }

            public override void Execute()
            {
                inputManager.LaunchTrain(type);
            }
        }

        private UnitTypeRegistry registry;

        public TrainEnabler(UserInputManager inputManager, ActionFrame frame, UnitTypeRegistry typeRegistry)
            : base(inputManager, frame)
        {
            registry = typeRegistry;
        }

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (type.HasSkill<Skills.Train>())
            {
                Skills.Train train = type.GetSkill<Skills.Train>();
                int x = 0;
                int y = 3;
                foreach (UnitType unitType in registry.Where(t => !t.IsBuilding))
                {
                    if (train.Supports(unitType))
                    {
                        // find an empty slot
                        while (buttonsArray[x, y] != null)
                        {
                            x++;
                            if (x == 4)
                            {
                                x = 0;
                                y--;
                            }
                        }
                        ImmediateUserCommand command = new TrainUserCommand(inputManager, unitType);
                        buttonsArray[x, y] = new ImmediateActionButton(container, inputManager, unitType.Name, Keys.None, command);
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;
using Orion.UserInterface.Widgets;
using Skills = Orion.GameLogic.Skills;

namespace Orion.UserInterface
{
    public class UnitTypeActionProvider : IActionProvider
    {
        #region Fields
        private ActionButton[,] buttons = new ActionButton[4, 4];
        private UserInputManager inputManager;
        private ActionFrame container;
        #endregion

        #region Constructors
        public UnitTypeActionProvider(UserInputManager manager, ActionFrame container, UnitType type)
        {
            inputManager = manager;
            this.container = container;
            Texture defaultTexture = null;// new Texture(40, 40, TextureFormat.Rgb, new byte[40 * 40 * 3]);

            // Harcode Paradise!
            buttons[0, 0] = new ActionButton(defaultTexture, "Cancel", "c", CancelAction);
            if (type.HasSkill<Skills.Move>()) buttons[0, 1] = new ActionButton(defaultTexture, "Move", "m", AssignMove);
            if (type.HasSkill<Skills.Attack>()) buttons[0, 2] = new ActionButton(defaultTexture, "Attack", "a", AssignAttack);
            if (type.HasSkill<Skills.Harvest>()) buttons[1, 0] = new ActionButton(defaultTexture, "Harvest", "h", AssignHarvest);
            if (type.HasSkill<Skills.Build>())
            {
                buttons[1, 1] = new ActionButton(defaultTexture, "Repair", "r", AssignRepair);
                buttons[3, 0] = new ActionButton(defaultTexture, "Build", "b", AssignBuild);
            }
        }
        #endregion

        #region Indexers
        public ActionButton this[int x, int y]
        {
            get { return buttons[x, y]; }
        }
        #endregion

        #region Methods
        #region Assign Commands

        private void CancelAction(Button source)
        {
            inputManager.Cancel();
        }

        private void AssignMove(Button source)
        {
            inputManager.SelectedCommand = MouseDrivenCommand.Move;
        }

        private void AssignAttack(Button source)
        {
            inputManager.SelectedCommand = MouseDrivenCommand.Attack;
        }

        private void AssignHarvest(Button source)
        {
            inputManager.SelectedCommand = MouseDrivenCommand.Harvest;
        }

        private void AssignRepair(Button source)
        {
            inputManager.SelectedCommand = MouseDrivenCommand.Repair;
        }

        private void AssignBuild(Button source)
        {
            // do cool stuff
        }

        #endregion
        #endregion
    }
}

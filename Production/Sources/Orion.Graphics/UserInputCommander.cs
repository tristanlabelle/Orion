using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.Graphics;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Graphics
{
    /// <summary>
    /// A <see cref="Commander"/> which gives <see cref="Command"/>s based on user input.
    /// </summary>
    public sealed class UserInputCommander : Commander
    {
        #region Fields
        private SelectionManager selectionManager;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor For a commander that can listen input to create commands
        /// </summary>
        /// <param name="faction">the faction of the player.</param>
        public UserInputCommander(Faction faction)
            : base(faction)
        {
            this.selectionManager = new SelectionManager(faction);
            
        }
        #endregion

        #region Proprieties
        /// <summary>
        /// Gets the <see cref="SelectionManager"/> this <see cref="UserInputCommander"/>
        /// uses internally to detect selection state.
        /// </summary>
        public SelectionManager SelectionManager
        {
            get { return selectionManager; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Listen of Mouse Event, implemented the right click associated to the move task
        /// </summary>
        /// <param name="position">The location of the cursor when the click occured.</param>
        /// <param name="button">The mouse button that was pressed or released.</param>
        /// <param name="pressed">True if the button was pressed, false otherwise.</param>
        public void OnMouseButton(Vector2 position, MouseButton button, bool pressed)
        {
            selectionManager.OnMouseButton(position, button, pressed);
            if (button == MouseButton.Right && pressed)
            {
                if (selectionManager.SelectedUnits.Count() != 0)
                {
                    List<Unit> unitsToAssignTask = selectionManager.SelectedUnits.Where(unit => unit.Faction == Faction).ToList();
                    if (unitsToAssignTask.Count() != 0)
                    {
                        Unit enemy = World.Units.FirstOrDefault(unit => unit.Circle.Contains(position));
                        Command command;
                        if (enemy != null && enemy.Faction != this.Faction)// TODO: CHECK IF Its Not Either an ally.
                        {
                                command = new Attack(Faction, unitsToAssignTask, enemy);
                        }
                        else
                        {
                            command = new Move(Faction, unitsToAssignTask, position);
                        }
                        GenerateCommand(command);
                    }
                }
            }
        }

        public void OnKeyDown(Keys key)
        {
            if (key == Keys.S)
            {
                if (selectionManager.SelectedUnits.Count() != 0)
                {
                    List<Unit> unitsToAssignTask = selectionManager.SelectedUnits.Where(unit => unit.Faction == Faction).ToList();
                    if (unitsToAssignTask.Count() != 0)
                    {
                        Command command = new Cancel(Faction, unitsToAssignTask);
                        GenerateCommand(command);
                    }
                }
            }
        }

        internal void OnMouseMove(Vector2 position)
        {
            selectionManager.OnMouseMove(position);
        }
        #endregion

      
    }
}

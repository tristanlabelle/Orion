using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.Graphics;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment;

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
            this.selectionManager = new SelectionManager(faction.World);
            
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
                    List<Unit> unitsToMove = selectionManager.SelectedUnits.Where(unit => unit.Faction == Faction).ToList();
                    Command command = new Move(Faction, unitsToMove, position);

                    GenerateCommand(command);
                }
            }
        }
        #endregion

        internal void OnMouseMove(Vector2 position)
        {
            selectionManager.OnMouseMove(position);
        }
    }
}

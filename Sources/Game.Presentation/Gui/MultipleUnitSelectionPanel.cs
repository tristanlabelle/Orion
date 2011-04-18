using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides a display of the selected units for selection of multiple units.
    /// </summary>
    public sealed partial class MultipleUnitSelectionPanel : ContentControl
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly WrapLayout wrap;
        private readonly Stack<EntityButton> unusedButtons = new Stack<EntityButton>();
        #endregion

        #region Constructors
        public MultipleUnitSelectionPanel(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;

            Content = wrap = new WrapLayout()
            {
                Direction = Direction.PositiveX,
                ChildGap = 4,
                SeriesGap = 4
            };
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the selection should change to a specific <see cref="Entity"/>.
        /// </summary>
        public event Action<MultipleUnitSelectionPanel, Entity> EntityFocused;

        /// <summary>
        /// Raised when a <see cref="Entity"/> should be removed from the selection.
        /// </summary>
        public event Action<MultipleUnitSelectionPanel, Entity> EntityDeselected;
        #endregion

        #region Methods
        /// <summary>
        /// Clears all <see cref="Entity">entities</see> from this panel.
        /// </summary>
        public void ClearEntities()
        {
            while (wrap.Children.Count > 0)
            {
                EntityButton button = (EntityButton)wrap.Children[wrap.Children.Count - 1];
                button.Entity = null;
                wrap.Children.RemoveAt(wrap.Children.Count - 1);
                unusedButtons.Push(button);
            }
        }

        /// <summary>
        /// Sets the <see cref="Entity">entities</see> to be displayed by this panel.
        /// </summary>
        /// <param name="entities">The <see cref="Entity">entities</see> to be displayed.</param>
        public void SetEntities(IEnumerable<Entity> entities)
        {
            Argument.EnsureNotNull(entities, "units");

            foreach (Entity entity in entities)
            {
                if (entity == null) throw new ArgumentException("entities");

                EntityButton button = GetButton();
                button.Entity = entity;
                wrap.Children.Add(button);
            }
        }

        private EntityButton GetButton()
        {
            return unusedButtons.Count > 0 ? unusedButtons.Pop() : new EntityButton(this);
        }
        #endregion
    }
}

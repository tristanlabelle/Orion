using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
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
        private readonly Stack<UnitButton> unusedButtons = new Stack<UnitButton>();
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
        /// Raised when the selection should change to a specific unit.
        /// </summary>
        public event Action<MultipleUnitSelectionPanel, Unit> UnitSelected;

        /// <summary>
        /// Raised when a unit should be removed from the selection.
        /// </summary>
        public event Action<MultipleUnitSelectionPanel, Unit> UnitDeselected;
        #endregion

        #region Methods
        /// <summary>
        /// Clears all units from this panel.
        /// </summary>
        public void ClearUnits()
        {
            while (wrap.Children.Count > 0)
            {
                UnitButton button = (UnitButton)wrap.Children[wrap.Children.Count - 1];
                button.Unit = null;
                wrap.Children.RemoveAt(wrap.Children.Count - 1);
                unusedButtons.Push(button);
            }
        }

        /// <summary>
        /// Sets the units to be displayed by this panel.
        /// </summary>
        /// <param name="units">The units to be displayed.</param>
        public void SetUnits(IEnumerable<Unit> units)
        {
            Argument.EnsureNotNull(units, "units");

            foreach (Unit unit in units)
            {
                if (unit == null) throw new ArgumentException("units");

                UnitButton button = GetButton();
                button.Unit = unit;
                wrap.Children.Add(button);
            }
        }

        private UnitButton GetButton()
        {
            return unusedButtons.Count > 0 ? unusedButtons.Pop() : new UnitButton(this);
        }
        #endregion
    }
}

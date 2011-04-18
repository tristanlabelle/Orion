using System;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Handles the selection of <see cref="Entity"/>s using the mouse and keyboard.
    /// </summary>
    public sealed class SelectionManager
    {
        #region Fields
        public static readonly int SelectionLimit = int.MaxValue;
        
        private readonly Selection selection;
        private Entity focusedPrototype;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SelectionManager"/> from the <see cref="Faction"/>
        /// which is being controlled.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> used for now.</param>
        public SelectionManager(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.selection = new Selection(faction.World, faction, SelectionLimit);
            this.selection.Changed += OnSelectionChanged;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the prototype that is currently focused has changed.
        /// </summary>
        public event Action<SelectionManager> FocusedPrototypeChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the selection.
        /// </summary>
        public Selection Selection
        {
            get { return selection; }
        }

        /// <summary>
        /// Gets the prototype of <see cref="Entity"/> which currently has the focus.
        /// </summary>
        public Entity FocusedPrototype
        {
            get { return focusedPrototype; }
            set
            {
                if (value == focusedPrototype) return;

                if (value == null)
                {
                    Debug.Assert(focusedPrototype == value);
                    return;
                }

                if (selection.None(entity => Identity.GetPrototype(entity) == value))
                    throw new ArgumentException("The focused unit type must be of a type present in the selection.");

                focusedPrototype = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Entity"/> that has the focus.
        /// </summary>
        public Entity FocusedEntity
        {
            get { return selection.FirstOrDefault(entity => Identity.GetPrototype(entity) == focusedPrototype); }
        }
        #endregion

        #region Methods
        #region Selected Unit Type
        /// <summary>
        /// Resets the focused prototype to its default value according to the current selection.
        /// </summary>
        public void ResetFocusedPrototype()
        {
            Entity newSelectedPrototype = selection
                .Select(entity => Identity.GetPrototype(entity))
                .FirstOrDefault();
            if (newSelectedPrototype == focusedPrototype) return;

            focusedPrototype = newSelectedPrototype;
            FocusedPrototypeChanged.Raise(this);
        }

        private void UpdateFocusedPrototype()
        {
            bool isStillValid = focusedPrototype != null && selection.Any(e => Identity.GetPrototype(e) == focusedPrototype);
            if (isStillValid) return;

            var maxGroup = selection
                .GroupBy(e => Identity.GetPrototype(e))
                .WithMaxOrDefault(group => group.Count());

            Entity newFocusedPrototype = maxGroup == null ? null : maxGroup.Key;
            if (newFocusedPrototype == focusedPrototype) return;

            focusedPrototype = newFocusedPrototype;
            FocusedPrototypeChanged.Raise(this);
        }

        private void OnSelectionChanged(Selection sender)
        {
            UpdateFocusedPrototype();
        }
        #endregion
        #endregion
    }
}

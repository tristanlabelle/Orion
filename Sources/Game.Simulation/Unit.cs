using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents an in-game unit, which can be a character, a vehicle or a building,
    /// depending on its <see cref="Entity"/>.
    /// </summary>
    [Serializable]
    public sealed class Unit : Entity
    {
        #region Constructors
        // Compatibility constructor so it isn't a pain in the ass to use the XmlDeserializer
        internal Unit(Handle handle)
            : base(handle)
        {
            // all units need a task queue, but entity templates don't (and shouldn't, really) reflect this
            Components.Add(new TaskQueue(this));
        }

        /// <summary>
        /// Initializes a new <see cref="Entity"/> from its identifier,
        /// <see cref="Entity"/> and <see cref="World"/>.
        /// </summary>
        /// <param name="handle">A unique handle for this <see cref="Entity"/>.</param>
        /// <param name="type">
        /// The <see cref="Entity"/> which determines
        /// the stats and capabilities of this <see cref="Entity"/>
        /// </param>
        /// <param name="position">The initial position of the <see cref="Entity"/>.</param>
        /// <param name="faction">The <see cref="T:Faction"/> this <see cref="Entity"/> is part of.</param>
        internal Unit(Handle handle, Entity prototype, Faction faction, Vector2 position)
            : base(faction.World, handle)
        {
            Argument.EnsureNotNull(prototype, "meta");
            Argument.EnsureNotNull(faction, "faction");

            foreach (Component component in prototype.Components)
                Components.Add(component.Clone(this));

            if (Identity.Prototype == null) Identity.Prototype = prototype;
            Components.Get<Spatial>().Position = position;

            FactionMembership factionMembership = Components.TryGet<FactionMembership>();
            if (factionMembership == null)
            {
                factionMembership = new FactionMembership(this);
                Components.Add(factionMembership);
            }

            factionMembership.Faction = faction;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Temporary measure until we broadly use the identity component.
        /// </summary>
        public Unit Type
        {
            get { return this; }
            set
            {
                Argument.EnsureNotNull(value, "Type");

                Identity newTypeIdentity = value.Identity;

                Identity identity = Identity;
                identity.Name = newTypeIdentity.Name;
                identity.VisualIdentity = newTypeIdentity.VisualIdentity;
                identity.SoundIdentity = newTypeIdentity.SoundIdentity;

                identity.Upgrades.Clear();
                foreach (UnitTypeUpgrade upgrade in value.Identity.Upgrades)
                    identity.Upgrades.Add(upgrade);
            }
        }
        #endregion
    }
}


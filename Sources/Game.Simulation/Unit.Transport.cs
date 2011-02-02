using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;
using System.Diagnostics;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Contains the attempt at basing transport on components.
    /// </summary>
    public partial class Unit : Entity
    {
        private void InitTransport()
        {
            Transport transport = new Transport(this);
            transport.CanTransport = CanTransport;
            transport.Capacity = type.GetBaseStat(TransportSkill.CapacityStat);
            this.AddComponent(transport);
        }

        public bool CanTransport(Entity e)
        {
            Unit unit = e as Unit;
            if (unit == null) return false;
            return unit.HasSkill<MoveSkill>()
                && !unit.IsAirborne
                && !unit.HasSkill<TransportSkill>();
        }

        public bool IsEmbarked
        {
            get { return HasComponent<Spatial>(); }
        }

        public bool IsTransportFull
        {
            get
            {
                Debug.Assert(HasComponent<Transport>(), "No Transport component!");
                Transport transport = GetComponent<Transport>();
                return transport.Capacity == transport.LoadSize;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using System.Diagnostics;

namespace Orion.Game.Simulation.Components
{
    public class TrainProgress : Component
    {
        #region Fields
        public static readonly EntityStat RequiredTimeStat = new EntityStat(typeof(TrainProgress), StatType.Real, "RequiredTime", "Temps requis");

        private static readonly HashSet<Type> essentialComponents = new HashSet<Type>()
        {
            typeof(TrainProgress), typeof(Identity), typeof(Spatial), typeof(Health)
        };

        private float timeSpent;
        private float requiredTime;
        private TrainProgressType progressType;
        private HashSet<Component> hiddenComponents = new HashSet<Component>();
        #endregion

        #region Constructors
        public TrainProgress(Entity e)
            : base(e)
        { }
        #endregion

        #region Properties
        public float TimeSpent
        {
            get { return timeSpent; }
            set { timeSpent = value; }
        }

        [Mandatory]
        public float RequiredTime
        {
            get { return requiredTime; }
            set { requiredTime = value; }
        }

        [Mandatory]
        public TrainProgressType ProgressType
        {
            get { return progressType; }
            set { progressType = value; }
        }
        #endregion

        #region Methods
        public void BeginProgress()
        {
            foreach (Component component in Entity.Components)
            {
                Type componentType = component.GetType();
                if (!essentialComponents.Contains(componentType))
                    hiddenComponents.Add(component);
            }

            foreach (Component hiddenComponent in hiddenComponents)
                Entity.Components.Remove(hiddenComponent);

            Debug.Assert(Entity.Components.Has<Health>(), "Entity doesn't have a Health component!");
            Entity.Components.Get<Health>().MaxHealth = 1;
        }

        public void SpendTime(float time)
        {
            timeSpent += time;

            Debug.Assert(Entity.Components.Has<Health>(), "Entity doesn't have a Health component!");
            Identity identity = Entity.Components.Get<Identity>();
            Health health = Entity.Components.Get<Health>();
            Health finalHealth = identity.TemplateEntity.Components.Get<Health>();

            if (timeSpent >= requiredTime)
            {
                foreach (Component hiddenComponent in hiddenComponents)
                    Entity.Components.Add(hiddenComponent);
                health.MaxHealth = finalHealth.MaxHealth;
                Entity.Components.Remove(this);
                return;
            }
            health.MaxHealth = (int)(finalHealth.MaxHealth * (timeSpent / requiredTime));
        }
        #endregion
    }
}

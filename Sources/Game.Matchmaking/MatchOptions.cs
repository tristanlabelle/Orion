using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Orion.Engine;

namespace Orion.Game.Matchmaking
{
    public class MatchOptions
    {
        #region Static
        #region Fields
#if DEBUG
        public static readonly Size SuggestedMinimumMapSize = new Size(0, 0);
#else
        public static readonly Size SuggestedMinimumMapSize = new Size(50, 50);
#endif
        public const int SuggestedMinimumPopulation = 100;
        public const int SuggestedMinimumAlagene = 0;
        public const int SuggestedMinimumAladdium = 0;
        #endregion
        #endregion

        #region Instance
        #region Fields
        private Size mapSize = new Size(150, 150);
        private MatchStartType startType = MatchStartType.Sedentary;
        private int maximumPopulation = 200;
        private bool revealTopology = false;
        private int initialAladdiumAmount = 200;
        private int initialAlageneAmount = 0;
        private int seed = 0;
        #endregion

        #region Events
        public event Action<MatchOptions> MapSizeChanged;
        public event Action<MatchOptions> StartTypeChanged;
        public event Action<MatchOptions> MaximumPopulationChanged;
        public event Action<MatchOptions> RevealTopologyChanged;
        public event Action<MatchOptions> InitialAladdiumAmountChanged;
        public event Action<MatchOptions> InitialAlageneAmountChanged;
        public event Action<MatchOptions> SeedChanged;
        public event Action<MatchOptions> Changed;
        #endregion

        #region Properties
        public Size MapSize
        {
            get { return mapSize; }
            set
            {
                mapSize = value;
                TriggerEvent(MapSizeChanged);
            }
        }

        public MatchStartType StartType
        {
            get { return startType; }
            set
            {
                startType = value;
                TriggerEvent(StartTypeChanged);
            }
        }

        public int Seed
        {
            get { return seed; }
            set
            {
                seed = value;
                TriggerEvent(SeedChanged);
            }
        }

        public int MaximumPopulation
        {
            get { return maximumPopulation; }
            set
            {
                maximumPopulation = value;
                TriggerEvent(MaximumPopulationChanged);
            }
        }

        public bool RevealTopology
        {
            get { return revealTopology; }
            set
            {
                revealTopology = value;
                TriggerEvent(RevealTopologyChanged);
            }
        }

        public int InitialAladdiumAmount
        {
            get { return initialAladdiumAmount; }
            set
            {
                initialAladdiumAmount = value;
                TriggerEvent(InitialAladdiumAmountChanged);
            }
        }

        public int InitialAlageneAmount
        {
            get { return initialAlageneAmount; }
            set
            {
                initialAlageneAmount = value;
                TriggerEvent(InitialAlageneAmountChanged);
            }
        }
        #endregion

        #region Methods
        private void TriggerEvent(Action<MatchOptions> eventHandler)
        {
            if (eventHandler != null) eventHandler(this);

            var genericHandler = Changed;
            if (genericHandler != null) genericHandler(this);
        }
        #endregion
        #endregion
    }
}

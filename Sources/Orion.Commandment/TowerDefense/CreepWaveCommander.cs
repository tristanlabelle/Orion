using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.GameLogic.Skills;

namespace Orion.Matchmaking.TowerDefense
{
    public sealed class CreepWaveCommander : Commander
    {
        #region Fields
        private static readonly string[] waveUnitTypeNames = new[]
        {
            "Mentos",
            "Coke Diète",
            "Schtroumpf",
            "Pirate",
            "Ninja",
            "Tapis Volant",
            "Jedihad",
            "Viking",
            "Flying Spaghetti Monster",
            "OVNI",
            "Jésus",
            "Jésus-Raptor",
            "Chuck Norris"
        };
        private static readonly int unitsPerWave = 20;

        private int nextWaveIndex;
        private float timeBeforeNextWave = 3;
        #endregion

        #region Constructors
        public CreepWaveCommander(Faction faction)
            : base(faction)
        { }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            timeBeforeNextWave -= timeDelta;
            if (timeBeforeNextWave < 0)
            {
                SendNextWave();
                timeBeforeNextWave = 120;
            }
        }

        public void SendNextWave()
        {
            Unit unit = Faction.Units
                .FirstOrDefault(u => u.HasSkill<BuildSkill>());
            if (unit == null) return;

            string unitTypeName = waveUnitTypeNames[nextWaveIndex];

            ++nextWaveIndex;
        }
        #endregion
    }
}

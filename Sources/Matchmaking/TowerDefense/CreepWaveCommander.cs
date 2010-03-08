using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.GameLogic.Tasks;

namespace Orion.Matchmaking.TowerDefense
{
    public sealed class CreepWaveCommander : Commander
    {
        #region Fields
        private static readonly string[] waveUnitTypeNames = new[]
        {
            "Mentos",
            "Coke diète",
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
        private static readonly float timeBetweenWaves = 5;

        private readonly CreepPath path;
        private int nextWaveIndex;
        private float timeBeforeNextWave = 3;
        #endregion

        #region Constructors
        public CreepWaveCommander(Faction faction, CreepPath path)
            : base(faction)
        {
            Argument.EnsureNotNull(path, "path");
            this.path = path;

            UnitType worker = World.UnitTypes.First(unit => unit.HasSkill<BuildSkill>());
            faction.CreateUnit(worker, Point.Zero);
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            timeBeforeNextWave -= timeDelta;
            if (timeBeforeNextWave < 0)
            {
                SendNextWave();
                timeBeforeNextWave = timeBetweenWaves;
            }
        }

        public void SendNextWave()
        {
            string unitTypeName = waveUnitTypeNames[nextWaveIndex];
            UnitType unitType = World.UnitTypes.FromName(unitTypeName);

            Point spawnPoint = path.Points[0];
            Unit creep = Faction.CreateUnit(unitType, spawnPoint);
            Point destinationPoint = path.Points[path.Points.Count - 1];
            creep.TaskQueue.OverrideWith(new CreepTask(creep, path));

            nextWaveIndex = (nextWaveIndex + 1) % waveUnitTypeNames.Length;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking.TowerDefense
{
    /// <summary>
    /// A tower defense commander which creates creeps and makes them follow a path.
    /// </summary>
    public sealed class CreepWaveCommander : Commander
    {
        #region Fields
        private static readonly string[] WaveCreepTypeNames = new[]
        {
            "Schtroumpf",
            "Pirate",
            "Ninja",
            "Jedihad",
            "Viking",
            "Tapis volant",
            "OVNI",
            "Jésus",
            "Jésus-raptor",
            "Mentos",
            "Coke diète",
            "Flying Spaghetti Monster",
        };
        private static readonly int CreepsPerWave = 10;
        private static readonly float TimeBetweenWaves = 10;
        private static readonly float TimeBetweenCreeps = 1;

        private readonly CreepPath path;
        private int waveIndex;
        private int spawnedCreepCount;
        private float timeBeforeNextCreep = TimeBetweenWaves;
        #endregion

        #region Constructors
        public CreepWaveCommander(Match match, Faction faction, CreepPath path)
            : base(match, faction)
        {
            Argument.EnsureNotNull(path, "path");

            this.path = path;
        }
        #endregion

        #region Methods
        public override void Update(float timeDeltaInSeconds)
        {
            timeBeforeNextCreep -= timeDeltaInSeconds;
            if (timeBeforeNextCreep > 0) return;

            if (!TrySpawnCreep()) return;

            timeBeforeNextCreep = TimeBetweenCreeps;
            ++spawnedCreepCount;

            if (spawnedCreepCount < CreepsPerWave) return;

            waveIndex = (waveIndex + 1) % WaveCreepTypeNames.Length;
            spawnedCreepCount = 0;
            timeBeforeNextCreep = TimeBetweenWaves;
        }

        private bool TrySpawnCreep()
        {
            string creepTypeName = WaveCreepTypeNames[waveIndex];
            UnitType creepType = Match.UnitTypes.FromName(creepTypeName);

            Point spawnPoint = path.Points[0];
            Region spawnRegion = new Region(spawnPoint, creepType.Size);
            if (!World.IsFree(spawnRegion, creepType.CollisionLayer))
                return false;

            Unit creep = Faction.CreateUnit(creepType, spawnPoint);
            creep.TaskQueue.OverrideWith(new CreepTask(creep, path));
            return true;
        }
        #endregion
    }
}

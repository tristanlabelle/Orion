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
        private static readonly float TimeBetweenWaves = 15;
        private static readonly float TimeBetweenCreeps = 0.75f;

        private readonly CreepPath path;
        private int waveIndex = -1;
        private int spawnedCreepCount = CreepsPerWave;
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

        #region Events
        /// <summary>
        /// Raised when a creep successfully escapes.
        /// </summary>
        public event Action<CreepWaveCommander> CreepLeaked;
        #endregion

        #region Properties
        public CreepPath Path
        {
            get { return path; }
        }

        public int WaveIndex
        {
            get { return waveIndex; }
        }

        public bool IsBetweenWaves
        {
            get { return Faction.Units.Count() == 0; }
        }

        public float TimeBeforeNextWave
        {
            get { return timeBeforeNextCreep; }
        }
        #endregion

        #region Methods
        internal void RaiseCreepLeaked()
        {
            CreepLeaked.Raise(this);
        }

        public override void Update(SimulationStep step)
        {
            if (spawnedCreepCount == CreepsPerWave && Faction.Units.Count() > 0)
                return;

            timeBeforeNextCreep -= step.TimeDeltaInSeconds;
            if (timeBeforeNextCreep > 0) return;
            if (spawnedCreepCount == CreepsPerWave)
            {
                spawnedCreepCount = 0;
                ++waveIndex;
            }

            if (!TrySpawnCreep()) return;

            timeBeforeNextCreep = TimeBetweenCreeps;
            ++spawnedCreepCount;

            if (spawnedCreepCount < CreepsPerWave) return;

            timeBeforeNextCreep = TimeBetweenWaves;
        }

        private bool TrySpawnCreep()
        {
            string creepTypeName = WaveCreepTypeNames[waveIndex % WaveCreepTypeNames.Length];
            UnitType creepType = Match.UnitTypes.FromName(creepTypeName);

            Point spawnPoint = path.Points[0];
            Region spawnRegion = new Region(spawnPoint, creepType.Size);
            if (!World.IsFree(spawnRegion, creepType.CollisionLayer))
                return false;

            Unit creep = Faction.CreateUnit(creepType, spawnPoint);
            creep.TaskQueue.OverrideWith(new CreepTask(creep, this));
            return true;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Engine;
using System.IO;
using Orion.Engine.Collections;

namespace Orion.Game.Matchmaking.TowerDefense
{
    public sealed class TypingCreepCommander : Commander
    {
        #region Fields
        private readonly List<string> phrases = new List<string>();
        private readonly Dictionary<Unit, CreepPhrase> creepPhrases = new Dictionary<Unit, CreepPhrase>();
        private readonly CreepPath creepPath;
        private int creepsSpawned;
        private float timeSinceLastCreep;
        #endregion

        #region Constructors
        public TypingCreepCommander(Match match, Faction faction, CreepPath path)
            : base(match, faction)
        {
            this.phrases = File.ReadAllLines("../../../Assets/Phrases.txt")
                .OrderBy(p => p.Length)
                .ThenBy(p => p.ToLowerInvariant())
                .ToList();
            this.creepPath = path;

            World.EntityRemoved += OnEntityRemoved;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a creep successfully escapes.
        /// </summary>
        public event Action<TypingCreepCommander> CreepLeaked;
        #endregion

        #region Properties
        public CreepPath Path
        {
            get { return creepPath; }
        }
        #endregion

        #region Methods
        public override void Update(SimulationStep step)
        {
            timeSinceLastCreep += step.TimeDeltaInSeconds;
            if (timeSinceLastCreep < (3 - creepsSpawned * 0.03f)) return;

            timeSinceLastCreep = 0;

            int longestPhraseLength = phrases.Last().Length;
            int minPhraseLength = Math.Min(2 + creepsSpawned / 3 + Match.Random.Next(3), longestPhraseLength);
            int phraseIndex = Enumerable.Range(0, phrases.Count)
                .Where(i => phrases[i].Length >= minPhraseLength)
                .WithMinOrDefault(i => phrases[i].Length, -1);
            if (phraseIndex == -1) phraseIndex = phrases.Count - 1;

            string phrase = phrases[phraseIndex];
            if (phrases.Count > 1) phrases.RemoveAt(phraseIndex);

            float progress = (float)minPhraseLength / longestPhraseLength;
            int creepTypeIndex = Math.Min((int)(progress * CreepWaveCommander.WaveCreepTypeNames.Length), CreepWaveCommander.WaveCreepTypeNames.Length - 1);
            string creepTypeName = CreepWaveCommander.WaveCreepTypeNames[creepTypeIndex];
            Unit creepType = Match.UnitTypes.FromName(creepTypeName);
            Unit creep = Faction.CreateUnit(creepType, creepPath.Points[0]);
            var task = new CreepTask(creep, creepPath);
            task.Leaked += OnCreepLeaked;
            creep.TaskQueue.OverrideWith(task);

            creepPhrases.Add(creep, new CreepPhrase(phrase));

            ++creepsSpawned;
        }

        public CreepPhrase GetCreepPhrase(Unit creep)
        {
            return creepPhrases[creep];
        }

        private void OnEntityRemoved(World arg1, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit != null) creepPhrases.Remove(unit);
        }

        private void OnCreepLeaked(CreepTask obj)
        {
            CreepLeaked.Raise(this);
        }
        #endregion
    }
}

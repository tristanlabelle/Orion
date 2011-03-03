using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Utilities;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Audio
{
    /// <summary>
    /// Provides an audible representation of game logic events.
    /// </summary>
    public sealed class MatchAudioPresenter : IDisposable
    {
        #region Fields
        private readonly GameAudio audio;
        private readonly UserInputManager userInputManager;

        /// <summary>
        /// Stores the entities which are last known to be selected.
        /// Used to determine if entities were added to the selection when it changes.
        /// </summary>
        private readonly HashSet<Entity> previousSelection = new HashSet<Entity>();

        private readonly Action<Unit> buildingConstructionCompletedEventHandler;

        private bool isGameStarted;
        private bool hasExplosionOccuredInFrame;
        #endregion

        #region Constructors
        public MatchAudioPresenter(GameAudio audio, UserInputManager userInputManager)
        {
            Argument.EnsureNotNull(audio, "audio");
            Argument.EnsureNotNull(userInputManager, "userInputManager");

            this.audio = audio;
            this.userInputManager = userInputManager;
            this.buildingConstructionCompletedEventHandler = OnBuildingConstructionCompleted;

            this.userInputManager.UnderAttackMonitor.Warning += OnUnderAttackWarning;
            this.World.EntityAdded += OnEntityAdded;
            this.World.EntityRemoved += OnEntityRemoved;
            this.World.EntityDied += OnEntityDied;
            this.World.UnitHitting += OnUnitHitting;
            this.World.Updated += OnWorldUpdated;
            this.World.ExplosionOccured += OnExplosionOccured;
            this.userInputManager.Selection.Changed += OnSelectionChanged;
            this.userInputManager.LocalCommander.CommandIssued += OnCommandIssued;
        }
        #endregion

        #region Properties
        private SelectionManager SelectionManager
        {
            get { return userInputManager.SelectionManager; }
        }

        private Selection Selection
        {
            get { return userInputManager.Selection; }
        }

        private Faction LocalFaction
        {
            get { return userInputManager.LocalFaction; }
        }

        private Match Match
        {
            get { return userInputManager.Match; }
        }

        private World World
        {
            get { return LocalFaction.World; }
        }
        #endregion

        #region Methods
        public void SetViewBounds(Rectangle viewBounds)
        {
            audio.ListenerPosition = new Vector3(viewBounds.CenterX, viewBounds.CenterY, viewBounds.Width / 100.0f);
        }

        public void PlayDefeatSound()
        {
            audio.PlayUISound("Defeat");
        }

        public void PlayVictorySound()
        {
            audio.PlayUISound("Victory");
        }

        public void Dispose()
        {
        }

        private void OnEntityAdded(World sender, Entity entity)
        {
            if (!isGameStarted) return;

            Identity identity = entity.Identity;
            if (identity.Name == "Chuck Norris")
            {
                audio.PlayUISound("Chuck Norris.Spawn");
                return;
            }

            Faction faction = FactionMembership.GetFaction(entity);
            if (faction != LocalFaction) return;

            Unit unit = entity as Unit;
            if (unit == null) return;

            if (unit.IsBuilding && unit.Components.Has<BuildProgress>())
            {
                unit.ConstructionCompleted += OnBuildingConstructionCompleted;
                audio.PlaySfx("UnderConstruction", unit.Center);
                return;
            }

            string soundName = audio.GetUnitSoundName(entity, "Select");
            Spatial spatial = entity.Spatial;
            audio.PlaySfx(soundName, spatial.Center);
        }

        private void OnEntityDied(World world, Entity entity)
        {
            Spatial spatial = entity.Spatial;
            if (spatial != null)
            {
                string soundName = audio.GetUnitSoundName(entity, "Die");
                audio.PlaySfx(soundName, spatial.Center);
            }
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Unit unit = entity as Unit;
            Faction faction = FactionMembership.GetFaction(entity);
            if (unit != null && faction == LocalFaction && unit.Components.Has<BuildProgress>())
                unit.ConstructionCompleted -= buildingConstructionCompletedEventHandler;
        }

        private void OnBuildingConstructionCompleted(Unit building)
        {
            building.ConstructionCompleted -= buildingConstructionCompletedEventHandler;

            string soundName = audio.GetUnitSoundName(building, "Select");
            audio.PlaySfx(soundName, building.Center);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            hasExplosionOccuredInFrame = false;

            if (!isGameStarted && step.TimeInSeconds > 0.5f)
            {
                isGameStarted = true;

                if (userInputManager.LocalFaction.Color == Colors.Magenta)
                    audio.PlayUISound("Tapette");
            }
        }

        private void OnSelectionChanged(Selection sender)
        {
            if (sender.Type != SelectionType.Units)
            {
                previousSelection.Clear();
                return;
            }

            // Find the most frequent unit type in the newly selected units.
#warning Unit type comparision
            var unitTypeGroup = sender.Except(previousSelection)
                .Cast<Unit>()
                .GroupBy(unit => unit.Type)
                .WithMaxOrDefault(group => group.Count());

            Unit unitType = unitTypeGroup == null ? null : unitTypeGroup.Key;

            previousSelection.Clear();
            previousSelection.UnionWith(sender);

            if (unitType == null) return;

            string soundName = audio.GetUnitSoundName(unitType, "Select");
            audio.PlayUISound(soundName);
        }

        private void OnCommandIssued(Commander sender, Command args)
        {
            Debug.Assert(args != null);

            Unit unitType = userInputManager.SelectionManager.FocusedUnitType;
            if (unitType == null) return;

            string commandName = args.GetType().Name.Replace("Command", string.Empty);
            string soundName = audio.GetUnitSoundName(unitType, commandName);
            audio.PlayUISound(soundName);
        }

        private void OnUnitHitting(World sender, HitEventArgs args)
        {
            bool isVisible = LocalFaction.CanSee(args.Hitter) || LocalFaction.CanSee(args.Target);
            if (!isVisible) return;

            bool isMelee = args.Hitter.Components.Get<Attacker>().IsMelee;
            string soundName = isMelee ? "MeleeAttack" : "RangeAttack";

            audio.PlaySfx(soundName, args.Hitter.Center);
        }

        private void OnUnderAttackWarning(UnderAttackMonitor sender, Vector2 position)
        {
            bool isNearBase = World.Entities
                .Intersecting(new Circle(position, 6))
                .OfType<Unit>()
                .Any(unit => unit.IsBuilding && FactionMembership.GetFaction(unit) == LocalFaction);

            string soundName = isNearBase ? "UnderAttackBase" : "UnderAttackUnit";
            audio.PlayUISound(soundName);
        }

        private void OnExplosionOccured(World sender, Circle args)
        {
            if (hasExplosionOccuredInFrame) return;

            audio.PlaySfx("Explosion", null);
            hasExplosionOccuredInFrame = true;
        }
        #endregion
    }
}

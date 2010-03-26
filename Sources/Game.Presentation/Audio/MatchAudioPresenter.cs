using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Utilities;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands;

namespace Orion.Game.Presentation.Audio
{
    /// <summary>
    /// Provides an audible representation of game logic events.
    /// </summary>
    public sealed class MatchAudioPresenter : IDisposable
    {
        #region Fields
        private readonly GameAudio gameAudio;
        private readonly Match match;
        private readonly UserInputManager userInputManager;

        /// <summary>
        /// Stores the entities which are last known to be selected.
        /// Used to determine if entities were added to the selection when it changes.
        /// </summary>
        private readonly HashSet<Entity> previousSelection = new HashSet<Entity>();

        private bool isGameStarted;
        private bool hasExplosionOccuredInFrame;
        #endregion

        #region Constructors
        public MatchAudioPresenter(GameAudio gameAudio, Match match, UserInputManager userInputManager)
        {
            Argument.EnsureNotNull(gameAudio, "gameAudio");
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(userInputManager, "userInputManager");

            this.gameAudio = gameAudio;
            this.match = match;
            this.userInputManager = userInputManager;

            this.userInputManager.UnderAttackMonitor.Warning += OnUnderAttackWarning;

            this.match.World.Entities.Added += OnEntityAdded;
            this.match.World.UnitHitting += OnUnitHitting;
            this.match.World.Updated += OnWorldUpdated;
            this.match.World.ExplosionOccured += OnExplosionOccured;
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

        private World World
        {
            get { return LocalFaction.World; }
        }
        #endregion

        #region Methods
        public void SetViewBounds(Rectangle viewBounds)
        {
            gameAudio.ListenerPosition = new Vector3(viewBounds.CenterX, viewBounds.CenterY, viewBounds.Width / 100.0f);
        }

        public void Dispose()
        {
        }

        private void OnEntityAdded(EntityManager arg1, Entity entity)
        {
            if (!isGameStarted) return;

            Unit unit = entity as Unit;
            if (unit == null) return;
            
            if (unit.Type.Name == "Chuck Norris")
            {
                gameAudio.PlayUISound("Chuck Norris.Spawn");
                return;
            }

            if (unit.Faction != LocalFaction) return;

            string soundName = gameAudio.GetUnitSoundName(unit.Type, "Select");
            gameAudio.PlaySfx(soundName, unit.Position);
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            hasExplosionOccuredInFrame = false;

            if (!isGameStarted && step.TimeInSeconds > 0.5f)
            {
                isGameStarted = true;

                if (userInputManager.LocalFaction.Color == Colors.Magenta)
                    gameAudio.PlayUISound("Tapette");
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
            var unitTypeGroup = sender.Except(previousSelection)
                .Cast<Unit>()
                .GroupBy(unit => unit.Type)
                .WithMaxOrDefault(group => group.Count());

            UnitType unitType = unitTypeGroup == null ? null : unitTypeGroup.Key;

            previousSelection.Clear();
            previousSelection.UnionWith(sender);

            if (unitType == null) return;

            string soundName = gameAudio.GetUnitSoundName(unitType, "Select");
            gameAudio.PlayUISound(soundName);
        }

        private void OnCommandIssued(Commander sender, Command args)
        {
            Debug.Assert(args != null);

            UnitType unitType = userInputManager.SelectionManager.FocusedUnitType;
            if (unitType == null) return;

            string commandName = args.GetType().Name.Replace("Command", string.Empty);
            string soundName = gameAudio.GetUnitSoundName(unitType, commandName);
            gameAudio.PlayUISound(soundName);
        }

        private void OnUnitHitting(World sender, HitEventArgs args)
        {
            bool isVisible = LocalFaction.CanSee(args.Hitter) || LocalFaction.CanSee(args.Target);
            if (!isVisible) return;

            bool isMelee = args.Hitter.GetStat(UnitStat.AttackRange) == 0;
            string soundName = isMelee ? "MeleeAttack" : "RangeAttack";

            gameAudio.PlaySfx(soundName, args.Hitter.Center);
        }

        private void OnUnderAttackWarning(UnderAttackMonitor sender, Vector2 position)
        {
            bool isNearBase = World.Entities
                .Intersecting(new Circle(position, 6))
                .OfType<Unit>()
                .Any(unit => unit.IsBuilding && unit.Faction == LocalFaction);

            string soundName = isNearBase ? "UnderAttackBase" : "UnderAttackUnit";
            gameAudio.PlayUISound(soundName);
        }

        private void OnExplosionOccured(World sender, Circle args)
        {
            if (hasExplosionOccuredInFrame) return;

            gameAudio.PlaySfx("Explosion", null);
            hasExplosionOccuredInFrame = true;
        }
        #endregion
    }
}

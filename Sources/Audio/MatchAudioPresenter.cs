using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Collections;
using Orion.Geometry;
using Orion.GameLogic;
using Orion.GameLogic.Utilities;
using Orion.Matchmaking;
using Orion.Matchmaking.Commands;

namespace Orion.Audio
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

        private bool isGameStarted;
        private bool hasExplosionOccuredInFrame;
        private bool hasSelectionChangedInFrame;
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
            this.userInputManager.SelectionManager.SelectionChanged += OnSelectionChanged;
            this.userInputManager.LocalCommander.CommandGenerated += OnCommandGenerated;
        }
        #endregion

        #region Properties
        private SelectionManager SelectionManager
        {
            get { return userInputManager.SelectionManager; }
        }

        private IEnumerable<Unit> SelectedUnits
        {
            get { return SelectionManager.SelectedUnits; }
        }

        private Faction LocalFaction
        {
            get { return userInputManager.LocalFaction; }
        }

        private World World
        {
            get { return LocalFaction.World; }
        }

        private UnitType SelectedUnitType
        {
            get
            {
                return SelectedUnits
                    .Select(unit => unit.Type)
                    .Distinct()
                    .WithMaxOrDefault(type => SelectedUnits.Count(unit => unit.Type == type));
            }
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
            if (unit == null || unit.Faction != LocalFaction) return;

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

            if (hasSelectionChangedInFrame)
            {
                hasSelectionChangedInFrame = false;

                if (SelectedUnitType == null) return;

                string soundName = gameAudio.GetUnitSoundName(SelectedUnitType, "Select");
                gameAudio.PlayUISound(soundName);
            }
        }

        private void OnSelectionChanged(SelectionManager sender)
        {
            Debug.Assert(sender == SelectionManager);

            hasSelectionChangedInFrame = true;
        }

        private void OnCommandGenerated(Commander sender, Command args)
        {
            Debug.Assert(sender == userInputManager.LocalCommander);
            Debug.Assert(args != null);

            UnitType unitType = SelectedUnitType;
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine.Audio;
using Orion.Geometry;
using Orion.GameLogic;
using Orion.Matchmaking;
using Orion.Matchmaking.Commands;

namespace Orion.Audio
{
    public sealed class MatchAudioPresenter : IDisposable
    {
        #region Fields
        private readonly SoundContext soundContext;
        private readonly Match match;
        private readonly UserInputManager userInputManager;
        private readonly SoundSource voicesSoundSource;
        private readonly AttackMonitor attackMonitor;

        /// <summary>
        /// Reused between calls to minimize object garbage.
        /// </summary>
        private readonly StringBuilder stringBuilder = new StringBuilder();
        #endregion

        #region Constructors
        public MatchAudioPresenter(SoundContext audioContext, Match match, UserInputManager userInputManager)
        {
            Argument.EnsureNotNull(audioContext, "audioContext");
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(userInputManager, "userInputManager");

            this.match = match;
            this.soundContext = audioContext;
            this.userInputManager = userInputManager;

            this.voicesSoundSource = audioContext.CreateSource();
            this.voicesSoundSource.Volume = 0.8f;

            this.attackMonitor = new AttackMonitor(userInputManager.LocalCommander.Faction);
            this.attackMonitor.Warning += OnAttackWarning;

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
            soundContext.ListenerPosition = new Vector3(viewBounds.CenterX, viewBounds.CenterY, viewBounds.Width / 50.0f);
        }
        
        public void Dispose()
        {
            voicesSoundSource.Dispose();
        }

        private void PlayVoice(string name)
        {
            Sound sound = soundContext.GetRandomSoundFromGroup(name);
            if (sound == null) return;

            voicesSoundSource.Play(sound);
        }

        private void PlayUnitSound(UnitType unitType, string name)
        {
            stringBuilder.Clear();
            stringBuilder.Append(unitType.Name);
            stringBuilder.Append('.');
            stringBuilder.Append(name);
            PlayVoice(stringBuilder.ToString());
        }

        private void OnEntityAdded(EntityManager arg1, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null || unit.Faction != LocalFaction) return;

            PlayUnitSound(unit.Type, "Select");
        }

        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            if (step.Number == 5 && userInputManager.LocalFaction.Color == Colors.Pink)
            {
                Sound sound = soundContext.GetRandomSoundFromGroup("Tapette");
                if (sound == null) return;

                soundContext.PlayAndForget(sound, null);
            }
        }

        private void OnSelectionChanged(SelectionManager sender)
        {
            Debug.Assert(sender == SelectionManager);

            UnitType unitType = SelectedUnitType;
            if (unitType == null) return;

            PlayUnitSound(unitType, "Select");
        }

        private void OnCommandGenerated(Commander sender, Command args)
        {
            Debug.Assert(sender == userInputManager.LocalCommander);
            Debug.Assert(args != null);

            UnitType unitType = SelectedUnitType;
            if (unitType == null) return;

            string commandName = args.GetType().Name.Replace("Command", "");
            PlayUnitSound(unitType, commandName);
        }

        private void OnUnitHitting(World sender, HitEventArgs args)
        {
            bool isVisible = LocalFaction.CanSee(args.Hitter) || LocalFaction.CanSee(args.Target);
            if (!isVisible) return;

            bool isMelee = args.Hitter.GetStat(UnitStat.AttackRange) == 0;
            string soundGroup = isMelee ? "MeleeAttack" : "RangeAttack";
            Sound sound = soundContext.GetRandomSoundFromGroup(soundGroup);
            if (sound == null) return;

            soundContext.PlayAndForget(sound, args.Hitter.Center);
        }

        private void OnAttackWarning(AttackMonitor sender, Vector2 position)
        {
            bool isNearBase = World.Entities
                .Intersecting(new Circle(position, 6))
                .OfType<Unit>()
                .Any(unit => unit.IsBuilding && unit.Faction == LocalFaction);

            string soundGroup = isNearBase ? "UnderAttackBase" : "UnderAttackUnit";
            Sound sound = soundContext.GetRandomSoundFromGroup(soundGroup);
            if (sound == null) return;

            soundContext.PlayAndForget(sound, null);
        }

        private void OnExplosionOccured(World sender, Circle args)
        {
            Sound sound = soundContext.GetRandomSoundFromGroup("Explosion");
            if (sound == null) return;

            soundContext.PlayAndForget(sound, null);
        }
        #endregion
    }
}

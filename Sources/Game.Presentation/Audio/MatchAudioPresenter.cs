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

        private readonly HashCountedSet<string> tempTemplateCounts = new HashCountedSet<string>();

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

            this.userInputManager.UnderAttackMonitor.Warning += OnUnderAttackWarning;
            this.World.EntityAdded += OnEntityAdded;
            this.World.EntityDied += OnEntityDied;
            this.World.HitOccured += OnUnitHitting;
            this.World.BuildingConstructed += OnBuildingConstructed;
            this.userInputManager.Selection.Changed += OnSelectionChanged;
            this.World.Updated += OnWorldUpdated;
            this.World.ExplosionOccured += OnExplosionOccured;
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

            if (entity.Components.Has<BuildProgress>())
            {
                audio.PlaySfx("UnderConstruction", entity.Center);
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

        private string GetLeadingTemplate(IEnumerable<Entity> entities)
        {
            // Find the most frequent entity prototype in the newly selected entities.
            tempTemplateCounts.Clear();
            foreach (Entity entity in entities)
            {
                if (previousSelection.Contains(entity)) continue;

                Identity identity = entity.Identity;
                if (identity == null || identity.SoundIdentity == null) continue;

                tempTemplateCounts.Add(identity.SoundIdentity);
            }
            if (tempTemplateCounts.Count == 0) return null;

            var maxEntry = tempTemplateCounts.Entries
                .WithMaxOrDefault(entry => entry.Count);

            return maxEntry.Item;
        }

        private void OnSelectionChanged(Selection sender)
        {
            string template = GetLeadingTemplate(sender);

            previousSelection.Clear();
            previousSelection.UnionWith(sender);

            if (template == null) return;

            audio.PlayUISound(template + ".Select");
        }

        private void OnCommandIssued(Commander sender, Command args)
        {
            Debug.Assert(args != null);

            Entity prototype = userInputManager.SelectionManager.FocusedPrototype;
            if (prototype == null) return;

            string commandName = args.GetType().Name.Replace("Command", string.Empty);
            string soundName = audio.GetUnitSoundName(prototype, commandName);
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

        private void OnBuildingConstructed(World world, Entity building)
        {
            if (FactionMembership.GetFaction(building) != LocalFaction) return;

            string soundName = audio.GetUnitSoundName(building, "Select");
            audio.PlaySfx(soundName, building.Center);
        }

        private void OnUnderAttackWarning(UnderAttackMonitor sender, Vector2 position)
        {
            bool isNearBase = World.Entities
                .Intersecting(new Circle(position, 6))
                .Any(entity => entity.Identity.IsBuilding && FactionMembership.GetFaction(entity) == LocalFaction);

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

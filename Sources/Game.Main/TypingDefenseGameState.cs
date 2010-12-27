using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Audio;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation.Gui;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Presentation.Renderers;
using Orion.Engine.Gui;
using System.Diagnostics;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Main
{
    public sealed class TypingDefenseGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly GameAudio audio;
        private readonly CreepPath creepPath;
        private readonly Match match;
        private readonly SlaveCommander localCommander;
        private readonly TypingCreepCommander creepCommander;
        private readonly CommandPipeline commandPipeline;
        private readonly MatchUI ui;
        private readonly MatchAudioPresenter audioPresenter;
        private SimulationStep lastSimulationStep;
        private Unit focusedCreep;
        private int lifeCount = 10;
        #endregion

        #region Constructors
        public TypingDefenseGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.audio = new GameAudio();

            Random random = new MersenneTwister(Environment.TickCount);
            Terrain terrain = Terrain.CreateFullyWalkable(new Size(60, 40));
            World world = new World(terrain, random, 200);
            creepPath = CreepPath.Generate(world.Size, random);
            match = new Match(world, random, p => !creepPath.Contains(p));

            Faction localFaction = world.CreateSpectatorFaction();
            localCommander = new SlaveCommander(match, localFaction);

            Faction creepFaction = world.CreateFaction("Creeps", Colors.Cyan);
            creepCommander = new TypingCreepCommander(match, creepFaction, creepPath);
            creepCommander.CreepLeaked += OnCreepLeaked;

            commandPipeline = new CommandPipeline(match);
            commandPipeline.AddCommander(localCommander);
            commandPipeline.AddCommander(creepCommander);

            UserInputManager userInputManager = new UserInputManager(match, localCommander);
            var matchRenderer = new TypingDefenseMatchRenderer(userInputManager, graphics, creepCommander);

            ui = new MatchUI(graphics, userInputManager, matchRenderer);
            ui.SetResourcesLabelTextGetter(GetResourcesLabelText);
            audioPresenter = new MatchAudioPresenter(audio, userInputManager);

            ui.CharacterTyped += OnCharacterTyped;
            world.EntityRemoved += OnEntityRemoved;
            ui.QuitPressed += OnQuitPressed;
        }
        #endregion

        #region Properties
        public RootView RootView
        {
            get { return graphics.RootView; }
        }

        private Faction LocalFaction
        {
            get { return localCommander.Faction; }
        }

        private Faction CreepFaction
        {
            get { return creepCommander.Faction; }
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            RootView.Children.Add(ui);
        }

        protected internal override void OnShadowed()
        {
            RootView.Children.Remove(ui);
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            if (match.IsRunning)
            {
                SimulationStep step = new SimulationStep(
                    lastSimulationStep.Number + 1,
                    lastSimulationStep.TimeInSeconds + timeDeltaInSeconds,
                    timeDeltaInSeconds);

                match.World.Update(step);

                lastSimulationStep = step;
            }

            commandPipeline.Update(lastSimulationStep);

            graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            RootView.Draw(graphics.Context);
        }

        public override void Dispose()
        {
            audioPresenter.Dispose();
            ui.Dispose();
            audio.Dispose();
            commandPipeline.Dispose();
        }

        private void OnCreepLeaked(TypingCreepCommander obj)
        {
            if (lifeCount == 0) return;

            --lifeCount;
            if (lifeCount == 0) ui.DisplayDefeatMessage(() => Manager.Pop());
        }

        private string GetResourcesLabelText()
        {
            return "Aladdium: {0}    Vies: {1}"
                .FormatInvariant(LocalFaction.AladdiumAmount, lifeCount);
        }

        private void OnQuitPressed(MatchUI sender)
        {
            Manager.PopTo<MainMenuGameState>();
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Unit unit = entity as Unit;
            Debug.Assert(unit != null);

            if (unit.Faction != creepCommander.Faction)
            {
                if (unit == focusedCreep) focusedCreep = null;
                bool isKilledCreep = !unit.GridRegion.Contains(creepPath.Points[creepPath.Points.Count - 1]);
                if (!isKilledCreep) return;

                LocalFaction.AladdiumAmount += unit.GetStat(BasicSkill.AladdiumCostStat);
            }
        }

        private void OnCharacterTyped(Responder sender, char character)
        {
            if (focusedCreep == null)
            {
                focusedCreep = creepCommander.Faction.Units
                    .FirstOrDefault(c => creepCommander.GetCreepPhrase(c).NextCharacter == character);
            }

            if (focusedCreep != null)
            {
                CreepPhrase phrase = creepCommander.GetCreepPhrase(focusedCreep);
                phrase.Focus();
                phrase.Type(character);
                if (phrase.IsComplete)
                {
                    focusedCreep.Suicide();
                    focusedCreep = null;
                }
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Presentation.Audio;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialisation, updating and clean up of the state of the game when
    /// a tower defense game is being played.
    /// </summary>
    public sealed class TowerDefenseGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly GameAudio audio;
        private readonly CreepPath creepPath;
        private readonly Match match;
        private readonly SlaveCommander localCommander;
        private readonly CreepWaveCommander creepCommander;
        private readonly CommandPipeline commandPipeline;
        private readonly MatchUI ui;
        private readonly MatchAudioPresenter audioPresenter;
        private SimulationStep lastSimulationStep;
        private int lifeCount = 10;
        #endregion

        #region Constructors
        public TowerDefenseGameState(GameStateManager manager, GameGraphics graphics)
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

            Faction localFaction = world.CreateFaction("Player", Colors.Red);
            localFaction.AladdiumAmount = 300;
            localFaction.LocalFogOfWar.Disable();
            localFaction.CreateUnit(match.UnitTypes.FromName("Créateur"), new Point(world.Width / 2, world.Height / 2));
            localCommander = new SlaveCommander(match, localFaction);

            Faction creepFaction = world.CreateFaction("Creeps", Colors.Cyan);
            creepCommander = new CreepWaveCommander(match, creepFaction, creepPath);
            creepCommander.CreepLeaked += OnCreepLeaked;

            commandPipeline = new CommandPipeline(match);
            commandPipeline.AddCommander(localCommander);
            commandPipeline.AddCommander(creepCommander);

            UserInputManager userInputManager = new UserInputManager(match, localCommander);
            var matchRenderer = new TowerDefenseMatchRenderer(userInputManager, graphics, creepPath);

            ui = new MatchUI(graphics, userInputManager, matchRenderer);
            ui.SetResourcesLabelTextGetter(GetResourcesLabelText);
            audioPresenter = new MatchAudioPresenter(audio, userInputManager);

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

            graphics.UpdateRootView(timeDeltaInSeconds);
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

        private void OnCreepLeaked(CreepWaveCommander obj)
        {
            if (lifeCount == 0) return;

            --lifeCount;
            if (lifeCount == 0) ui.DisplayDefeatMessage(() => Manager.Pop());
        }

        private string GetResourcesLabelText()
        {
            string text = "Aladdium: {0}    Vies: {1}    Vague: {2}"
                .FormatInvariant(LocalFaction.AladdiumAmount, lifeCount, creepCommander.WaveIndex + 1);

            if (creepCommander.IsBetweenWaves)
            {
                int secondsBeforeNextWave = (int)Math.Ceiling(creepCommander.TimeBeforeNextWave);
                text += "    Prochaine vague dans {0} {1}"
                    .FormatInvariant(secondsBeforeNextWave, secondsBeforeNextWave > 1 ? "secondes" : "seconde");
            }

            return text;
        }

        private void OnQuitPressed(MatchUI sender)
        {
            Manager.PopTo<MainMenuGameState>();
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Unit unit = entity as Unit;
            Debug.Assert(unit != null);

            if (unit.Faction == LocalFaction)
            {
                if (unit.Type.Name == "Créateur")
                {
                    LocalFaction.MarkAsDefeated();
                    ui.DisplayDefeatMessage(() => Manager.PopTo<MainMenuGameState>());
                }
            }
            else
            {
                bool isKilledCreep = !unit.GridRegion.Contains(creepPath.Points[creepPath.Points.Count - 1]);
                if (!isKilledCreep) return;

                LocalFaction.AladdiumAmount += unit.GetStat(BasicSkill.AladdiumCostStat);
            }
        }
        #endregion
    }
}

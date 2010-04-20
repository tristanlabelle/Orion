using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;
using Orion.Engine.Gui;
using System.Diagnostics;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Game.Simulation.Skills;
using Orion.Game.Presentation.Renderers;

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
        private readonly CreepPath creepPath;
        private readonly Match match;
        private readonly SlaveCommander localCommander;
        private readonly CreepWaveCommander creepCommander;
        private readonly CommandPipeline commandPipeline;
        private readonly MatchUI ui;
        private SimulationStep lastSimulationStep;
        #endregion

        #region Constructors
        public TowerDefenseGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;

            Random random = new MersenneTwister(Environment.TickCount);
            Terrain terrain = Terrain.CreateFullyWalkable(new Size(60, 40));
            World world = new World(terrain, random, 200);
            match = new Match(world, random);
            creepPath = CreepPath.Generate(world.Size, random);

            Faction localFaction = world.CreateFaction("Player", Colors.Red);
            localFaction.AladdiumAmount = 200;
            localFaction.LocalFogOfWar.Disable();
            localFaction.CreateUnit(match.UnitTypes.FromName("Métaschtroumpf"), new Point(world.Width / 2, world.Height / 2));
            localCommander = new SlaveCommander(match, localFaction);

            Faction creepFaction = world.CreateFaction("Creeps", Colors.Cyan);
            creepCommander = new CreepWaveCommander(match, creepFaction, creepPath);

            commandPipeline = new CommandPipeline(match);
            commandPipeline.AddCommander(localCommander);
            commandPipeline.AddCommander(creepCommander);

            UserInputManager userInputManager = new UserInputManager(match, localCommander);
            var matchRenderer = new TowerDefenseMatchRenderer(userInputManager, graphics, creepPath);

            ui = new MatchUI(graphics, userInputManager, matchRenderer);

            world.Entities.Removed += OnEntityRemoved;
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
            ui.Dispose();
            commandPipeline.Dispose();
        }

        private void OnQuitPressed(MatchUI sender)
        {
            Manager.PopTo<MainMenuGameState>();
        }

        private void OnEntityRemoved(EntityManager sender, Entity entity)
        {
            Unit unit = entity as Unit;
            Debug.Assert(unit != null);

            if (unit.Faction == LocalFaction)
            {
                if (unit.Type.Name == "Métaschtroumpf")
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

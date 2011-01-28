using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the menu which lists the available replays.
    /// </summary>
    public sealed class ReplayBrowserGameState : GameState
    {
        #region Fields
        private static readonly string replayFolderName = "Replays";
        private static readonly string replayExtension = "replay";
        
        private readonly GameGraphics graphics;
        private readonly ReplayBrowser2 ui;
        #endregion

        #region Constructors
        public ReplayBrowserGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.ui = new ReplayBrowser2(graphics);

            this.ui.Exited += OnExited;
            this.ui.Started += OnStarted;
            
            try
            {
            	var replayNames = Directory.GetFiles(replayFolderName, "*." + replayExtension)
            		.Select(filePath => Path.GetFileNameWithoutExtension(filePath))
            		.OrderBy(name => name);
                foreach (string replayName in replayNames) this.ui.AddReplay(replayName);
            }
            catch (DirectoryNotFoundException exception)
            {
            	// This happens if no replay has been saved.
            }
            catch (IOException exception)
            {
            	Debug.Fail("Unexpected exception while attempting to enumerate replays: \n" + exception.ToString());
            }
        }
        #endregion

        #region Methods
        #region Overrides
        protected internal override void OnEntered()
        {
            graphics.UIManager.Content = ui;
        }

        protected internal override void OnShadowed()
        {
            graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
        	graphics.DrawGui();
        }
        #endregion

        private void OnExited(ReplayBrowser2 sender)
        {
            Manager.Pop();
        }

        private void OnStarted(ReplayBrowser2 sender, string replayName)
        {
        	string replayFilePath = Path.Combine(replayFolderName, replayName + "." + replayExtension);
        	
            ReplayReader replayReader = new ReplayReader(replayFilePath);
            MatchSettings matchSettings = replayReader.MatchSettings;
            PlayerSettings playerSettings = replayReader.PlayerSettings;

            Random random = new MersenneTwister(matchSettings.RandomSeed);

            Terrain terrain = Terrain.Generate(matchSettings.MapSize, random);
            World world = new World(terrain, random, matchSettings.FoodLimit);

            Match match = new Match(world, random);
            match.AreRandomHeroesEnabled = matchSettings.AreRandomHeroesEnabled;

            Faction localFaction = world.CreateSpectatorFaction();
            SlaveCommander localCommander = new SlaveCommander(match, localFaction);

            int colorIndex = 0;
            foreach (Player player in playerSettings.Players)
            {
                ColorRgb color = player.Color;
                string factionName = player.Name;
                ++colorIndex;

                Faction faction = world.CreateFaction(factionName, color);
                faction.AladdiumAmount = matchSettings.InitialAladdiumAmount;
                faction.AlageneAmount = matchSettings.InitialAlageneAmount;
                if (matchSettings.RevealTopology) faction.LocalFogOfWar.Reveal();
            }

            WorldGenerator.Generate(world, match.UnitTypes, random, !matchSettings.StartNomad);

            CommandPipeline commandPipeline = new CommandPipeline(match);
            if (matchSettings.AreCheatsEnabled) commandPipeline.PushFilter(new CheatCodeExecutor(match));
            commandPipeline.PushFilter(new ReplayPlayer(replayReader));

            GameState targetGameState = new DeathmatchGameState(Manager, graphics,
                match, commandPipeline, localCommander);
            Manager.Push(targetGameState);
        }
        #endregion
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.Engine;
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
        
        private readonly ReplayBrowser ui;
        #endregion

        #region Constructors
        public ReplayBrowserGameState(GameStateManager manager)
            : base(manager)
        {
            this.ui = new ReplayBrowser(Graphics, Localizer);

            this.ui.Exited += OnExited;
            this.ui.Started += OnStarted;
            
            try
            {
            	var replayNames = Directory.GetFiles(replayFolderName, "*." + replayExtension)
            		.Select(filePath => Path.GetFileNameWithoutExtension(filePath))
            		.OrderBy(name => name);
                foreach (string replayName in replayNames) this.ui.AddReplay(replayName);
        }
            catch (DirectoryNotFoundException)
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
            Graphics.UIManager.Content = ui;
        }

        protected internal override void OnShadowed()
        {
            Graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            Graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics Graphics)
        {
        	Graphics.DrawGui();
        }
        #endregion

        private void OnExited(ReplayBrowser sender)
        {
            Manager.Pop();
        }

        private void OnStarted(ReplayBrowser sender, string replayName)
        {
        	string replayFilePath = Path.Combine(replayFolderName, replayName + "." + replayExtension);
        	
            ReplayReader replayReader = new ReplayReader(replayFilePath);
            MatchSettings matchSettings = replayReader.MatchSettings;
            PlayerSettings playerSettings = replayReader.PlayerSettings;

            Random random = new MersenneTwister(matchSettings.RandomSeed);

            WorldGenerator generator = new RandomWorldGenerator(random, matchSettings.MapSize, !matchSettings.StartNomad);
            Terrain terrain = generator.GenerateTerrain();
            World world = new World(terrain, random, matchSettings.FoodLimit);

            Match match = new Match(Manager.AssetsDirectory, world, random);
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

            generator.PrepareWorld(world, match.UnitTypes);

            CommandPipeline commandPipeline = new CommandPipeline(match);
            if (matchSettings.AreCheatsEnabled) commandPipeline.PushFilter(new CheatCodeExecutor(match));
            commandPipeline.PushFilter(new ReplayPlayer(replayReader));

            GameState targetGameState = new DeathmatchGameState(Manager,
                match, commandPipeline, localCommander);
            Manager.Push(targetGameState);
        }
        #endregion
    }
}

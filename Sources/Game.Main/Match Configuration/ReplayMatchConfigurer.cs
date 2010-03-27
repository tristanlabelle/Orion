﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;

namespace Orion.Main
{
    class ReplayMatchConfigurer : MatchConfigurer
    {
        private const string neutralFactionName = "\rSpectator";

        private ReplayReader replay;

        public ReplayMatchConfigurer(string replayName)
        {
            FileInfo replayFile = new FileInfo(replayName);
            BinaryReader replayReader = new BinaryReader(replayFile.OpenRead());
            replay = new ReplayReader(replayReader);

            options = replay.Options;
        }

        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return null; }
        }

        public override void Start(out Match match, out SlaveCommander localCommander)
        {
            CreateWorld(options.MapSize);

            Faction userFaction = world.CreateSpectatorFaction();
            userFaction.LocalFogOfWar.Disable();
            localCommander = new SlaveCommander(userFaction);

            int colorIndex = 0;
            foreach (string factionName in replay.FactionNames)
            {
                Faction faction = world.CreateFaction(factionName, Faction.Colors[colorIndex], options.InitialAladdiumAmount, options.InitialAlageneAmount);
                if (options.RevealTopology)
                    faction.LocalFogOfWar.Reveal();
                colorIndex++;
            }

            WorldGenerator.Generate(world, random, options.IsNomad);
            match = new Match(random, world);
            match.IsPausable = true;

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.PushFilter(new CheatCodeExecutor(CheatCodeManager.Default, match));
            pipeline.PushFilter(new ReplayPlayer(replay));

            match.Updated += (sender, args) =>
                pipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);
        }
    }
}

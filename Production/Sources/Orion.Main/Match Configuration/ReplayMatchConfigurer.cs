using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Color = System.Drawing.Color;
using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.UserInterface;

namespace Orion.Main
{
    class ReplayMatchConfigurer : MatchConfigurer
    {
        private const string neutralFactionName = "\rSpectator";
        private static readonly Color neutralFactionColor = Color.Black;

        private ReplayReader replay;

        public ReplayMatchConfigurer(string replayName)
        {
            FileInfo replayFile = new FileInfo(replayName);
            BinaryReader replayReader = new BinaryReader(replayFile.OpenRead());
            replay = new ReplayReader(replayReader);

            Seed = replay.WorldSeed;
        }

        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return null; }
        }

        public override void Start(out Match match, out SlaveCommander localCommander)
        {
            CreateWorld();

            Faction userFaction = world.CreateSpectatorFaction();
            userFaction.LocalFogOfWar.Disable();
            localCommander = new SlaveCommander(userFaction);

            int colorIndex = 0;
            foreach (string factionName in replay.FactionNames)
            {
                world.CreateFaction(factionName, playerColors[colorIndex]);
                colorIndex++;
            }

            match = new Match(random, world);
            match.IsPausable = true;

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.PushFilter(new CheatCodeExecutor(match));
            pipeline.PushFilter(new ReplayPlayer(replay));

            match.Updated += (sender, args) => pipeline.Update(sender.LastFrameNumber, args.TimeDeltaInSeconds);
        }
    }
}

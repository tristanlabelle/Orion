using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Matchmaking;

namespace Orion.Tools.ReplayPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            using (ReplayReader replayReader = new ReplayReader(args[0]))
            {
                bool owned = false;
                TextWriter textWriter = null;
                if (args.Length == 1)
                {
                    textWriter = Console.Out;
                    owned = false;
                }
                else
                {
                    textWriter = new StreamWriter(args[1]);
                    owned = true;
                }

                try
                {
                    PrintReplay(replayReader, textWriter);
                }
                finally
                {
                    if (owned) textWriter.Dispose();
                }
            }
        }

        static void PrintReplay(ReplayReader replay, TextWriter output)
        {
            PrintHeader(replay, output);
            PrintCommands(replay, output);
        }

        static void PrintHeader(ReplayReader replay, TextWriter output)
        {
            output.WriteLine("Game seed is {0}", replay.Options.Seed);
            output.WriteLine("There are {0} factions:", replay.FactionNames.Count());
            foreach (string factionName in replay.FactionNames)
                output.WriteLine("  - {0}", factionName);
        }

        static void PrintCommands(ReplayReader replay, TextWriter output)
        {
            while (!replay.IsEndOfStreamReached)
            {
                ReplayEvent replayEvent = replay.ReadCommand();
                output.WriteLine("Update #{0}: {1}", replayEvent.UpdateNumber, replayEvent.Command);
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("usage: Orion.Tools.ReplayPrinter replay-name [output-file]");
        }
    }
}

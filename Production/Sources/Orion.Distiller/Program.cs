using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment;

namespace Orion.Distiller
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
                Stream outputStream = args.Length == 1
                    ? Console.OpenStandardOutput()
                    : new FileStream(args[1], FileMode.Create);

                using (StreamWriter outputWriter = new StreamWriter(outputStream))
                {
                    PrintReplay(replayReader, outputWriter);
                }
            }
        }

        static void PrintReplay(ReplayReader replay, StreamWriter output)
        {
            PrintHeader(replay, output);
            PrintCommands(replay, output);
        }

        static void PrintHeader(ReplayReader replay, StreamWriter output)
        {
            output.WriteLine("Game seed is {0}", replay.WorldSeed);
            output.WriteLine("There are {0} factions:", replay.FactionNames.Count());
            foreach (string factionName in replay.FactionNames)
                output.WriteLine("  - {0}", factionName);
        }

        static void PrintCommands(ReplayReader replay, StreamWriter output)
        {
            while (!replay.IsEndOfStreamReached)
            {
                ReplayEvent replayEvent = replay.ReadCommand();
                output.WriteLine("Update #{0}: {1}", replayEvent.UpdateNumber, replayEvent.Command);
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("usage: Orion.Distiller replay-name [output-file]");
        }
    }
}

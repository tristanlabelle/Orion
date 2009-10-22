using System;
using Color = System.Drawing.Color;

using OpenTK.Math;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Main
{
	class SinglePlayerMatchConfigurer : MatchConfigurer
	{
		public override Match Start()
		{
            var random = new MersenneTwister();
            Console.WriteLine("Mersenne twister seed: {0}", random.Seed);
			
			Terrain terrain = Terrain.Generate(100, 100, random);
			World world = new World(terrain);

            Faction redFaction = world.CreateFaction("Red", Color.Red);
            UserInputCommander redCommander = new UserInputCommander(redFaction);

            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            DummyAICommander blueCommander = new DummyAICommander(blueFaction, random);

            SinglePlayerCommandPipeline pipeline = new SinglePlayerCommandPipeline();

            redCommander.AddToPipeline(pipeline);
            blueCommander.AddToPipeline(pipeline);

            return new Match(random, redCommander, terrain, world, pipeline);
		}

	}
}

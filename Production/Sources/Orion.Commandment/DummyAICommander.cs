using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.GameLogic;

namespace Orion.Commandment
{
    public sealed class DummyAICommander : Commander
    {
        #region Fields
        private readonly Random random = new Random();
        #endregion

        #region Constructors
        public DummyAICommander(Faction faction, Random random)
            : base(faction)
        {
            Argument.EnsureNotNull(random, "random");
            this.random = random;
        }
        #endregion

        #region Methods
        public override void AddToPipeline(CommandPipeline pipeline)
        {
            pipeline.AddCommander(this);
            commandsEntryPoint = pipeline.AICommandmentEntryPoint;
        }

        public override void Update(float timeDelta)
        {
            List<Unit> unitsToMove = World.Entities
                .OfType<Unit>()
                .Where(unit => unit.Faction == Faction && unit.IsIdle)
                .ToList();

            if (unitsToMove.Count != 0)
            {
                Command command = new Move(Faction, unitsToMove,
                    new Vector2(random.Next(World.Width), random.Next(World.Height)));
                GenerateCommand(command);
            }
        }
        #endregion
    }
}

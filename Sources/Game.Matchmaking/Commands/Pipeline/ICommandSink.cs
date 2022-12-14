using System;
using Orion.Engine;

namespace Orion.Game.Matchmaking.Commands.Pipeline
{
    /// <summary>
    /// Interface for objects which can handle commands and serve as an end point to a command pipeline.
    /// </summary>
    public interface ICommandSink : IDisposable
    {
        void Handle(Command command);
        void Update(SimulationStep step);
    }
}

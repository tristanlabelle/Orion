using System;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// Interface for objects which can handle commands and serve as an end point to a command pipeline.
    /// </summary>
    public interface ICommandSink : IDisposable
    {
        void Handle(Command command);
        void Update(int updateNumber, float timeDeltaInSeconds);
    }
}

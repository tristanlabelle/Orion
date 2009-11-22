
namespace Orion
{
    /// <summary>
    /// Objects of this structure encapsulate the data of a frame update.
    /// </summary>
    public struct UpdateEventArgs
    {
        /// <summary>
        /// The time, in seconds, since the last Update event was triggered
        /// </summary>
        public readonly float TimeDelta;

        /// <summary>
        /// Constructs a new Update event arguments structure with a given time delta.
        /// </summary>
        /// <param name="timeDelta">The time in seconds since the last Update event was triggered.</param>
        public UpdateEventArgs(float timeDelta)
        {
            TimeDelta = timeDelta;
        }

        public override string ToString()
        {
            return "Update Event Args delta={0}".FormatInvariant(TimeDelta);
        }
    }
}

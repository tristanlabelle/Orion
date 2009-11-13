using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Orion.Networking
{
    /// <summary>
    /// Represent a connection to a distant computer (peer)
    /// </summary>
    public sealed class PeerLink
    {
        #region Fields
        private readonly IPv4EndPoint endPoint;
        private readonly Queue<TimeSpan> pings = new  Queue<TimeSpan>();
        #endregion

        #region Construstor
        public PeerLink(IPv4EndPoint endPoint)
        {
            this.endPoint = endPoint;
        }
        #endregion

        #region Proprieties
        public IPv4EndPoint EndPoint
        {
            get { return endPoint; }
        }

        public bool HasPingData
        {
            get { return pings.Count > 0; }
        }

        public TimeSpan AveragePing
        {
            get
            {
                lock (pings)
                {
                    if (pings.Count == 0) return TimeSpan.Zero;
                    return LocklessAveragePing;
                }
            }
        }

        private TimeSpan LocklessAveragePing
        {
            get
            {
                long averageTicks = (long)pings.Average(timeSpan => timeSpan.Ticks);
                return TimeSpan.FromTicks(averageTicks);
            }
        }

        public TimeSpan AveragePingDeviation
        {
            get
            {
                lock (pings)
                {
                    if (pings.Count == 0) return TimeSpan.Zero;

                    TimeSpan averagePing = LocklessAveragePing;

                    long deviationSumInTicks = pings.Sum(ping => Math.Abs(averagePing.Ticks - ping.Ticks));
                    long averageDeviationInTicks = deviationSumInTicks / pings.Count;
                    return TimeSpan.FromTicks(averageDeviationInTicks);
                }
            }
        }
        #endregion

        #region Methods
        public void AddPing(TimeSpan timeSpan)
        {
            lock (pings)
            {
                pings.Enqueue(timeSpan);
                if (pings.Count > 50) pings.Dequeue();
            }
        }
        #endregion
    }
}

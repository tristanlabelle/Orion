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
        private readonly Queue<TimeSpan> pings
               = new  Queue<TimeSpan>();

        private readonly Ipv4EndPoint ipEndPoint;
        
        private static readonly TimeSpan DefaultPing = TimeSpan.FromMilliseconds(100);
        #endregion

        #region Construstor
        public PeerLink(Ipv4EndPoint ipEndPoint)
        {
            this.ipEndPoint = ipEndPoint;
        }
        #endregion

        #region Proprieties
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
                    if (pings.Count == 0) return DefaultPing;

                    long averageTicks = (long)pings.Average(timeSpan => timeSpan.Ticks);
                    return TimeSpan.FromTicks(averageTicks);
                }
            }
        }

        public TimeSpan StandardDeviationForPings
        {
            get
            {

                long deviationInTicks = 0;

                if (pings.Count == 0) return TimeSpan.FromMilliseconds(50);

                TimeSpan average = AveragePing;
                lock (pings)
                {
                    foreach (TimeSpan ping in pings)
                        deviationInTicks += Math.Abs(average.Ticks - ping.Ticks);

                    deviationInTicks /= pings.Count;
                }

                return TimeSpan.FromTicks(deviationInTicks);
            }
        }
        #endregion

        #region Methods
        #region Ping calculation methods
        public void AddPing(TimeSpan timeSpan)
        {
            

            lock (pings)
            {
                pings.Enqueue(timeSpan);
                if (pings.Count > 50)
                {
                    pings.Dequeue();
                }
            }
        }
        #endregion
        #endregion
    }
}

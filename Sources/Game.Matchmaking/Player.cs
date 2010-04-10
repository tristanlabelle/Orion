using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Orion.Engine.Networking;
using Orion.Game.Simulation;
using Orion.Engine;

namespace Orion.Game.Matchmaking
{
    public abstract class Player
    {
        #region Fields
        private ColorRgb color;
        #endregion

        #region Constructors
        public Player(ColorRgb color)
        {
            this.color = color;
        }
        #endregion

        #region Events
        public event Action<Player> ColorChanged;
        #endregion

        #region Properties
        public ColorRgb Color
        {
            get { return color; }
            set
            {
                color = value;
                ColorChanged.Raise(this);
            }
        }
        #endregion

        #region Methods
        public abstract override string ToString();
        #endregion
    }

    public sealed class LocalPlayer : Player
    {
        public LocalPlayer(ColorRgb color)
            : base(color)
        { }

        public override string ToString()
        {
            return "Vous";
        }
    }

    public sealed class RemotePlayer : Player
    {
        #region Fields
        private readonly IPv4EndPoint hostEndPoint;
        private readonly string hostName;
        #endregion

        #region Constructors
        public RemotePlayer(ColorRgb color, IPv4EndPoint endPoint)
            : base(color)
        {
            this.hostEndPoint = endPoint;
            IPHostEntry hostEntry = Dns.GetHostEntry(endPoint.Address);
            hostName = hostEntry == null ? null : hostEntry.HostName;
        }

        #endregion

        #region Properties
        public IPv4EndPoint HostEndPoint
        {
            get { return hostEndPoint; }
        }

        public string HostName
        {
            get { return hostName; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return hostName ?? hostEndPoint.ToString();
        }
        #endregion
    }

    public sealed class AIPlayer : Player
    {
        public AIPlayer(ColorRgb color)
            : base(color)
        { }

        public override string ToString()
        {
            return "Ordinateur";
        }
    }
}

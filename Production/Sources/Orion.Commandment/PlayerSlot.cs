using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

using Orion.GameLogic;

namespace Orion.Commandment
{
    public abstract class PlayerSlot
    {
        private Color color;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        public abstract bool NeedsFaction { get; }
        public abstract override string ToString();
    }

    public sealed class RemotePlayerSlot : PlayerSlot
    {
        public IPv4EndPoint? RemoteHost { get; set; }

        public override bool NeedsFaction
        {
            get { return RemoteHost.HasValue; }
        }

        public override string ToString()
        {
            if (RemoteHost.HasValue)
                return RemoteHost.Value.ToString();
            return "Open";
        }
    }

    public sealed class ClosedPlayerSlot : PlayerSlot
    {
        public override bool NeedsFaction
        {
            get { return false; }
        }

        public override string ToString()
        {
            return "Closed";
        }
    }

    public sealed class AIPlayerSlot : PlayerSlot
    {
        public override bool NeedsFaction
        {
            get { return true; }
        }

        public override string ToString()
        {
            return "Computer";
        }
    }

    public sealed class LocalPlayerSlot : PlayerSlot
    {
        public override bool NeedsFaction
        {
            get { return true; }
        }

        public override string ToString()
        {
            return "You";
        }
    }
}

using System;
using Orion.Engine;
using System.IO;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Base class for classes which provide the description of a specific type of match participant.
    /// </summary>
    public abstract class Player
    {
        #region Instance
        #region Fields
        /// <summary>
        /// The maximum length of player names.
        /// </summary>
        /// <remarks>
        /// This limits exists for networking purposes.
        /// </remarks>
        public static readonly int MaximumNameLength = 20;

        private string name;
        private ColorRgb color;
        #endregion

        #region Constructors
        public Player(string name, ColorRgb color)
        {
            Argument.EnsureNotNull(name, "name");

            this.Name = name;
            this.color = ColorRgb.Clamp(color);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised after the name of this player has changed.
        /// </summary>
        public event Action<Player> NameChanged;

        /// <summary>
        /// Raised after the color of this player has changed.
        /// </summary>
        public event Action<Player> ColorChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of this player.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNull(value, "Name");
                if (value.Length > MaximumNameLength)
                    //throw new ArgumentException("The name exceeds the maximum length.", "Name");
                    value = value.Substring(0, MaximumNameLength);

                this.name = value;
                NameChanged.Raise(this);
            }
        }

        /// <summary>
        /// Accesses the color of this player's faction.
        /// </summary>
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
        public override string ToString()
        {
            return name;
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        public static readonly BinarySerializer<Player> Serializer;
        #endregion

        #region Constructor
        static Player()
        {
            Serializer = BinarySerializer<Player>.FromCallingAssemblyExportedTypes();
        }
        #endregion

        #region Methods
        protected static void SerializeNameAndColor(Player player, BinaryWriter writer)
        {
            writer.Write(player.name);
            player.color.Serialize(writer);
        }
        #endregion
        #endregion
    }
}

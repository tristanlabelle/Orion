using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using Orion.Engine;
using Orion.Game.Matchmaking.Commands;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Provides means of reading replay files.
    /// </summary>
    public sealed class ReplayReader : IDisposable
    {
        #region Fields
        private readonly BinaryReader reader;
        private readonly MatchSettings matchSettings = new MatchSettings();
        private readonly PlayerSettings playerSettings = new PlayerSettings();
        private int lastUpdateNumber = 0;
        #endregion

        #region Constructors
        public ReplayReader(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");
            this.reader = reader;

            matchSettings.Deserialize(reader);
            playerSettings.Deserialize(reader);
        }

        public ReplayReader(Stream stream)
            : this(new BinaryReader(stream)) { }

        public ReplayReader(string filePath)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) { }
        #endregion

        #region Properties
        public MatchSettings MatchSettings
        {
            get { return matchSettings; }
        }

        public PlayerSettings PlayerSettings
        {
            get { return playerSettings; }
        }

        /// <summary>
        /// Gets a value indicating if the end of the stream was reached.
        /// </summary>
        public bool IsEndOfStreamReached
        {
            get { return reader.BaseStream.Position == reader.BaseStream.Length; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads the next command from the data stream.
        /// </summary>
        /// <returns>The command that was read, paired with the number of the update where it occured.</returns>
        public ReplayEvent ReadCommand()
        {
            int updateNumber = reader.ReadInt32();
            Command command = Command.Serializer.Deserialize(reader);

            if (updateNumber < lastUpdateNumber)
                throw new InvalidDataException("Replay command numbers are not ascending .");
            lastUpdateNumber = updateNumber;

            return new ReplayEvent(updateNumber, command);
        }

        /// <summary>
        /// Closes the underlying data stream.
        /// </summary>
        public void Dispose()
        {
            reader.Close();
        }
        #endregion
    }
}

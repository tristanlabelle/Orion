﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using Orion.Engine;
using Orion.Matchmaking.Commands;

namespace Orion.Matchmaking
{
    /// <summary>
    /// Provides means of reading replay files.
    /// </summary>
    public sealed class ReplayReader : IDisposable
    {
        #region Fields
        private readonly BinaryReader reader;
        private readonly int worldSeed;
        private readonly ReadOnlyCollection<string> factionNames;
        private int lastUpdateNumber = 0;
        #endregion

        #region Constructors
        public ReplayReader(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");
            this.reader = reader;
            worldSeed = reader.ReadInt32();

            int factionCount = reader.ReadInt32();
            factionNames = Enumerable.Range(0, factionCount)
                .Select(i => reader.ReadString())
                .ToList()
                .AsReadOnly();
        }

        public ReplayReader(Stream stream)
            : this(new BinaryReader(stream)) { }

        public ReplayReader(string filePath)
            : this(new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the seed used to generate the world.
        /// </summary>
        public int WorldSeed
        {
            get { return worldSeed; }
        }

        /// <summary>
        /// Gets the names of the factions in the game.
        /// </summary>
        public IEnumerable<string> FactionNames
        {
            get { return factionNames; }
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
            Command command = Command.Deserialize(reader);

            if (updateNumber < lastUpdateNumber)
                throw new InvalidDataException("Commands aren't in order in replay.");
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

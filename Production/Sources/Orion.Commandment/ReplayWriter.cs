using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Commandment
{
    /// <summary>
    /// Provides means of creating replay files.
    /// </summary>
    public sealed class ReplayWriter : IDisposable
    {
        #region Fields
        private readonly BinaryWriter writer;
        private bool autoFlush = true;
        private bool isHeaderWritten;
        private int lastUpdateNumber = 0;
        #endregion

        #region Constructors
        public ReplayWriter(BinaryWriter writer)
        {
            Argument.EnsureNotNull(writer, "writer");
            this.writer = writer;
        }

        public ReplayWriter(Stream stream)
            : this(new BinaryWriter(stream)) {}

        public ReplayWriter(string filePath)
            : this(new BinaryWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses a value indicating if the underlying stream
        /// should be automatically flushed after each write operation.
        /// </summary>
        public bool AutoFlush
        {
            get { return autoFlush; }
            set { autoFlush = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes the header of the replay.
        /// </summary>
        /// <param name="worldSeed">The seed of the world.</param>
        /// <param name="factionNames">The names of the factions participating.</param>
        public void WriteHeader(int worldSeed, IEnumerable<string> factionNames)
        {
            if (isHeaderWritten) throw new InvalidOperationException("Cannot write more than one replay header.");

            Argument.EnsureNotNull(factionNames, "factionNames");
            List<string> factionNameList = factionNames.ToList();
            Argument.EnsureNoneNull(factionNames, "factionNames");

            writer.Write(worldSeed);

            writer.Write(factionNameList.Count);
            foreach (string factionName in factionNameList)
                writer.Write(factionName);

            if (autoFlush) writer.Flush();

            isHeaderWritten = true;
        }

        /// <summary>
        /// Adds a <see cref="Command"/> to the replay data.
        /// </summary>
        /// <param name="updateNumber">The update number where the <see cref="Command"/> occured.</param>
        /// <param name="command">The <see cref="Command"/> to be written.</param>
        public void WriteCommand(int updateNumber, Command command)
        {
            if (!isHeaderWritten) throw new InvalidOperationException("Cannot write a command without a replay header.");

            if (updateNumber < lastUpdateNumber) throw new ArgumentException("Updates should be increasing.");
            Argument.EnsureNotNull(command, "command");

            writer.Write(updateNumber);
            command.Serialize(writer);
            if (autoFlush) writer.Flush();
            
            lastUpdateNumber = updateNumber;
        }

        /// <summary>
        /// Flushes the underlying data stream.
        /// </summary>
        public void Flush()
        {
            writer.Flush();
        }

        /// <summary>
        /// Closes the underlying data stream.
        /// </summary>
        public void Dispose()
        {
            writer.Close();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Commandment.Commands;

namespace Orion.Commandment
{
    /// <summary>
    /// Provides means of creating replay files.
    /// </summary>
    public sealed class ReplayWriter : IDisposable
    {
        #region Instance
        #region Fields
        private readonly BinaryWriter writer;
        private bool autoFlush;
        private bool isHeaderWritten;
        private int lastUpdateNumber = 0;
        #endregion

        #region Constructors
        public ReplayWriter(BinaryWriter writer)
        {
            Argument.EnsureNotNull(writer, "writer");
            this.writer = writer;

#if !DEBUG
            this.autoFlush = true;
#endif
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
        #endregion

        #region Static
        #region Fields
        private static readonly DirectoryInfo directory = new DirectoryInfo("Replays");
        #endregion

        #region Methods
        public static ReplayWriter TryCreate()
        {
            Stream stream = TryOpenFileStream();
            if (stream == null) return null;

            return new ReplayWriter(stream);
        }

        private static Stream TryOpenFileStream()
        {
            if (!directory.Exists)
            {
                try { directory.Create(); }
                catch (IOException) { return null; }
            }

            DateTime now = DateTime.Now;

            string dateString = "{0:D4}-{1:D2}-{2:D2} {3:D2}.{4:D2}.{5:D2}"
                .FormatInvariant(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            for (int i = 0; i < 10; ++i)
            {
                string filePath = dateString;
                if (i > 0) filePath += " ({0})".FormatInvariant(i + 1);
                filePath += ".replay";

                filePath = Path.Combine(directory.Name, filePath);

                try { return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read); }
                catch (IOException) { }
            }

            return null;
        }
        #endregion
        #endregion
    }
}

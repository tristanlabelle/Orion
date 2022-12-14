using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Engine;
using Orion.Game.Matchmaking.Commands;

namespace Orion.Game.Matchmaking
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
        private int lastUpdateNumber = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new replay writer from the initial match settings.
        /// </summary>
        /// <param name="writer">The binary writer to be used.</param>
        /// <param name="settings">The initial match settings.</param>
        /// <param name="factionNames">The names of the participating factions.</param>
        public ReplayWriter(BinaryWriter writer, MatchSettings settings, PlayerSettings playerSettings)
        {
            Argument.EnsureNotNull(writer, "writer");
            Argument.EnsureNotNull(settings, "settings");
            Argument.EnsureNotNull(playerSettings, "playerSettings");

            this.writer = writer;
#if DEBUG
            this.autoFlush = true;
#endif

            settings.Serialize(writer);
            playerSettings.Serialize(writer);

            if (autoFlush) writer.Flush();
        }

        public ReplayWriter(Stream stream, MatchSettings settings, PlayerSettings playerSettings)
            : this(new BinaryWriter(stream, Encoding.UTF8), settings, playerSettings) { }

        public ReplayWriter(string filePath, MatchSettings settings, PlayerSettings playerSettings)
            : this(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read), settings, playerSettings) { }
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
        /// Adds a <see cref="Command"/> to the replay data.
        /// </summary>
        /// <param name="updateNumber">The update number where the <see cref="Command"/> occured.</param>
        /// <param name="command">The <see cref="Command"/> to be written.</param>
        public void WriteCommand(int updateNumber, Command command)
        {
            if (updateNumber < lastUpdateNumber) throw new ArgumentException("Update numbers should be increasing.");
            Argument.EnsureNotNull(command, "command");

            writer.Write(updateNumber);
            Command.Serializer.Serialize(command, writer);
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
#if DEBUG
        private static readonly DirectoryInfo directory = new DirectoryInfo("Replays");
#else
        private static readonly DirectoryInfo directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Orion\\Replays");
#endif
        #endregion

        #region Methods
        public static ReplayWriter TryCreate(MatchSettings settings, PlayerSettings playerSettings)
        {
            Stream stream = TryOpenFileStream();
            if (stream == null) return null;

            return new ReplayWriter(stream, settings, playerSettings);
            
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

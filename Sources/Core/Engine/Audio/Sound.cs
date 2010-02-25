using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrrKlang;

namespace Orion.Engine.Audio
{
    public sealed class Sound
    {
        #region Fields
        private readonly string name;
        private readonly string filePath;
        private readonly ISoundSource irrKlangSource;
        #endregion

        #region Constructors
        internal Sound(string name, string filePath, ISoundSource source)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(filePath, "filePath");
            Argument.EnsureNotNull(source, "source");

            this.name = name;
            this.filePath = filePath;
            this.irrKlangSource = source;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of this sound.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        public string FilePath
        {
            get { return filePath; }
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromMilliseconds(irrKlangSource.PlayLength); }
        }

        public float DurationInSeconds
        {
            get { return (float)Duration.TotalSeconds; }
        }

        internal ISoundSource IrrKlangSource
        {
            get { return irrKlangSource; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return name;
        }

        internal void Dispose()
        {
            irrKlangSource.Dispose();
        }
        #endregion
    }
}

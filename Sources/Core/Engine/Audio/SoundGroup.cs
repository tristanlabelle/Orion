using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using IrrKlang;

namespace Orion.Engine.Audio
{
    internal sealed class SoundGroup : IDisposable
    {
        #region Fields
        private readonly string name;
        private readonly ReadOnlyCollection<ISound> sounds;
        #endregion

        #region Constructors
        internal SoundGroup(string name, IEnumerable<ISound> sounds)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(sounds, "sounds");

            this.name = name;
            this.sounds = sounds.ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }

        public ReadOnlyCollection<ISound> Sounds
        {
            get { return sounds; }
        }

        public int SoundCount
        {
            get { return sounds.Count; }
        }

        public bool IsEmpty
        {
            get { return sounds.Count == 0; }
        }
        #endregion

        #region Methods
        public ISound GetRandomSoundOrNull(Random random)
        {
            Argument.EnsureNotNull(random, "random");

            if (sounds.Count == 0) return null;

            int index = random.Next(sounds.Count);
            return sounds[index];
        }

        public void Dispose()
        {
            foreach (ISound sound in sounds)
                sound.Dispose();
        }

        public override string ToString()
        {
            return "{0} ({1} sounds(s))".FormatInvariant(name, SoundCount);
        }
        #endregion
    }
}

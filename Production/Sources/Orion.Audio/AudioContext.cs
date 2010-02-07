using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using OpenTK.Math;
using IrrKlang;
using System.Text.RegularExpressions;

namespace Orion.Audio
{
    public sealed class AudioContext : IDisposable
    {
        #region Fields
        internal static readonly string[] SupportedFormats = new[] { ".wav", ".ogg" };

        private readonly Random random = new Random();
        private readonly ISoundEngine engine;
        private readonly Dictionary<string, SoundGroup> soundGroups = new Dictionary<string, SoundGroup>();
        private float volume = 1;
        private bool isMuted;
        #endregion

        #region Constructors
        public AudioContext()
        {
            engine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MultiThreaded);
        }
        #endregion

        #region Properties
        public float Volume
        {
            get { return volume; }
            set
            {
                if (!isMuted) engine.SoundVolume = value;
                volume = value;
            }
        }

        public bool IsMuted
        {
            get { return isMuted; }
            set
            {
                isMuted = value;
                engine.SoundVolume = isMuted ? 0 : volume;
            }
        }
        #endregion

        #region Methods
        public void PlaySound(string name)
        {
            Argument.EnsureNotNull(name, "name");

            SoundGroup soundGroup = GetSoundGroup(name);
            if (soundGroup.IsEmpty) return;

            string filePath = soundGroup.GetRandomFilePath(random);
            engine.Play2D(filePath);
        }

        public void Dispose()
        {
            engine.RemoveAllSoundSources();
            engine.StopAllSounds();
            engine.__dtor();
        }

        private SoundGroup GetSoundGroup(string name)
        {
            SoundGroup soundGroup;
            if (soundGroups.TryGetValue(name, out soundGroup))
                return soundGroup;

            soundGroup = new SoundGroup(name);
            soundGroups.Add(name, soundGroup);
            return soundGroup;
        }
        #endregion
    }
}

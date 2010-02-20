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
        private readonly Random random = new Random();
        private readonly ISoundEngine irrKlangEngine;
        private readonly Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
        private readonly Dictionary<string, SoundGroup> soundGroups = new Dictionary<string, SoundGroup>();
        private Vector3 listenerPosition;
        private Vector3 listenerDirection;
        private float volume = 1;
        private bool isMuted;
        #endregion

        #region Constructors
        public AudioContext()
        {
            try
            {
                irrKlangEngine = new ISoundEngine();
            }
            catch (Exception) // We have to catch Exception as that's what's thrown by IrrKlang >.<
            {
                irrKlangEngine = new ISoundEngine(SoundOutputDriver.NullDriver);
            }

            listenerPosition = Vector3.Zero;
            listenerDirection = new Vector3(0, 0, 1);
        }
        #endregion

        #region Events
        public event Action<AudioContext> ListenerPositionChanged;
        #endregion

        #region Properties
        public string DriverName
        {
            get { return irrKlangEngine.Name; }
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                if (!isMuted) irrKlangEngine.SoundVolume = value;
                volume = value;
            }
        }

        public bool IsMuted
        {
            get { return isMuted; }
            set
            {
                isMuted = value;
                irrKlangEngine.SoundVolume = isMuted ? 0 : volume;
            }
        }

        public Vector3 ListenerPosition
        {
            get { return listenerPosition; }
            set
            {
                listenerPosition = value;
                UpdateIrrKlangListener();
                if (ListenerPositionChanged != null) ListenerPositionChanged(this);
            }
        }

        public Vector3 ListenerDirection
        {
            get { return listenerDirection; }
            set
            {
                listenerDirection = value;
                UpdateIrrKlangListener();
            }
        }

        internal ISoundEngine IrrKlangEngine
        {
            get { return irrKlangEngine; }
        }

        internal Vector3D IrrKlangListenerPosition
        {
            get { return new Vector3D(listenerPosition.X, listenerPosition.Y, listenerPosition.Z); }
        }
        #endregion

        #region Methods
        public Sound GetSound(string name)
        {
            Argument.EnsureNotNull(name, "name");

            Sound sound;
            if (sounds.TryGetValue(name, out sound))
                return sound;

            string filePath = Directory.GetFiles("../../../Assets/Sounds/", name + ".*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path => Path.GetFileName(path).LastIndexOf('.') == name.Length);

            if (filePath == null)
            {
                sounds.Add(name, null);
                return null;
            }

            ISoundSource source = irrKlangEngine.AddSoundSourceFromFile(filePath);
            if (source == null)
            {
                sounds.Add(name, null);
                return null;
            }

            sound = new Sound(name, filePath, source);
            sounds.Add(name, sound);
            return sound;
        }

        public SoundGroup GetSoundGroup(string name)
        {
            SoundGroup soundGroup;
            if (soundGroups.TryGetValue(name, out soundGroup))
                return soundGroup;

            var sounds = Directory.GetFiles("../../../Assets/Sounds/", name + ".*")
                .Select(filePath => Path.GetFileNameWithoutExtension(filePath))
                .Select(soundName => GetSound(soundName))
                .Where(sound => sound != null);

            soundGroup = new SoundGroup(name, sounds);
            soundGroups.Add(name, soundGroup);
            return soundGroup;
        }

        public Sound GetRandomSoundFromGroup(string name)
        {
            Argument.EnsureNotNull(name, "name");

            SoundGroup soundGroup = GetSoundGroup(name);
            if (soundGroup == null || soundGroup.IsEmpty) return null;

            return soundGroup.GetRandomSound(random);
        }

        public SoundSource CreateSource()
        {
            return new SoundSource(this);
        }

        public void PlayAndForget(Sound sound, Vector2? position)
        {
            Argument.EnsureNotNull(sound, "sound");

            if (position.HasValue)
                irrKlangEngine.Play3D(sound.IrrKlangSource, position.Value.X, position.Value.Y, 0, false, false, false);
            else
                irrKlangEngine.Play2D(sound.IrrKlangSource, false, false, false);
        }

        public void Dispose()
        {
            irrKlangEngine.StopAllSounds();

            soundGroups.Clear();

            foreach (Sound sound in sounds.Values)
                sound.Dispose();
            sounds.Clear();

            irrKlangEngine.RemoveAllSoundSources();
            irrKlangEngine.__dtor();
        }

        private void UpdateIrrKlangListener()
        {
            Vector3D position = new Vector3D(listenerPosition.X, listenerPosition.Y, listenerPosition.Z);
            Vector3D direction = new Vector3D(listenerDirection.X, listenerDirection.Y, listenerDirection.Z);
            irrKlangEngine.SetListenerPosition(position, direction);
        }
        #endregion
    }
}

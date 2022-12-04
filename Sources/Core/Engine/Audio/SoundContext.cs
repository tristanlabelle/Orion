using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using OpenTK.Math;
using IrrKlang;
using System.Text.RegularExpressions;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Provides means of playing 3D sound using a sound driver.
    /// </summary>
    public sealed class SoundContext : IDisposable
    {
        #region Fields
        private readonly Random random = new Random();
        private readonly ISoundEngine irrKlangEngine;
        private readonly bool isUsingNullDriver;
        private readonly Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
        private readonly Dictionary<string, SoundGroup> soundGroups = new Dictionary<string, SoundGroup>();
        private Vector3 listenerPosition;
        private Vector3 listenerDirection;
        private float volume = 1;
        private bool isMuted;
        #endregion

        #region Constructors
        public SoundContext()
        {
            bool succeeded;
            this.irrKlangEngine = CreateIrrKlangEngine(out succeeded);
            this.isUsingNullDriver = !succeeded;
            this.listenerPosition = Vector3.Zero;
            this.listenerDirection = new Vector3(0, 0, 1);

            Debug.WriteLine("Using {0} sound device".FormatInvariant(irrKlangEngine.Name));
        }
        #endregion

        #region Events
        public event Action<SoundContext> ListenerPositionChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the driver being used.
        /// </summary>
        public string DriverName
        {
            get { return irrKlangEngine.Name; }
        }

        /// <summary>
        /// Gets a value indicating if we're using a dummy driver because sound wasn't available.
        /// </summary>
        public bool IsUsingNullDriver
        {
            get { return isUsingNullDriver; }
        }

        /// <summary>
        /// Gets the global sound volume in this <see cref="AudioContext"/>.
        /// </summary>
        public float Volume
        {
            get { return volume; }
            set
            {
                if (!isMuted) irrKlangEngine.SoundVolume = value;
                volume = value;
            }
        }

        /// <summary>
        /// Gets a value indicating if sounds in this <see cref="AudioContext"/> are muted.
        /// </summary>
        public bool IsMuted
        {
            get { return isMuted; }
            set
            {
                isMuted = value;
                irrKlangEngine.SoundVolume = isMuted ? 0 : volume;
            }
        }

        /// <summary>
        /// Accesses the location of the listener in the 3D world.
        /// </summary>
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

        /// <summary>
        /// Accesses the direction vector of the sound listener.
        /// </summary>
        public Vector3 ListenerDirection
        {
            get { return listenerDirection; }
            set
            {
                if (value.LengthSquared < 0.001f) throw new ArgumentOutOfRangeException("The listener direction vector must be non-zero.");
                listenerDirection = Vector3.NormalizeFast(value);
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
        /// <summary>
        /// Loads a sound from its name.
        /// </summary>
        /// <param name="name">The name of the sound to be loaded.</param>
        /// <returns>The sound that was loaded, or null.</returns>
        public Sound GetSound(string name)
        {
            Argument.EnsureNotNull(name, "name");

            Sound sound;
            if (sounds.TryGetValue(name, out sound))
                return sound;

            string filePath = Directory.GetFiles("Assets/Sounds/", name + ".*", SearchOption.TopDirectoryOnly)
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

            IEnumerable<Sound> sounds = null;
            try
            {
                sounds = Directory.GetFiles("Assets/Sounds/", name + ".*")
                    .Select(filePath => Path.GetFileNameWithoutExtension(filePath))
                    .Select(soundName => GetSound(soundName))
                    .Where(sound => sound != null);
            }
            catch (DirectoryNotFoundException)
            {
                sounds = Enumerable.Empty<Sound>();
            }

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

        /// <summary>
        /// Creates a new sound source in this context.
        /// </summary>
        /// <returns>A newly created source.</returns>
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

        public void PlayAndForgetRandomSoundFromGroup(string groupName, Vector2? position)
        {
            Sound sound = GetRandomSoundFromGroup(groupName);
            if (sound == null) return;

            PlayAndForget(sound, position);
        }

        /// <summary>
        /// Releases all resources used by this sound context.
        /// </summary>
        public void Dispose()
        {
            irrKlangEngine.StopAllSounds();

            soundGroups.Clear();

            foreach (Sound sound in sounds.Values)
                sound.Dispose();
            sounds.Clear();

            irrKlangEngine.RemoveAllSoundSources();
            CallDtor(irrKlangEngine);
        }

        private static ISoundEngine CreateIrrKlangEngine(out bool succeeded)
        {
            ISoundDeviceList deviceList = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice);
            try
            {
#if DEBUG
                Debug.WriteLine(deviceList.DeviceCount.ToStringInvariant() + " available sound devices");
                for (int i = 0; i < deviceList.DeviceCount; ++i)
                    Debug.WriteLine(i.ToStringInvariant() + ": " + deviceList.getDeviceDescription(i));
#endif

                if (deviceList.DeviceCount > 0)
                {
                    try
                    {
                        succeeded = true;
                        return new ISoundEngine(SoundOutputDriver.AutoDetect,
                            SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MultiThreaded);

                    }
                    catch (Exception) { } // We have to catch Exception as that's what's thrown by IrrKlang >.<
                }

                succeeded = false;
                return new ISoundEngine(SoundOutputDriver.NullDriver);
            }
            catch
            {
                CallDtor(deviceList);
                throw;
            }
        }

        private void UpdateIrrKlangListener()
        {
            Vector3D position = new Vector3D(listenerPosition.X, listenerPosition.Y, listenerPosition.Z);
            Vector3D direction = new Vector3D(listenerDirection.X, listenerDirection.Y, listenerDirection.Z);
            irrKlangEngine.SetListenerPosition(position, direction);
        }

        private static void CallDtor(object obj)
        {
            // IrrKlang 1.1 had __dtor() methods. Things got weirder in 1.3.
            obj.GetType().GetMethod("{dtor}").Invoke(obj, null);
        }
        #endregion
    }
}

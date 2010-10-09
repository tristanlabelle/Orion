using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using OpenTK;
using IrrKlang;
using System.Text.RegularExpressions;

namespace Orion.Engine.Audio.IrrKlang
{
    /// <summary>
    /// Provides means of playing 3D sound using a sound driver.
    /// </summary>
    public sealed class SoundContext : ISoundContext
    {
        #region Instance
        #region Fields
        private readonly Random random = new Random();
        private readonly ISoundEngine irrKlangSoundEngine;
        private Matrix4 listenerMatrix = Matrix4.Identity;
        private float volume = 1;
        private bool isMuted;
        #endregion

        #region Constructors
        internal SoundContext(SoundOutputDriver driver)
        {
            try
            {
                this.irrKlangSoundEngine = new ISoundEngine(driver,
                    SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MultiThreaded);
            }
            catch (Exception) // Sadly, IrrKlang.NET throws nothing more precise
            {
                throw new NotSupportedException("IrrKlang {0} driver is not supported.".FormatInvariant(driver));
            }

            Debug.WriteLine("Using IrrKlang's {0} sound device.".FormatInvariant(irrKlangSoundEngine.Name));
        }

        public SoundContext() : this(SoundOutputDriver.AutoDetect) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the driver being used.
        /// </summary>
        public string DriverName
        {
            get { return irrKlangSoundEngine.Name; }
        }

        /// <summary>
        /// Gets the global sound volume in this <see cref="AudioContext"/>.
        /// </summary>
        public float Volume
        {
            get { return volume; }
            set
            {
                if (!isMuted) irrKlangSoundEngine.SoundVolume = value;
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
                irrKlangSoundEngine.SoundVolume = isMuted ? 0 : volume;
            }
        }

        /// <summary>
        /// Accesses the matrix representing the listener's position and orientation.
        /// </summary>
        public Matrix4 ListenerMatrix
        {
            get { return listenerMatrix; }
            set
            {
                irrKlangSoundEngine.SetListenerPosition(
                    ToIrrKlangVector(value.Row3.Xyz),
                    ToIrrKlangVector(value.Row2.Xyz),
                    ToIrrKlangVector(Vector3.Zero),
                    ToIrrKlangVector(value.Row1.Xyz));

                listenerMatrix = value;
            }
        }

        internal ISoundEngine IrrKlangEngine
        {
            get { return irrKlangSoundEngine; }
        }

        internal Vector3D IrrKlangListenerPosition
        {
            get { return ToIrrKlangVector(listenerMatrix.Row3.Xyz); }
        }
        #endregion

        #region Methods
        public ISound LoadSoundFromFile(string filePath)
        {
            try
            {
                ISoundSource irrKlangSoundSource = irrKlangSoundEngine.AddSoundSourceFromFile(filePath, StreamMode.AutoDetect, true);
                return new Sound(irrKlangSoundSource);
            }
            catch (Exception)
            {
                throw new IOException("Error while reading file \"{0}\"".FormatInvariant(filePath));
            }
        }

        public ISoundChannel CreateChannel()
        {
            return new SoundChannel(irrKlangSoundEngine);
        }

        /// <summary>
        /// Releases all resources used by this sound context.
        /// </summary>
        public void Dispose()
        {
            irrKlangSoundEngine.StopAllSounds();
            irrKlangSoundEngine.RemoveAllSoundSources();
            irrKlangSoundEngine.__dtor();
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Creates a null IrrKlang sound context.
        /// </summary>
        /// <returns>The null sound context that was created.</returns>
        public static SoundContext CreateNull()
        {
            return new SoundContext(SoundOutputDriver.NullDriver);
        }

        internal static Vector3D ToIrrKlangVector(Vector3 vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }
        #endregion
        #endregion
    }
}

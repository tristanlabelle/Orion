﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Audio;
using Orion.Game.Simulation;
using OpenALSoundContext = Orion.Engine.Audio.OpenAL.SoundContext;
using IrrKlangSoundContext = Orion.Engine.Audio.IrrKlang.SoundContext;
using NullSoundContext = Orion.Engine.Audio.Null.SoundContext;
using System.Diagnostics;

namespace Orion.Game.Presentation.Audio
{
    /// <summary>
    /// Central point of access to the audio resources used by the game.
    /// </summary>
    public sealed class GameAudio : IDisposable
    {
        #region Fields
        private readonly ISoundContext soundContext;
        private readonly ISoundChannel sfxChannel;
        private readonly ISoundChannel uiChannel;
        private readonly SoundManager soundManager;
        private readonly Random random = new Random();

        /// <summary>
        /// Reused between calls to minimize object garbage.
        /// </summary>
        private readonly StringBuilder stringBuilder = new StringBuilder();
        #endregion

        #region Constructors
        public GameAudio()
        {
            try
            {
                soundContext = new OpenALSoundContext();
            }
            catch (Exception exception1)
            {
                Debug.WriteLine("Failed to create an OpenAL sound context: " + exception1.Message);
                try
                {
                    soundContext = new IrrKlangSoundContext();
                }
                catch (Exception exception2)
                {
                    Debug.WriteLine("Failed to create an IrrKlang sound context: " + exception2.Message);
                    soundContext = new NullSoundContext();
                }
            }

            sfxChannel = soundContext.CreateChannel();
            uiChannel = soundContext.CreateChannel();
            uiChannel.Volume = 0.5f;

            soundManager = new SoundManager(soundContext, "../../../Assets/Sounds");
        }
        #endregion

        #region Properties
        public float SfxVolume
        {
            get { return sfxChannel.Volume; }
            set { sfxChannel.Volume = value; }
        }

        public float UIVolume
        {
            get { return uiChannel.Volume; }
            set { uiChannel.Volume = value; }
        }

        public Vector3 ListenerPosition
        {
            get { return soundContext.ListenerMatrix.Row3.Xyz; }
            set
            {
                soundContext.ListenerMatrix = new Matrix4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    value.X, value.Y, value.Z, 1);
            }
        }
        #endregion

        #region Methods
        public string GetUnitSoundName(UnitType unitType, string eventName)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            Argument.EnsureNotNull(eventName, "eventName");

            stringBuilder.Clear();
            stringBuilder.Append(unitType.VoicesTemplate);
            stringBuilder.Append('.');
            stringBuilder.Append(eventName);

            return stringBuilder.ToString();
        }

        public void PlaySfx(string name, Vector2? position)
        {
            ISound sound = soundManager.GetRandom(name, random);
            if (sound == null) return;
            
            Vector3? position3D = position.HasValue ? (Vector3?)new Vector3(position.Value) : null;
            sfxChannel.Play(sound, position3D);
        }

        public void PlayUISound(string name)
        {
            uiChannel.StopAllSounds();

            ISound sound = soundManager.GetRandom(name, random);
            if (sound == null) return;
            uiChannel.Play(sound, null);
        }

        public void Dispose()
        {
            sfxChannel.Dispose();
            uiChannel.Dispose();
            soundManager.Dispose();
            soundContext.Dispose();
        }
        #endregion
    }
}

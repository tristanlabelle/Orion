using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Represents a transformation that is applied to a sound.
    /// </summary>
    public struct SoundTransform
    {
        #region Instance
        #region Fields
        /// <remarks>
        /// As value types are zero-initialized and a default volume of 1 makes more sense,
        /// this is stored upside-down, with 0 being the loudest and 1 being the quietest.
        /// </remarks>
        private float volume;

        private Vector3? position;
        #endregion

        #region Constructors
        public SoundTransform(float volume, Vector3? position)
        {
            Argument.EnsureWithin(volume, 0, 1, "volume");

            this.volume = 1 - volume;
            this.position = position;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the volume of this transform, in range [0, 1].
        /// </summary>
        public float Volume
        {
            get { return 1 - volume; }
            set
            {
                Argument.EnsureWithin(value, 0, 1, "Volume");
                volume = 1 - value;
            }
        }

        /// <summary>
        /// Accesses the position transform of this sound in 3D space.
        /// A value of <c>null</c> indicates that the sound should not be 3D positioned.
        /// </summary>
        public Vector3? Position
        {
            get { return position; }
            set { position = value; }
        }
        #endregion
        #endregion

        #region Static
        #region Properties
        /// <summary>
        /// Gets the identity sound transform, which represents a full-volume, non positioned sound.
        /// </summary>
        public static SoundTransform Identity
        {
            get { return new SoundTransform(); }
        }
        #endregion
        #endregion
    }
}

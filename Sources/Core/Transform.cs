using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion
{
    /// <summary>
    /// Encapsulates a 2D transformation.
    /// </summary>
    [Serializable]
    public struct Transform
    {
        #region Fields
        private readonly Vector2 translation;
        private readonly float rotation;
        private readonly Vector2 scaling;
        #endregion

        #region Constructors
        public Transform(Vector2 translation, float rotation, Vector2 scaling)
        {
            this.translation = translation;
            this.rotation = rotation;
            this.scaling = scaling;
        }

        public Transform(Vector2 translation, float rotation)
        {
            this.translation = translation;
            this.rotation = rotation;
            this.scaling = new Vector2(1, 1);
        }
        #endregion

        #region Properties
        public Vector2 Translation
        {
            get { return translation; }
        }

        public float Rotation
        {
            get { return rotation; }
        }

        public Vector2 Scaling
        {
            get { return scaling; }
        } 
        #endregion
    }
}

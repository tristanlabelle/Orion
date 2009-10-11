using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.Graphics
{
    class ShapePath
    {
        #region Fields

        public List<Vector2> Points;

        #endregion

        #region Constructors

        public ShapePath()
        {
            Points = new List<Vector2>();
        }

        #endregion

        #region Properties
        
        public int Count
        {
            get { return Points.Count; }
        }

        #endregion

        #region Methods

        public void AddPoint(Vector2 Point)
        {
            Points.Add(Point);
        }

        public Vector2 GetPointAt(int index)
        {
            return Points.ElementAt(index); 
        }

        #endregion
    }
}

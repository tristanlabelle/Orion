using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.Graphics
{
    public class ShapePath
    {
        #region Fields

        private readonly List<Vector2> points;

        #endregion

        #region Constructors

        public ShapePath()
        {
            points = new List<Vector2>();
        }

        #endregion

        #region Properties
        
        public int Count
        {
            get { return points.Count; }
        }

        #endregion

        #region Methods

        public void AddPoint(Vector2 Point)
        {
            points.Add(Point);
        }

        public Vector2 GetPointAt(int index)
        {
            return points.ElementAt(index); 
        }

        #endregion
    }
}

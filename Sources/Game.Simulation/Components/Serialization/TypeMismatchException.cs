using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components.Serialization
{
    /// <summary>
    /// Thrown internally when an object is inappropriately deserialized.
    /// </summary>
    internal class TypeMismatchException : Exception
    {
        #region Fields
        private Type expectedType;
        private string data;
        #endregion

        #region Constructors
        public TypeMismatchException(Type expectedType, string data)
        {
            this.data = data;
            this.expectedType = expectedType;
        }
        #endregion

        #region Properties
        public Type ExpectedType
        {
            get { return expectedType; }
        }

        public new string Data
        {
            get { return data; }
        }
        #endregion
    }
}

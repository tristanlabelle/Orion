using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents a faction, a group of allied units sharing resources and sharing a goal.
    /// </summary>
    [Serializable]
    public sealed class Faction
    {
        #region Fields
        private string name;
        private Color color = Color.Blue;
        private int aladdiumAmount;
        private int allageneAmount;
        #endregion

        #region Constructors
        #endregion

        #region Events
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the name of this <see cref="Faction"/>.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNullNorBlank(value, "Name");
                name = value.Trim();
            }
        }

        /// <summary>
        /// Accesses the <see cref="Color"/> used to visually identify units of this <see cref="Faction"/>.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = Color.FromArgb(value.R, value.G, value.B); }
        }

        /// <summary>
        /// Accesses the amount of the aladdium resource that this <see cref="Faction"/> possesses.
        /// </summary>
        public int AladdiumAmount
        {
            get { return aladdiumAmount; }
            set
            {
                Argument.EnsurePositive(value, "AladdiumAmount");
                aladdiumAmount = value;
            }
        }

        /// <summary>
        /// Accesses the amount of the allagene resource that this <see cref="Faction"/> possesses.
        /// </summary>
        public int AllageneAmount
        {
            get { return allageneAmount; }
            set
            {
                Argument.EnsurePositive(value, "AllageneAmount");
                allageneAmount = value;
            }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}

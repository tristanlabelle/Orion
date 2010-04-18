using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Provides the information needed to draw a chart.
    /// </summary>
    public abstract class ChartModel
    {
        #region Properties
        /// <summary>
        /// Gets the title of the chart.
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// Gets the number of categories that are represented in this chart.
        /// </summary>
        public abstract int CategoryCount { get; }

        /// <summary>
        /// Gets the number of entries for each categaries in this chart.
        /// </summary>
        public abstract int EntryCount { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a label that describes the category at the given index.
        /// </summary>
        /// <param name="index">The index of the category.</param>
        /// <returns>A label that describes it.</returns>
        public abstract string GetCategoryLabel(int index);

        /// <summary>
        /// Gets the color used to display a category.
        /// </summary>
        /// <param name="index">The index of the category.</param>
        /// <returns>A color to be used to display that category.</returns>
        public abstract ColorRgb GetCategoryColor(int index);

        /// <summary>
        /// Gets a label that describes an entry of the chart.
        /// </summary>
        /// <param name="index">The index of the entry.</param>
        /// <returns>A label describing that entry.</returns>
        public abstract string GetEntryLabel(int index);

        /// <summary>
        /// Gets a value in the chart for a category at a given entry.
        /// </summary>
        /// <param name="categoryIndex">The index of the category.</param>
        /// <param name="entryIndex">The index of the entry.</param>
        /// <returns>The value for that category and that entry.</returns>
        public abstract float GetValue(int categoryIndex, int entryIndex);

        /// <summary>
        /// Gets the range of the values in the entries of this chart.
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        public virtual void GetValuesRange(out float minimum, out float maximum)
        {
            minimum = GetValue(0, 0);
            maximum = minimum;

            for (int categoryIndex = 0; categoryIndex < CategoryCount; ++categoryIndex)
            {
                for (int entryIndex = 0; entryIndex < EntryCount; ++entryIndex)
                {
                    float value = GetValue(categoryIndex, entryIndex);
                    if (value < minimum) minimum = value;
                    else if (value > maximum) maximum = value;
                }
            }
        }

        /// <summary>
        /// Gets a value in the chart for a category at a given entry,
        /// normalized with regard to the values for other categories in the same entry.
        /// </summary>
        /// <param name="categoryIndex">The index of the category.</param>
        /// <param name="entryIndex">The index of the entry.</param>
        /// <returns>The normalized value for that category and that entry.</returns>
        public virtual float GetNormalizedValue(int categoryIndex, int entryIndex)
        {
            float value = GetValue(categoryIndex, entryIndex);

            float sum = value;
            for (int i = 0; i < CategoryCount; ++i)
            {
                if (i == categoryIndex) continue;
                sum += GetValue(i, entryIndex);
            }

            return sum == 0 ? 0 : value / sum;
        }
        #endregion
    }
}

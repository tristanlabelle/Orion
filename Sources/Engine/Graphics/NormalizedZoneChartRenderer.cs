using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using OpenTK.Math;
using Orion.Engine.Geometry;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Draws charts in normalized zone format. For each entry, the values for the categories
    /// are divided by their sum and then stacked.
    /// </summary>
    public sealed class NormalizedZoneChartRenderer
    {
        #region Fields
        private readonly ChartModel model;
        private readonly int maxSampleCount;
        private readonly Vector2[][] categoryTriangleStripVertices;
        #endregion

        #region Constructors
        public NormalizedZoneChartRenderer(ChartModel model, int maxSampleCount)
        {
            Argument.EnsureNotNull(model, "model");
            Argument.EnsureStrictlyPositive(maxSampleCount, "maxSampleCount");

            this.model = model;
            this.maxSampleCount = maxSampleCount;

            int sampleCount = Math.Min(maxSampleCount, model.EntryCount);

            this.categoryTriangleStripVertices = new Vector2[model.CategoryCount][];
            for (int i = 0; i < model.CategoryCount; ++i)
                categoryTriangleStripVertices[i] = new Vector2[sampleCount * 2];

            for (int sampleIndex = 0; sampleIndex < sampleCount; ++sampleIndex)
            {
                int entryIndex = (int)(sampleIndex / (float)sampleCount * model.EntryCount);

                float sum = 0;
                for (int categoryIndex = 0; categoryIndex < model.CategoryCount; ++categoryIndex)
                {
                    float value = model.GetValue(categoryIndex, entryIndex);
                    sum += value;
                    categoryTriangleStripVertices[categoryIndex][sampleIndex * 2].X = sum;
                }

                if (sum == 0) sum = float.PositiveInfinity;

                float x = sampleIndex / (float)(sampleCount - 1);
                for (int categoryIndex = 0; categoryIndex < model.CategoryCount; ++categoryIndex)
                {
                    var vertices = categoryTriangleStripVertices[categoryIndex];
                    float partialSum = vertices[sampleIndex * 2].X;
                    vertices[sampleIndex * 2] = new Vector2(x, partialSum / sum);
                    vertices[sampleIndex * 2 + 1] = new Vector2(x, 0);
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            Argument.EnsureNotNull(context, "context");

            using (context.PushTransform(bounds.Min, 0, bounds.Size))
            {
                for (int categoryIndex = model.CategoryCount - 1; categoryIndex >= 0; --categoryIndex)
                {
                    var vertices = categoryTriangleStripVertices[categoryIndex];
                    var color = model.GetCategoryColor(categoryIndex);
                    context.FillTriangleStrip(vertices, color);
                }
            }
        }
        #endregion
    }
}

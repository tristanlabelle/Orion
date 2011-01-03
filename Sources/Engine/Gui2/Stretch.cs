using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Describes how content is resized to fill its allocated space.
    /// </summary>
    public enum Stretch
    {
        /// <summary>
        /// Specifies that the content keeps its original size no matter the available space.
        /// If the available space is smaller than the content, it will be clipped.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that the content is resized uniformly to preserve its aspect ratio
        /// while fitting in the available space without being clipped. If the aspect ratio
        /// of the available space does not match the content's, some of the space will be unused.
        /// </summary>
        Uniform,

        /// <summary>
        /// Specifies that the content is resized uniformly to fill the allocated space.
        /// If the aspect ratio of the available space does not match the content's,
        /// part of the content will be clipped.
        /// </summary>
        UniformToFill,

        /// <summary>
        /// Specifies that the content is resized non-uniformly to fill the available space.
        /// </summary>
        Fill
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK;

namespace Orion.Graphics
{
    /// <summary>
    /// A custom version of OpenTK's <see cref="GLControl"/>
    /// which properly raises key events.
    /// </summary>
    public sealed class CustomGLControl : GLControl
    {
        protected override bool IsInputChar(char charCode)
        {
            return true;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }
    }
}

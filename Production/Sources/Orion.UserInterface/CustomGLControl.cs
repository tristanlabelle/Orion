using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK;

namespace Orion.UserInterface
{
    /// <summary>
    /// A custom version of OpenTK's <see cref="GLControl"/>
    /// which properly raises key events.
    /// </summary>
    public sealed class CustomGLControl : GLControl
    {
        /// <summary>Indicates if a character should generate a keyboard evet.</summary>
        /// <param name="charCode">The resulting <see cref="System.Char"/> of the previous keystrokes</param>
        /// <returns>Always true</returns>
        protected override bool IsInputChar(char charCode)
        {
            return true;
        }

        /// <summary>
        /// Indicates if a specific key should trigger a keyboard event.
        /// </summary>
        /// <param name="keyData">A <see cref="Keys"/></param>
        /// <returns>Always true</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }
    }
}

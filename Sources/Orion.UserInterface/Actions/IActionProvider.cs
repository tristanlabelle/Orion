using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.UserInterface.Actions
{
    public interface IActionProvider
    {
        ActionButton GetButtonAt(int x, int y);
    }
}

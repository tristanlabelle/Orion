using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.UserInterface
{
    public interface IActionProvider
    {
        ActionButton this[int x, int y] { get; }
    }
}

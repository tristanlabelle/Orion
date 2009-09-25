using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    public delegate void GenericEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);
}

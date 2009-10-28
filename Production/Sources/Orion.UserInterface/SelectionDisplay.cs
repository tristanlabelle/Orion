using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment;
using Orion.Geometry;
using Orion.GameLogic;

namespace Orion.UserInterface
{
    public class SelectionDisplay : View
    {

        public SelectionDisplay(Rectangle frame)
            : base(frame)
        { }

        public void SelectionChanged(SelectionManager manager)
        {
            Children.Clear();
            foreach (Unit unit in manager.SelectedUnits)
            {

            }
        }

        protected internal override void Draw(Orion.Graphics.GraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}

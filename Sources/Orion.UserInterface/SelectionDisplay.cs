using System;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Geometry;

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

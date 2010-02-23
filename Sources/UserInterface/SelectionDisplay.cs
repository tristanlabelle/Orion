using System;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Matchmaking;

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

        protected internal override void Draw(GraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}

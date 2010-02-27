using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics
{
    public class DropdownListRowValueRenderer<T>
    {
        public virtual void Draw(T value, GraphicsContext context, Rectangle bounds)
        {
            context.Draw(value.ToString(), bounds.Min);
        }
    }

    public class DropdownListRowColorRenderer : DropdownListRowValueRenderer<ColorRgb>
    {
        public override void Draw(ColorRgb color, GraphicsContext context, Rectangle bounds)
        {
            context.FillColor = color;
            context.Fill(bounds.TranslatedBy(1, 1).ResizedBy(-2, -2));
        }
    }

    public class DropdownListRowDiplomaticStanceRenderer : DropdownListRowValueRenderer<DiplomaticStance>
    {
        private static Dictionary<DiplomaticStance, string> stances = new Dictionary<DiplomaticStance,string>();

        static DropdownListRowDiplomaticStanceRenderer()
        {
            stances[DiplomaticStance.Ally] = "Allié";
            stances[DiplomaticStance.Enemy] = "Ennemi";
        }

        public override void Draw(DiplomaticStance stance, GraphicsContext context, Rectangle bounds)
        {
            context.Draw(stances[stance], bounds.Min);
        }
    }
}

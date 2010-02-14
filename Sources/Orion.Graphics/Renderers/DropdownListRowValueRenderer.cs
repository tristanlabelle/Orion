using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;

namespace Orion.Graphics
{
    public class DropdownListRowValueRenderer<T>
    {
        public virtual void Draw(T value, GraphicsContext context)
        {
            context.Draw(value.ToString());
        }
    }

    public class DropdownListRowColorRenderer : DropdownListRowValueRenderer<ColorRgb>
    {
        public override void Draw(ColorRgb color, GraphicsContext context)
        {
            context.FillColor = color;
            context.Fill(context.CoordinateSystem.TranslatedBy(1, 1).ResizedBy(-2, -2));
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

        public override void Draw(DiplomaticStance stance, GraphicsContext context)
        {
            context.Draw(stances[stance]);
        }
    }
}

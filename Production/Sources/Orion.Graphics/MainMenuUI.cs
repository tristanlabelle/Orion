using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

using Orion.Geometry;
using Orion.Graphics.Widgets;

namespace Orion.Graphics
{
    public class MainMenuUI : UIDisplay
    {
        public MainMenuUI()
        {
            Button singlePlayerGame = new Button(new Rectangle(40, 10), "Single Player Game");
            singlePlayerGame.Frame = new Rectangle(300, 400, 400, 100);
            Children.Add(singlePlayerGame);
        }

        internal override void OnEnter(RootView enterOn)
        { }

        internal override void OnShadow(RootView hiddenOf)
        { }

        protected override void Draw()
        {
            context.FillColor = Color.DarkBlue;
            context.Fill(Bounds);
        }
    }
}

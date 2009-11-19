using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;
using Color = System.Drawing.Color;
using Orion.Geometry;
using Orion.Graphics;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class TextField : View
    {
        #region Field
        private const int cursorBlinkFrequency = 30;

        private Text contents;
        private int updateCounter;
        #endregion

        #region Constructors
        public TextField(Rectangle frame)
            : base(frame)
        {
            contents = new Text("");
        }
        #endregion

        #region Properties
        public string Contents
        {
            get { return contents.Value; }
            set { contents = new Text(value); }
        }
        #endregion

        #region Methods
        public void Clear()
        {
            contents = new Text("");
        }

        protected override bool OnKeyPress(char arg)
        {
            if (arg == '\b')
            {
                string value = Contents;
                if (value.Length > 0)
                    Contents = value.Remove(value.Length - 1);
            }
            else
                Contents += arg;
            base.OnKeyPress(arg);
            return false;
        }

        protected override void OnUpdate(UpdateEventArgs args)
        {
            updateCounter++;
            base.OnUpdate(args);
        }

        protected internal override void Draw(GraphicsContext context)
        {
            context.FillColor = Color.LightGreen;
            context.StrokeColor = Color.Gray;
            context.Fill(Bounds);
            context.Stroke(Bounds);

            context.FillColor = Color.Black;
            context.Draw(contents);
            if ((updateCounter / cursorBlinkFrequency) % 2 == 0)
            {
                Rectangle textFrame = contents.Frame;
                context.StrokeLineStrip(textFrame.Max, new Vector2(textFrame.MaxX, textFrame.MinY));
            }
        }
        #endregion
    }
}

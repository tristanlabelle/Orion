using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;
using Orion.Graphics;

using OpenTK.Math;

namespace Orion.UserInterface
{
    public abstract class View : Responder
    {
        #region Fields
        private GraphicsContext context;
        #endregion

        #region Constructors
        public View(Rectangle rectangle)
        {
            context = new GraphicsContext(new Rectangle(rectangle.Size));
            Frame = rectangle;
        }
        #endregion

        #region Properties

        public new ViewChildrenCollection Children
        {
            get { return base.Children as ViewChildrenCollection; }
        }

        public override Rectangle Bounds
        {
            get { return context.CoordinateSystem; }
            set
            {
                context.CoordinateSystem = value;
                if (IsMouseOver)
                {
                    Vector2 position = CursorPosition.Value;
                    PropagateMouseEvent(MouseEventType.MouseMoved, new MouseEventArgs(position.X, position.Y, MouseButton.None, 0, 0));
                }
            }
        }
        #endregion

        #region Methods
        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 coords = args.Position;
            coords -= Frame.Origin;
            coords.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);
            coords += Bounds.Origin;

            return base.PropagateMouseEvent(eventType, new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
        }

        protected internal override bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            return base.PropagateKeyboardEvent(type, args);
        }

        protected internal override sealed void Render()
        {
            context.SetUpGLContext(Frame);

            Draw(context);
            base.Render();
            context.RestoreGLContext();
        }

        protected internal abstract void Draw(GraphicsContext context);
        #endregion
    }
}

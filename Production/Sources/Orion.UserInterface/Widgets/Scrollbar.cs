using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;
using Orion.Geometry;
using Orion.Graphics;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class Scrollbar : Frame
    {
        #region Fields
        private static readonly Triangle upTriangle = new Triangle(new Vector2(0.2f, 0.2f), new Vector2(0.5f, 0.7f), new Vector2(0.8f, 0.2f));
        private static readonly Triangle downTriangle = new Triangle(new Vector2(0.2f, 0.7f), new Vector2(0.5f, 0.2f), new Vector2(0.8f, 0.7f));

        private readonly Frame topArrow;
        private readonly Frame bottomArrow;
        private readonly Frame slider;

        private float? mouseDownPosition;

        public readonly ClippedView Scrollee;
        #endregion

        #region Constructors
        public Scrollbar(Rectangle frame, ClippedView scrollee)
            : base(frame, Color.FromArgb(0x80, Color.Gray))
        {
            Frame = frame;
            Scrollee = scrollee;

            Rectangle arrowRect = new Rectangle(frame.Width, frame.Width);
            bottomArrow = new Frame(arrowRect, new DelegatedRenderer(RenderBottomArrow));
            topArrow = new Frame(arrowRect.TranslatedBy(0, frame.Height - frame.Width), new DelegatedRenderer(RenderTopArrow));
            topArrow.Bounds = new Rectangle(1, 1);
            bottomArrow.Bounds = new Rectangle(1, 1);
            slider = new Frame(new Rectangle(frame.Width, 1), Color.Orange);

            slider.MouseDown += SliderMouseDown;
            topArrow.MouseDown += MoveUp;
            bottomArrow.MouseDown += MoveDown;
            Scrollee.MouseWheel += ScrolleeMouseWheel;
            Scrollee.BoundsChanged += RegenerateScrollbar;
            Scrollee.FullBoundsChanged += RegenerateScrollbar;
            RegenerateScrollbar();

            Children.Add(bottomArrow);
            Children.Add(topArrow);
            Children.Add(slider);
        }
        #endregion

        #region Methods
        private void RegenerateScrollbar()
        {
            float arrowsSize = topArrow.Frame.Height / Bounds.Height;
            float visiblePortionStart = Scrollee.Bounds.MaxY / Scrollee.FullBounds.Height;
            float sliderSize = Scrollee.Bounds.Height / Scrollee.FullBounds.Height;
            if (visiblePortionStart > 1) visiblePortionStart = 1;
            if (sliderSize > 1) sliderSize = 1;
            Vector2 sliderOrigin = new Vector2(0, visiblePortionStart - sliderSize + arrowsSize);
            Vector2 sliderEnd = new Vector2(1, visiblePortionStart - arrowsSize);
            slider.Frame = Instant.CreateComponentRectangle(Bounds, sliderOrigin, sliderEnd);
        }

        #region Event Handling
        private void SliderMouseDown(Responder sender, MouseEventArgs args)
        {
            mouseDownPosition = args.Position.Y;
        }

        private void SliderMouseUp(Responder sender, MouseEventArgs args)
        {
            mouseDownPosition = null;
        }

        private void ScrolleeMouseWheel(Responder sender, MouseEventArgs args)
        {
            ScrollBy(args.WheelDelta / 600.0f);
        }

        private void RegenerateScrollbar(View sender, Rectangle newFullBounds)
        {
            RegenerateScrollbar();
        }

        private void MoveUp(Responder sender, MouseEventArgs args)
        {
            ScrollBy(slider.Frame.Height / Frame.Height / 2);
        }

        private void MoveDown(Responder sender, MouseEventArgs args)
        {
            ScrollBy(-slider.Frame.Height / Frame.Height / 2);
        }
        
        protected override bool OnMouseMove(MouseEventArgs args)
        {
            if (mouseDownPosition.HasValue)
            {
                float mouseOffset = args.Y - mouseDownPosition.Value;
                ScrollBy(mouseOffset / Bounds.Height);
                mouseDownPosition = args.Y;
            }
            return base.OnMouseMove(args);
        }

        private void ScrollBy(float offset)
        {
            Scrollee.ScrollBy(0, offset * Scrollee.Bounds.Height);
        }
        #endregion

        #region Drawing
        private void RenderBottomArrow(GraphicsContext context)
        {
            DrawScrollbarEnd(context, downTriangle);
        }

        private void RenderTopArrow(GraphicsContext context)
        {
            DrawScrollbarEnd(context, upTriangle);
        }

        private void DrawScrollbarEnd(GraphicsContext context, Triangle fillMe)
        {
            context.StrokeColor = Color.Black;
            context.FillColor = Color.Gray;

            context.Fill(context.CoordinateSystem);
            context.Stroke(context.CoordinateSystem);

            context.FillColor = Color.Black;
            context.Fill(fillMe);
        }
        #endregion
        #endregion
    }
}

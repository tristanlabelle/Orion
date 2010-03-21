using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using Orion.Graphics;
using Orion.Graphics.Renderers;

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
            : base(frame, new ColorRgba(Colors.Gray, 0.5f))
        {
            Frame = frame;
            Scrollee = scrollee;

            Rectangle arrowRect = new Rectangle(frame.Width, frame.Width);
            bottomArrow = new Frame(arrowRect, new DelegatedRenderer(RenderBottomArrow));
            topArrow = new Frame(arrowRect.TranslatedBy(0, frame.Height - frame.Width), new DelegatedRenderer(RenderTopArrow));
            topArrow.Bounds = new Rectangle(1, 1);
            bottomArrow.Bounds = new Rectangle(1, 1);
            slider = new Frame(new Rectangle(frame.Width, 1), Colors.Orange);

            slider.MouseButtonPressed += SliderMouseDown;
            topArrow.MouseButtonPressed += MoveUp;
            bottomArrow.MouseButtonPressed += MoveDown;
            Scrollee.MouseWheelScrolled += OnMouseWheelScrolled;
            Scrollee.BoundsChanged += RegenerateScrollbar;
            Scrollee.FullBoundsChanged += RegenerateScrollbar;
            RegenerateScrollbarFrame();

            Children.Add(bottomArrow);
            Children.Add(topArrow);
            Children.Add(slider);
        }
        #endregion

        #region Methods
        private void RegenerateScrollbarFrame()
        {
            float arrowHeight = topArrow.Frame.Height / Bounds.Height;
            float maximumSliderHeight = 1 - arrowHeight * 2;
            float invisibleHeight = Scrollee.Bounds.MinY - Scrollee.FullBounds.MinY;
            float visibleHeight = Scrollee.Bounds.Height;

            float sliderOrigin = arrowHeight + invisibleHeight / Scrollee.FullBounds.Height * maximumSliderHeight;
            float sliderHeight = visibleHeight / Scrollee.FullBounds.Height * maximumSliderHeight;
            if (sliderOrigin < arrowHeight) sliderOrigin = arrowHeight;
            if (sliderHeight > maximumSliderHeight) sliderHeight = maximumSliderHeight;
            slider.Frame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, sliderOrigin), new Vector2(1, sliderOrigin + sliderHeight));
        }

        #region Event Handling
        private void SliderMouseDown(Responder sender, MouseEventArgs args)
        {
            mouseDownPosition = args.Position.Y;
        }

        private void OnMouseWheelScrolled(Responder sender, MouseEventArgs args)
        {
            ScrollBy(args.WheelDelta);
        }

        private void RegenerateScrollbar(View sender, Rectangle newFullBounds)
        {
            RegenerateScrollbarFrame();
        }

        private void MoveUp(Responder sender, MouseEventArgs args)
        {
            ScrollBy(slider.Frame.Height / Frame.Height / 10);
        }

        private void MoveDown(Responder sender, MouseEventArgs args)
        {
            ScrollBy(-slider.Frame.Height / Frame.Height / 10);
        }

        protected override bool OnMouseButtonReleased(MouseEventArgs args)
        {
            mouseDownPosition = null;
            return base.OnMouseButtonReleased(args);
        }
        
        protected override bool OnMouseMoved(MouseEventArgs args)
        {
            if (mouseDownPosition.HasValue)
            {
                float mouseOffset = args.Y - mouseDownPosition.Value;
                ScrollBy(mouseOffset / Bounds.Height);
                mouseDownPosition = args.Y;
            }
            return base.OnMouseMoved(args);
        }

        private void ScrollBy(float offset)
        {
            Scrollee.ScrollBy(0, offset * Scrollee.Bounds.Height);
        }
        #endregion

        #region Drawing
        private void RenderBottomArrow(GraphicsContext graphicsContext, Rectangle bounds)
        {
            DrawScrollbarEnd(graphicsContext, bounds, downTriangle);
        }

        private void RenderTopArrow(GraphicsContext graphicsContext, Rectangle bounds)
        {
            DrawScrollbarEnd(graphicsContext, bounds, upTriangle);
        }

        private void DrawScrollbarEnd(GraphicsContext graphicsContext, Rectangle bounds, Triangle triangle)
        {
            graphicsContext.Fill(bounds, Colors.Gray);
            graphicsContext.Stroke(bounds, Colors.Black);

            graphicsContext.Fill(triangle, Colors.Black);
        }
        #endregion
        #endregion
    }
}

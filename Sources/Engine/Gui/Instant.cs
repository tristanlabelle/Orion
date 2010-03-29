using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Engine.Gui
{
    public static class Instant
    {
        public static Rectangle CreateComponentRectangle(Rectangle parentSystem, Vector2 originPoint, Vector2 topmostPoint)
        {
            Vector2 computedOrigin = new Vector2(originPoint.X * parentSystem.Size.X, originPoint.Y * parentSystem.Size.Y);
            Vector2 computedTopmost = new Vector2(topmostPoint.X * parentSystem.Size.X, topmostPoint.Y * parentSystem.Size.Y);
            return new Rectangle(computedOrigin + parentSystem.Min, computedTopmost - computedOrigin);
        }

        public static Rectangle CreateComponentRectangle(Rectangle parentSystem, Rectangle childRectangle)
        {
            return CreateComponentRectangle(parentSystem, childRectangle.Min, childRectangle.Max);
        }

        public static void DisplayAlert(Responder parent, string message)
        {
            DisplayAlert(parent, message, null);
        }

        public static void DisplayAlert(Responder parent, string message, Action action)
        {
            Argument.EnsureNotNull(parent, "parent");
            Argument.EnsureNotNull(message, "message");

            Rectangle panelFrame = CreateComponentRectangle(parent.Bounds, new Vector2(0.25f, 0.33f), new Vector2(0.75f, 0.66f));
            Rectangle panelBounds = new Rectangle(panelFrame.Size);
            Rectangle labelRect = CreateComponentRectangle(panelBounds, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.9f));
            Rectangle buttonRect = CreateComponentRectangle(panelBounds, new Vector2(0.4f, 0.2f), new Vector2(0.6f, 0.3f));

            Panel panel = new Panel(panelFrame);
            Label messageLabel = new Label(labelRect, message);
            Button okButton = new Button(buttonRect, "Ok");
            okButton.HotKey = Keys.Enter;
            okButton.Triggered += button => { panel.Dispose(); if(action != null) action(); };

            panel.Children.Add(messageLabel);
            panel.Children.Add(okButton);

            parent.Children.Add(panel);
        }

        public static void Prompt(Responder parent, string message, Action<string> onClose)
        {
            Prompt(parent, message, string.Empty, onClose);
        }

        public static void Prompt(Responder parent, string message, string defaultValue, Action<string> onClose)
        {
            Argument.EnsureNotNull(parent, "parent");
            Argument.EnsureNotNull(message, "message");

            Rectangle panelFrame = CreateComponentRectangle(parent.Bounds, new Vector2(0.25f, 0.33f), new Vector2(0.75f, 0.66f));
            Rectangle panelBounds = new Rectangle(panelFrame.Size);
            Rectangle labelFrame = CreateComponentRectangle(panelBounds, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.9f));
            Rectangle textFieldFrame = CreateComponentRectangle(panelBounds, new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.5f));
            Rectangle okButtonFrame = CreateComponentRectangle(panelBounds, new Vector2(0.2f, 0.2f), new Vector2(0.4f, 0.3f));
            Rectangle cancelButtonFrame = CreateComponentRectangle(panelBounds, new Vector2(0.5f, 0.2f), new Vector2(0.7f, 0.3f));

            Panel panel = new Panel(panelFrame);
            Label messageLabel = new Label(labelFrame, message);
            TextField input = new TextField(textFieldFrame);
            input.Contents = defaultValue;
            Button okButton = new Button(okButtonFrame, "Ok");
            Button cancelButton = new Button(cancelButtonFrame, "Cancel");

            Action<Responder> accept = delegate(Responder sender)
            {
                onClose(input.Contents);
                panel.Dispose();
            };

            okButton.Triggered += new Action<Button>(accept);
            input.Triggered += new Action<TextField>(accept);

            cancelButton.Triggered += delegate(Button sender)
            {
                panel.Dispose();
            };

            panel.Children.Add(messageLabel);
            panel.Children.Add(input);
            panel.Children.Add(okButton);
            panel.Children.Add(cancelButton);

            parent.Children.Add(panel);
        }
    }
}

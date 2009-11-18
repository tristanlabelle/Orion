﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;
using Orion.Geometry;

namespace Orion.UserInterface.Widgets
{
    public static class Instant
    {
        private static Rectangle CreateComponentRectangle(Rectangle parentSystem, Vector2 originPoint, Vector2 topmostPoint)
        {
            Vector2 computedOrigin = new Vector2(originPoint.X * parentSystem.Size.X, originPoint.Y * parentSystem.Size.Y);
            Vector2 computedTopmost = new Vector2(topmostPoint.X * parentSystem.Size.X, topmostPoint.Y * parentSystem.Size.Y);
            return new Rectangle(computedOrigin + parentSystem.Min, computedTopmost - computedOrigin);
        }

        public static void DisplayAlert(Responder parent, string message)
        {
            Argument.EnsureNotNull(parent, "parent");
            Argument.EnsureNotNull(message, "message");

            Rectangle frameRect = CreateComponentRectangle(parent.Bounds, new Vector2(0.25f, 0.33f), new Vector2(0.75f, 0.66f));
            Rectangle frameBounds = new Rectangle(frameRect.Size);
            Rectangle labelRect = CreateComponentRectangle(frameBounds, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.9f));
            Rectangle buttonRect = CreateComponentRectangle(frameBounds, new Vector2(0.4f, 0.2f), new Vector2(0.6f, 0.3f));

            Frame container = new Frame(frameRect);
            Label displayedMessage = new Label(labelRect, message);
            Button okButton = new Button(buttonRect, "Ok");
            okButton.Pressed += delegate(Button sender) { container.Dispose(); };

            container.Children.Add(displayedMessage);
            container.Children.Add(okButton);

            parent.Children.Add(container);
        }

        public static void Prompt(Responder parent, string message, GenericEventHandler<string> onClose)
        {
            Argument.EnsureNotNull(parent, "parent");
            Argument.EnsureNotNull(message, "message");

            Rectangle frameRect = CreateComponentRectangle(parent.Bounds, new Vector2(0.25f, 0.33f), new Vector2(0.75f, 0.66f));
            Rectangle frameBounds = new Rectangle(frameRect.Size);
            Rectangle labelRect = CreateComponentRectangle(frameBounds, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.9f));
            Rectangle textFieldRect = CreateComponentRectangle(frameBounds, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.5f));
            Rectangle okButtonRect = CreateComponentRectangle(frameBounds, new Vector2(0.2f, 0.2f), new Vector2(0.4f, 0.3f));
            Rectangle cancelButtonRect = CreateComponentRectangle(frameBounds, new Vector2(0.5f, 0.2f), new Vector2(0.7f, 0.3f));

            Frame container = new Frame(frameRect);
            Label displayedMessage = new Label(labelRect, message);
            TextField input = new TextField(textFieldRect);
            Button okButton = new Button(okButtonRect, "Ok");
            Button cancelButton = new Button(cancelButtonRect, "Cancel");
            okButton.Pressed += delegate(Button sender)
            {
                onClose(input.Contents);
                container.Dispose();
            };
            cancelButton.Pressed += delegate(Button sender)
            {
                container.Dispose();
            };

            container.Children.Add(displayedMessage);
            container.Children.Add(input);
            container.Children.Add(okButton);
            container.Children.Add(cancelButton);

            parent.Children.Add(container);
        }
    }
}
using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Renderers;
using Orion.Matchmaking;

namespace Orion.Game.Presentation.Actions.UserCommands
{
    public sealed class AttackUserCommand : UserInputCommand, IViewRenderer
    {
        #region Fields
        private readonly Texture texture;
        private Vector2 cursorPosition = new Vector2(float.NaN, float.NaN);
        #endregion

        #region Constructors
        public AttackUserCommand(UserInputManager inputManager, GameGraphics gameGraphics)
            : base(inputManager)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            this.texture = gameGraphics.GetActionTexture("Attack");
        }
        #endregion

        #region Methods
        public override void OnMouseMoved(Vector2 location)
        {
            cursorPosition = location;
        }

        public override void OnClick(Vector2 location)
        {
            Point point = (Point)location;
            if (!World.IsWithinBounds(point)) return;

            Unit target = World.Entities.GetTopmostUnitAt(point);
            if (target == null) InputManager.LaunchZoneAttack(location);
            else InputManager.LaunchAttack(target);

            cursorPosition = new Vector2(float.NaN, float.NaN);
        }

        public void Draw(GraphicsContext graphicsContext, Rectangle bounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            float minBoundsSize = Math.Min(bounds.Width, bounds.Height);
            Vector2 size = new Vector2(minBoundsSize / texture.Width, minBoundsSize / texture.Height) * 4;
            Rectangle rectangle = new Rectangle(cursorPosition - size, size);
            graphicsContext.Fill(rectangle, texture);
        }
        #endregion
    }
}

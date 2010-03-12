using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics;
using Orion.Matchmaking;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class AttackUserCommand : UserInputCommand, IRenderer
    {
        #region Fields
        private readonly Texture texture;
        private Vector2 cursorPosition = new Vector2(float.NaN, float.NaN);
        #endregion

        #region Constructors
        public AttackUserCommand(UICommander uiCommander, GameGraphics gameGraphics)
            : base(uiCommander)
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

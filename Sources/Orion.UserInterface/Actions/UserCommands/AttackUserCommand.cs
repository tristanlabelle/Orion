using System;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;

namespace Orion.UserInterface.Actions.UserCommands
{
    public sealed class AttackUserCommand : UserInputCommand, IRenderer
    {
        #region Fields
        private readonly TextureManager textureManager;
        private Vector2 cursorPosition = new Vector2(float.NaN, float.NaN);
        #endregion

        #region Constructors
        public AttackUserCommand(UserInputManager inputManager, TextureManager textureManager)
            : base(inputManager)
        {
            Argument.EnsureNotNull(textureManager, "textureManager");
            this.textureManager = textureManager;
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

            Unit target = World.Entities.GetUnitAt(point);
            if (target == null) InputManager.LaunchZoneAttack(location);
            else InputManager.LaunchAttack(target);

            cursorPosition = new Vector2(float.NaN, float.NaN);
        }

        public void Draw(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            Texture texture = textureManager.GetAction("Attack");
            float minBoundsSize = Math.Min(graphicsContext.CoordinateSystem.Width, graphicsContext.CoordinateSystem.Height);
            Vector2 size = new Vector2(minBoundsSize / texture.Width, minBoundsSize / texture.Height) * 4;
            Rectangle rectangle = new Rectangle(cursorPosition - size, size);
            graphicsContext.Fill(rectangle, texture);
        }
        #endregion
    }
}

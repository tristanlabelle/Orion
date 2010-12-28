using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui2;
using Orion.Engine.Input;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Technologies;
using Keys = System.Windows.Forms.Keys;
using MouseButtons = System.Windows.Forms.MouseButtons;
using RootView = Orion.Engine.Gui.RootView;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Central point of access to game graphics. Used for rendering and resource creation.
    /// </summary>
    public sealed class GameGraphics : IDisposable
    {
        #region Fields
        private readonly IGameWindow window;
        private readonly Queue<InputEvent> inputEventQueue = new Queue<InputEvent>();
        private readonly UIManager uiManager;
        private readonly RootView rootView;
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public GameGraphics()
        {
            this.window = new OpenTKGameWindow("Orion", WindowMode.Windowed, new Size(1024, 768));
            this.window.InputReceived += OnInputReceived;
            this.window.Resized += OnWindowResized;

            Rectangle rootViewFrame = new Rectangle(window.ClientAreaSize.Width, window.ClientAreaSize.Height);
            this.rootView = new RootView(rootViewFrame, RootView.ContentsBounds);

            OrionGuiStyle style = new OrionGuiStyle(window.GraphicsContext);
            uiManager = style.CreateUIManager();
            uiManager.Size = window.ClientAreaSize;

            this.textureManager = new TextureManager(window.GraphicsContext, "../../../Assets/Textures");
        }
        #endregion

        #region Properties
        public IGameWindow Window
        {
            get { return window; }
        }

        /// <summary>
        /// Gets the graphics context which provides graphics to the game.
        /// </summary>
        public GraphicsContext Context
        {
            get { return window.GraphicsContext; }
        }

        public TextureManager TextureManager
        {
            get { return textureManager; }
        }

        public UIManager UIManager
        {
            get { return uiManager; }
        }

        public OrionGuiStyle GuiStyle
        {
            get { return (OrionGuiStyle)uiManager.Renderer; }
        }

        public RootView RootView
        {
            get { return rootView; }
        }

        public Texture DefaultTexture
        {
            get { return textureManager.Get("Default"); }
        }
        #endregion

        #region Methods
        #region Textures
        /// <summary>
        /// Gets a texture for a miscellaneous game element. 
        /// </summary>
        /// <param name="name">The name of the game element.</param>
        /// <returns>The texture for that game element.</returns>
        public Texture GetMiscTexture(string name)
        {
            return GetTexture(name);
        }

        /// <summary>
        /// Gets a texture representing a unit.
        /// </summary>
        /// <param name="unitTypeName">The name of a type of unit.</param>
        /// <returns>The texture for that unit type.</returns>
        public Texture GetUnitTexture(string unitTypeName)
        {
            Argument.EnsureNotNull(unitTypeName, "unitTypeName");

            string fullName = Path.Combine("Units", unitTypeName);
            return GetTexture(fullName);
        }

        /// <summary>
        /// Gets a texture representing a unit.
        /// </summary>
        /// <param name="unitType">The type of a unit.</param>
        /// <returns>The texture for that unit type.</returns>
        public Texture GetUnitTexture(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            return GetUnitTexture(unitType.GraphicsTemplate);
        }

        /// <summary>
        /// Gets a texture representing a unit.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>The texture for that unit.</returns>
        public Texture GetUnitTexture(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            return GetUnitTexture(unit.Type);
        }

        /// <summary>
        /// Gets a texture representing a resource.
        /// </summary>
        /// <param name="type">The type of resource for which to retrieve a texture.</param>
        /// <returns>The texture for that resource type.</returns>
        public Texture GetResourceTexture(ResourceType type)
        {
            string name = type.ToStringInvariant();
            return GetTexture(name);
        }

        /// <summary>
        /// Gets a texture representing a resource.
        /// </summary>
        /// <param name="node">The resource node for which to retrieve a texture.</param>
        /// <returns>The texture for that resource node.</returns>
        public Texture GetResourceTexture(ResourceNode node)
        {
            Argument.EnsureNotNull(node, "node");
            return GetResourceTexture(node.Type);
        }

        /// <summary>
        /// Gets a texture representing an action in the UI.
        /// </summary>
        /// <param name="actionName">The name of the UI action.</param>
        /// <returns>The texture for that action.</returns>
        public Texture GetActionTexture(string actionName)
        {
            string fullName = Path.Combine("Actions", actionName);
            return GetTexture(fullName);
        }

        /// <summary>
        /// Gets a texture representing a technology.
        /// </summary>
        /// <param name="actionName">The technology.</param>
        /// <returns>The texture for that technology.</returns>
        public Texture GetTechnologyTexture(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            string fullName = Path.Combine("Technologies", technology.Name);
            return GetTexture(fullName);
        }

        private Texture GetTexture(string name)
        {
            Texture texture = textureManager.Get(name);
            if (texture == textureManager.DefaultTexture)
                texture = DefaultTexture;
            return texture;
        }
        #endregion

        public void DrawGui()
        {
            uiManager.Draw();
            RootView.Draw(window.GraphicsContext);
        }

        /// <summary>
        /// Updates the root view for a frame, allowing it to process queued input.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the previous frame, in seconds.</param>
        public void UpdateGui(float timeDeltaInSeconds)
        {
            DispatchInputEvents();
            rootView.Update(timeDeltaInSeconds);
            uiManager.Update(TimeSpan.FromSeconds(timeDeltaInSeconds));
        }

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            textureManager.Dispose();
            window.Dispose();
        }

        private void DispatchInputEvents()
        {
            while (inputEventQueue.Count > 0)
            {
                InputEvent inputEvent = inputEventQueue.Dequeue();
                rootView.SendInputEvent(inputEvent);
                if (inputEvent.Type == InputEventType.Mouse)
                {
                    MouseEventType type;
                    MouseEventArgs args;
                    inputEvent.GetMouse(out type, out args);

                    uiManager.InjectMouseMove((int)args.X, (int)args.Y);
                    if (type == MouseEventType.WheelScrolled)
                    {
                        uiManager.InjectMouseWheel(args.WheelDelta);
                    }
                    else if (type == MouseEventType.ButtonPressed || type == MouseEventType.ButtonReleased)
                    {
                        MouseButtons buttons = MouseButtons.None;
                        if (args.Button == MouseButton.Left) buttons = MouseButtons.Left;
                        else if (args.Button == MouseButton.Middle) buttons = MouseButtons.Middle;
                        else if (args.Button == MouseButton.Right) buttons = MouseButtons.Right;

                        int pressCount = type == MouseEventType.ButtonReleased ? 0 : args.ClickCount;
                        if (buttons != MouseButtons.None) uiManager.InjectMouseButton(buttons, pressCount);
                    }
                }
                else if (inputEvent.Type == InputEventType.Keyboard)
                {
                    KeyboardEventType type;
                    KeyboardEventArgs args;
                    inputEvent.GetKeyboard(out type, out args);

                    uiManager.InjectKey(args.KeyAndModifiers, type == KeyboardEventType.ButtonPressed);
                }
                else if (inputEvent.Type == InputEventType.Character)
                {
                    char character;
                    inputEvent.GetCharacter(out character);

                    uiManager.InjectCharacter(character);
                }
            }
        }

        private void OnInputReceived(IGameWindow sender, InputEvent inputEvent)
        {
            HandleInput(inputEvent);
            inputEventQueue.Enqueue(inputEvent);
        }

        private void OnWindowResized(IGameWindow sender)
        {
            uiManager.Size = window.ClientAreaSize;
        }

        private void HandleInput(InputEvent inputEvent)
        {
            if (inputEvent.Type == InputEventType.Keyboard)
            {
                KeyboardEventType type;
                KeyboardEventArgs args;
                inputEvent.GetKeyboard(out type, out args);

                if (type == KeyboardEventType.ButtonPressed && args.IsAltModifierDown && args.Key == Keys.Enter)
                    ToggleFullscreen();
            }
        }

        private void ToggleFullscreen()
        {
            if (window.Mode == WindowMode.Windowed)
            {
                try { window.SetFullscreen(new Size(1280, 800)); }
                catch (NotSupportedException) { }
            }
            else
            {
                window.SetWindowed(window.ClientAreaSize);
            }
        }
        #endregion
    }
}

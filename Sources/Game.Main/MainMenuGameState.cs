using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui2;
using Orion.Engine.Gui2.Adornments;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the updating logic of the game when in the main menu.
    /// </summary>
    public sealed class MainMenuGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly DockPanel rootDockPanel;
        #endregion

        #region Constructors
        public MainMenuGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;

            var renderer = graphics.UIManager.Renderer;
            var style = graphics.GuiStyle;

            rootDockPanel = style.Create<DockPanel>();
            rootDockPanel.Adornment = new TextureAdornment(renderer.TryGetTexture("MenuBackground"));
            rootDockPanel.LastChildFill = true;

            ImageBox titleImageBox = style.Create<ImageBox>();
            titleImageBox.HorizontalAlignment = Alignment.Center;
            titleImageBox.Texture = renderer.TryGetTexture("Title");
            rootDockPanel.Dock(titleImageBox, Direction.MaxY);

            StackPanel buttonsStackPanel = style.Create<StackPanel>();
            buttonsStackPanel.HorizontalAlignment = Alignment.Center;
            buttonsStackPanel.VerticalAlignment = Alignment.Center;
            buttonsStackPanel.MinWidth = 300;
            buttonsStackPanel.MinChildSize = 50;
            buttonsStackPanel.ChildGap = 10;
            rootDockPanel.Dock(buttonsStackPanel, Direction.MinX);

            StackButton(buttonsStackPanel, "Monojoueur", sender => Manager.Push(new SinglePlayerDeathmatchSetupGameState(Manager, graphics)));
            StackButton(buttonsStackPanel, "Multijoueur", sender => Manager.Push(new MultiplayerLobbyGameState(Manager, graphics)));
            StackButton(buttonsStackPanel, "Tower Defense", sender => Manager.Push(new TowerDefenseGameState(Manager, graphics)));
            StackButton(buttonsStackPanel, "Typing Defense", sender => Manager.Push(new TypingDefenseGameState(Manager, graphics)));
            StackButton(buttonsStackPanel, "Visionner une partie", sender => Manager.Push(new ReplayBrowserGameState(Manager, graphics)));
            StackButton(buttonsStackPanel, "Quitter", sender => Manager.Pop());
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            graphics.UIManager.Content = rootDockPanel;
        }

        protected internal override void OnShadowed()
        {
            graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            graphics.DrawGui();
        }

        private void StackButton(StackPanel stackPanel, string text, Action<Button> action)
        {
            Button button = graphics.GuiStyle.CreateTextButton(text);
            button.Clicked += action;
            stackPanel.Stack(button);
        }
        #endregion
    }
}

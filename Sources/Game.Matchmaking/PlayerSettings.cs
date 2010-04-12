using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using ColorPalette = Orion.Engine.Colors;
using System.IO;

namespace Orion.Game.Matchmaking
{
    public class PlayerSettings
    {
        #region Fields
        private static ColorRgb[] colors = new ColorRgb[]
        {
            ColorPalette.Red, ColorPalette.Cyan, ColorPalette.Magenta, ColorPalette.Orange,
            ColorPalette.Green, ColorPalette.Yellow, ColorPalette.Gray, ColorPalette.Blue,
            ColorPalette.Lime, ColorPalette.Indigo, ColorPalette.White, ColorPalette.Chocolate
        };

        private Action<Player> onColorChanged;
        private List<Player> players = new List<Player>();
        private int maxPlayers;
        #endregion

        #region Constructors
        public PlayerSettings()
        {
            onColorChanged = OnColorChanged;
        }

        public PlayerSettings(IEnumerable<Player> players)
            : this()
        {
            this.players.AddRange(players);
        }
        #endregion

        #region Events
        public event Action<PlayerSettings, Player> PlayerJoined;
        public event Action<PlayerSettings, Player> PlayerLeft;
        public event Action<PlayerSettings, Player> PlayerChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the list of players that joined the game.
        /// </summary>
        public IEnumerable<Player> Players
        {
            get { return players; }
        }

        public IEnumerable<ColorRgb> AvailableColors
        {
            get { return colors.Except(players.Select(p => p.Color)); }
        }

        public int PlayersCount
        {
            get { return players.Count; }
        }

        public int MaximumNumberOfPlayers
        {
            get { return maxPlayers; }
            set { maxPlayers = value; }
        }
        #endregion

        #region Methods
        public void AddPlayer(Player player)
        {
            players.Add(player);
            player.ColorChanged += onColorChanged;
            TriggerEvent(PlayerJoined, player);
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);
            player.ColorChanged -= onColorChanged;
            TriggerEvent(PlayerLeft, player);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(players.Count);
            foreach (Player player in players)
                Player.Serializer.Serialize(player);
        }

        public void Deserialize(BinaryReader reader)
        {
            foreach (Player player in players.ToArray()) RemovePlayer(player);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                AddPlayer(Player.Serializer.Deserialize(reader));
        }

        private void OnColorChanged(Player player)
        {
            TriggerEvent(PlayerChanged, player);
        }

        private void TriggerEvent(Action<PlayerSettings, Player> eventHandler, Player player)
        {
            eventHandler.Raise(this, player);
        }
        #endregion
    }
}

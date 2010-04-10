using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Matchmaking
{
    public class PlayerSettings
    {
        #region Fields
        private Action<Player> onColorChanged;
        private List<Player> players = new List<Player>();
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

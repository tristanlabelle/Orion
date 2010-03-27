using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine.Collections;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation
{
    public sealed class MultiplayerMatchConfigurationUI : MatchConfigurationUI
    {
        #region Fields
        private SafeTransporter transporter;
        protected ListFrame chatMessages;
        #endregion

        #region Constructors
        public MultiplayerMatchConfigurationUI(MatchSettings options, SafeTransporter transporter, bool isHost)
            : base(options, isHost)
        {
            this.transporter = transporter;
        }
        #endregion

        #region Properties
        public IEnumerable<IPv4EndPoint> PlayerAddresses
        {
            get
            {
                return Players.OfType<RemotePlayerSlot>()
                    .Where(slot => slot.HostEndPoint.HasValue)
                    .Select(slot => slot.HostEndPoint.Value);
            }
        }
        #endregion

        #region Events
        public event Action<int, PlayerSlot> SlotChanged;
        public event Action<int, PlayerSlot> SlotOccupationChanged;
        public event Action<IPv4EndPoint> KickedPlayer;
        #endregion

        #region Methods
        protected override void InitializeSlots()
        {
            int i = 0;
            if (IsGameMaster)
            {
                LocalPlayerSlot you = new LocalPlayerSlot();
                playerSlots[i].AddItem(you);
                playerSlots[i].Enabled = false;
                i++;
            }

            while (i < playerSlots.Length)
            {
                DropdownList<PlayerSlot> dropdownList = playerSlots[i];
                dropdownList.AddItem(new AIPlayerSlot());
                dropdownList.AddItem(new ClosedPlayerSlot());
                dropdownList.AddItem(new RemotePlayerSlot());
                dropdownList.SelectedItem = dropdownList.Items.Last();
                dropdownList.SelectionChanged += SelectionChanged;
                dropdownList.Enabled = IsGameMaster;
                i++;
            }
        }

        public void OpenSlot(int slot)
        {
            SelectSlot<RemotePlayerSlot>(slot);
            ((RemotePlayerSlot)playerSlots[slot].SelectedItem).HostEndPoint = null;
        }

        public void CloseSlot(int slot)
        {
            SelectSlot<ClosedPlayerSlot>(slot);
        }

        public void SetLocalPlayerForSlot(int slot)
        {
            SelectSlot<LocalPlayerSlot>(slot);
        }

        public void UseAIForSlot(int slot)
        {
            SelectSlot<AIPlayerSlot>(slot);
        }

        public void UsePlayerForSlot(int slot, IPv4EndPoint host)
        {
            SelectSlot<RemotePlayerSlot>(slot);
            ((RemotePlayerSlot)playerSlots[slot].SelectedItem).HostEndPoint = host;
        }

        private void SelectionChanged(DropdownList<PlayerSlot> slot, PlayerSlot value)
        {
            RemotePlayerSlot remoteSlot = slot.Items.OfType<RemotePlayerSlot>().First();
            if (remoteSlot.HostEndPoint.HasValue)
            {
                Action<IPv4EndPoint> kickHandler = KickedPlayer;
                if (kickHandler != null) kickHandler(remoteSlot.HostEndPoint.Value);
                remoteSlot.HostEndPoint = null;
            }
            Action<int, PlayerSlot> handler = SlotOccupationChanged;
            if (handler != null) handler(playerSlots.IndexOf(slot), value);
        }

        private void SelectSlot<T>(int slot) where T : PlayerSlot, new()
        {
            T item = playerSlots[slot].Items.OfType<T>().FirstOrDefault();
            if(item == null)
            {
                item = new T();
                playerSlots[slot].AddItem(item);
            }
            playerSlots[slot].SelectedItem = item;
        }

        protected override void Update(float timeDeltaInSeconds)
        {
            transporter.Poll();
            base.Update(timeDeltaInSeconds);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) SlotOccupationChanged = null;

            base.Dispose(disposing);
        }
        #endregion
    }
}

using System;
using System.Net;
using System.Windows.Forms;

namespace Orion.UserInterface
{
    /// <summary>
    /// Describes the way a user decides to start a match.
    /// </summary>
    public enum MatchStartType
    {
        /// <summary>
        /// Specifies that no start type has been selected.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that a solo match should be started.
        /// </summary>
        Solo,

        /// <summary>
        /// Specifies that a multiplayer match should be hosted.
        /// </summary>
        Host,

        /// <summary>
        /// Specifies that a multiplayer match should be joined.
        /// </summary>
        Join
    }

    public sealed partial class MatchSettingsDialog : Form
    {
        #region Fields
        private MatchStartType startType;
        #endregion

        #region Constructors
        public MatchSettingsDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of match the user has selected.
        /// </summary>
        public MatchStartType StartType
        {
            get { return startType; }
        }

        /// <summary>
        /// Gets the address of the host to which to connect. 
        /// </summary>
        public IPAddress Host
        {
            get
            {
                IPAddress address;
                return IPAddress.TryParse(multiplayerHostTextBox.Text, out address) ? address : null;
            }
        }
        #endregion

        #region Methods
        private void startSoloGameButton_Click(object sender, EventArgs e)
        {
            startType = MatchStartType.Solo;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void hostMultiplayerGameButton_Click(object sender, EventArgs e)
        {
            startType = MatchStartType.Host;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void joinMultiplayerGameButton_Click(object sender, EventArgs e)
        {
            IPAddress address;
            IPAddress.TryParse(multiplayerHostTextBox.Text, out address);
            if (address == null || address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                MessageBox.Show("Invalid host IPV4 Address.", "Error!");
                return;
            }

            startType = MatchStartType.Join;
            DialogResult = DialogResult.OK;
            Close();
        }
        #endregion
    }
}

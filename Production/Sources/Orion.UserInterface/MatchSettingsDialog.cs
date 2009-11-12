using System;
using System.Net;
using System.Windows.Forms;

namespace Orion.UserInterface
{
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
        public Ipv4Address? HostAddress
        {
            get { return Ipv4Address.TryParse(multiplayerHostTextBox.Text); }
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
            Ipv4Address? address = Ipv4Address.TryParse(multiplayerHostTextBox.Text);
            if (!address.HasValue)
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

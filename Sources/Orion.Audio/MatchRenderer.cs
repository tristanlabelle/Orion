using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment;
using Orion.Commandment.Commands;
using System.Diagnostics;
using Orion.GameLogic;

namespace Orion.Audio
{
    public sealed class MatchRenderer
    {
        #region Fields
        private readonly AudioContext audioContext;
        private readonly Match match;
        private readonly UserInputManager userInputManager;
        private readonly StringBuilder stringBuilder = new StringBuilder();
        #endregion

        #region Constructors
        public MatchRenderer(AudioContext audioContext, Match match, UserInputManager userInputManager)
        {
            Argument.EnsureNotNull(audioContext, "audioContext");
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(userInputManager, "userInputManager");

            this.match = match;
            this.audioContext = audioContext;
            this.userInputManager = userInputManager;

            this.userInputManager.SelectionManager.SelectionChanged += OnSelectionChanged;
            this.userInputManager.LocalCommander.CommandGenerated += OnCommandGenerated;

            if (this.userInputManager.LocalFaction.Color == Colors.Pink)
                this.audioContext.PlaySound("tapette");
        }
        #endregion

        #region Properties
        private SelectionManager SelectionManager
        {
            get { return userInputManager.SelectionManager; }
        }

        private IEnumerable<Unit> SelectedUnits
        {
            get { return SelectionManager.SelectedUnits; }
        }
        #endregion

        #region Methods
        private void PlayUnitSound(UnitType unitType, string name)
        {
            stringBuilder.Clear();
            stringBuilder.Append(unitType.Name);
            stringBuilder.Append('.');
            stringBuilder.Append(name);
            audioContext.PlaySound(stringBuilder.ToString());
        }

        private void OnSelectionChanged(SelectionManager sender)
        {
            Debug.Assert(sender == SelectionManager);

            if (SelectedUnits.Count() == 1)
                PlayUnitSound(SelectedUnits.First().Type, "Select");
        }

        private void OnCommandGenerated(Commander sender, Command args)
        {
            Debug.Assert(sender == userInputManager.LocalCommander);
            Debug.Assert(args != null);

            UnitType unitType = SelectedUnits
                .Select(unit => unit.Type)
                .Distinct()
                .WithMaxOrDefault(type => SelectedUnits.Count(unit => unit.Type == type));

            if (unitType != null)
            {
                string commandName = args.GetType().Name.Replace("Command", "");
                PlayUnitSound(unitType, commandName);
            }
        }
        #endregion
    }
}

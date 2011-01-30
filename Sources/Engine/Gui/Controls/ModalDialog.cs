using System;

namespace Orion.Engine.Gui2
{
	/// <summary>
	/// A popup which displays a modal dialog.
	/// </summary>
	public sealed class ModalDialog : Popup
	{
		#region Constructors
		public ModalDialog()
		{
			HorizontalAlignment = Alignment.Center;
			VerticalAlignment = Alignment.Center;
		}
		#endregion
		
		#region Properties
        protected override bool IsModalImpl
		{
			get { return true; }
		}
		#endregion
		
		#region Methods
		public override Region GetDesiredRectangle()
		{
			return Manager.OuterRectangle;
		}
		#endregion
	}
}

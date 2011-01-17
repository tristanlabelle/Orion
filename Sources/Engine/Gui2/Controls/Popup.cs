using System;

namespace Orion.Engine.Gui2
{
	/// <summary>
	/// Represent a control which appear above the UI such as menus, tool tips and drop-down lists.
	/// </summary>
	public abstract class Popup : ContentControl
	{
		#region Properties
		/// <summary>
		/// Gets a value indicating if this <see cref="Popup"/> captures all input,
		/// prohibiting the user from using the underneath UI until it has been dealth with.
		/// </summary>
		public abstract bool IsModal { get; }
		#endregion
		
		#region Methods
		/// <summary>
		/// Gets the rectangle in which this <see cref="Popup"/> should be arranged.
		/// </summary>
		/// <returns>The rectangle to arrange this <see cref="Popup"/> in.</returns>
		public abstract Region GetDesiredRectangle();
		#endregion
	}
}

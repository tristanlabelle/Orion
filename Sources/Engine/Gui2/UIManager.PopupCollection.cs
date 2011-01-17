using System;
using System.Collections.ObjectModel;

namespace Orion.Engine.Gui2
{
	partial class UIManager
	{
		/// <summary>
		/// The collection of <see cref="Popup"/>s displayed in the UI.
		/// This works like a stack, the last <see cref="Popup"/> is the topmost one.
		/// </summary>
		public sealed class PopupCollection : Collection<Popup>
		{
			#region Fields
			private readonly UIManager manager;
			#endregion
			
			#region Constructors
			internal PopupCollection(UIManager manager)
			{
				Argument.EnsureNotNull(manager, "manager");
				this.manager = manager;
			}
			#endregion
			
			#region Methods
			protected override void InsertItem(int index, Popup item)
			{
				manager.AdoptChild(item);
				base.InsertItem(index, item);
				
				if (item.IsModal)
				{
					manager.KeyboardFocusedControl = null;
					manager.MouseCapturedControl = null;
				}
			}
			
			protected override void RemoveItem(int index)
			{
				Popup popup = Items[index];
				base.RemoveItem(index);
				manager.AbandonChild(popup);
			}
			
			protected override void SetItem(int index, Popup item)
			{
				RemoveItem(index);
				InsertItem(item);
			}
			
			protected override void ClearItems()
			{
				while (Items.Count > 0) RemoveItem(Items.Count - 1);
			}
			#endregion
		}
	}
}

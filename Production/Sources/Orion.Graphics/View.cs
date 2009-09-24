using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Graphics.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics
{
    public abstract class View
    {
		private List<View> subviewsList;
		private GraphicsContext Context;
		
		/// <summary>
		/// The superview of this view. 
		/// </summary>
		public View Superview { get; private set; }
		
		/// <summary>
		/// The list of subviews this view has. 
		/// </summary>
		public IEnumerable<View> Subviews
		{
			get
			{
				return subviewsList.ToArray();
			}
		}
		
		/// <summary>
		/// The rectangle in which this view appears in its superview. May be different from the bounds.
		/// </summary>
		public Rect Frame { get; set; }
		
		/// <summary>
		/// The internal coordinates system rectangle used for drawing.
		/// </summary>
		public Rect Bounds
		{
			get
			{
				return Context.CoordsSystem;
			}
			
			set
			{
				Context.CoordsSystem = value;
			}
		}
		
		/// <summary>
		/// Constructs a view with a given frame. 
		/// </summary>
		/// <param name="frame">
		/// The <see cref="Rect"/> that will be this object's Frame and (by default) Bounds
		/// </param>
		public View(Rect frame)
		{
			Context = new GraphicsContext(frame);
			Subviews = new List<View>();
			Frame = frame;
		}
		
		/// <summary>
		/// Inserts the given view in the view hierarchy under this one. 
		/// </summary>
		/// <param name="subview">
		/// A <see cref="View"/>
		/// </param>
		/// <exception cref="ArgumentException">
		/// If the passed view already has a superview, or if the passed view contains this view somewhere down its hierarchy
		/// </exception>
		public void AddSubview(View view)
		{
			if(view.Superview != null)
			{
				throw new ArgumentException("Cannot add as a subview a view that's already in another superview");
			}
			
			if(view.ContainsSubview(this))
			{
				throw new ArgumentException("Cannot add a view as a subview to itself");
			}
			
			view.Superview = this;
			subviewsList.Add(view);
		}
		
		/// <summary>
		/// Removes a directly descendant view of this object from the view hierarchy. 
		/// </summary>
		/// <param name="subview">
		/// The direct descendant <see cref="View"/>
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// If the passed view is not a direct descendant of this one
		/// </exception> 
		public void RemoveSubview(View subview)
		{
			if(subview.Superview != this)
			{
				throw new ArgumentException("Cannot remove a subview whose superview is not this object");
			}
			
			Subviews.Remove(subview);
			subview.Superview = null;
		}
		
		/// <summary>
		/// Removes the object from the view hierarchy. 
		/// </summary>
		public void RemoveFromSuperview()
		{
			Superview.RemoveSubview(this);
		}
		
		/// <summary>
		/// Indicates if the passed view is a descendant of this one. 
		/// </summary>
		/// <param name="view">
		/// The supposedly child view <see cref="View"/>
		/// </param>
		/// <returns>
		/// True if the passed view the same one as this one, or if it is under this one in the view hierarchy, false otherwise
		/// </returns>
		public bool ContainsSubview(View view)
		{
			while(view != null)
			{
				if(view == this)
					return true;
				view = view.Superview;
			}
			return false;
		}
		
		/// <summary>
		/// Tells the context object how to draw this view. 
		/// </summary>
		/// <param name="context">
		/// A <see cref="GraphicsContext"/> on which <see cref="Drawing.IDrawable"/> objects must be applied in the method body
		/// </param>
		protected abstract void Draw(GraphicsContext context);
		
		/// <summary>
		/// Renders the view hierarchy. 
		/// </summary>
		internal virtual void Render()
		{
			Context.Clear();
			Draw();
			Context.DrawInto(Frame);
			
			foreach(View view in subviewsList)
			{
				view.Render();
			}
		}
    }
}

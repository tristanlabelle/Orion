using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Orion.Geometry;

namespace Orion.Graphics.Widgets
{
    class Label : View
    {
        private string caption;

        public Label(Rectangle frame)
            : base(frame)
        { }

        public Label(Rectangle frame, string caption)
            : base(frame)
        {
            this.caption = caption;
        }

        protected override void Draw()
        {
			throw new NotImplementedException();
        }
    }
}

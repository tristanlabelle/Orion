using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class Tags : Component
    {
        #region Fields
        private readonly HashSet<string> tags = new HashSet<string>();
        #endregion

        #region Constructors
        public Tags(Entity entity, IEnumerable<string> tags)
            : base(entity)
        {
            foreach (string tag in tags)
                this.tags.Add(tag);
        }
        #endregion

        #region Methods
        public bool HasTag(string tag)
        {
            Argument.EnsureNotNull(tag, "tag");
            return tags.Contains(tag);
        }

        public void AddTag(string tag)
        {
            Argument.EnsureNotNull(tag, "tag");
            tags.Add(tag);
        }

        public void RemoveTag(string tag)
        {
            Argument.EnsureNotNull(tag, "tag");
            tags.Remove(tag);
        }
        #endregion
    }
}

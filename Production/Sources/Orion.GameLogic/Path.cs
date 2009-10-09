using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    class Path
    {
        #region Fields
        LinkedList<PathNode> linkList = new LinkedList<PathNode>();

        #endregion
        
        #region Methods
        public void AddNode(PathNode node)
        {
            linkList.AddFirst(node);
        }
        #endregion

        #region Proprieties

        public List<PathNode> List
        {
            get { return linkList.ToList(); }
        }

        #endregion
    }
}

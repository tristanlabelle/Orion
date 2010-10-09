using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking
{
    public class CompositeMatchAdvertizer : IMatchAdvertizer
    {
        #region Fields
        private List<IMatchAdvertizer> advertizers = new List<IMatchAdvertizer>();
        #endregion

        #region Constructors
        public CompositeMatchAdvertizer()
        { }
        #endregion

        #region Methods
        public void AddAdvertiser(IMatchAdvertizer advertizer)
        {
            advertizers.Add(advertizer);
        }

        public void Advertize(string name, int openSlotsCount)
        {
            advertizers.ForEach(a => a.Advertize(name, openSlotsCount));
        }

        public void Delist(string name)
        {
            advertizers.ForEach(a => a.Delist(name));
        }
        #endregion
    }
}

using System;

namespace Orion
{
    [Serializable]
    public struct ValueChangedEventArgs<TValue>
    {
        #region Fields
        private readonly TValue oldValue;
        private readonly TValue newValue;
        #endregion

        #region Constructors
        public ValueChangedEventArgs(TValue oldValue, TValue newValue)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
        #endregion

        #region Properties
        public TValue OldValue
        {
            get { return oldValue; }
        }

        public TValue NewValue
        {
            get { return newValue; }
        } 
        #endregion

        #region Methods
        public override string ToString()
        {
            return "{0} changed to {1}";
        }
        #endregion
    }

    public delegate void ValueChangedEventHandler<TSender, TValue>(TSender sender,
        ValueChangedEventArgs<TValue> eventArgs);
}

using System;

namespace Orion
{
    /// <summary>
    /// Delegate for a method which is called when a value gets changed.
    /// </summary>
    /// <typeparam name="TSender">The type of object which raised the event.</typeparam>
    /// <typeparam name="TValue">The type of the value which changed.</typeparam>
    /// <param name="sender">The object which raised the event.</param>
    /// <param name="oldValue">The value before the change.</param>
    /// <param name="newValue">The new value.</param>
    public delegate void ValueChangedEventHandler<TSender, TValue>(
        TSender sender, TValue oldValue, TValue newValue);
}

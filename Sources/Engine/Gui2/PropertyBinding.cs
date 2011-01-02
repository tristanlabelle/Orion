using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Orion.Engine.Gui2
{
    public static class PropertyBinding
    {
        public static void BindOneWay<T, U>(Expression<Func<T>> sourcePropertyExpression, Expression<Func<U>> destinationPropertyExpression, Func<T, U> converter = null)
        {
            var source = BindableProperty<T>.FromExpression(sourcePropertyExpression);
            if (!source.IsGettable) return;

            var destination = BindableProperty<U>.FromExpression(destinationPropertyExpression);
            if (!destination.IsSettable) return;

            if (converter == null) converter = value => (U)Convert.ChangeType(value, typeof(U));

            destination.Set(converter(source.Get()));

            if (source.Bind())
            {
                source.ValueChanged += sender => destination.Set(converter(source.Get()));
            }
        }
    }
}

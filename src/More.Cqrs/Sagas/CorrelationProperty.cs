// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Represents the property used for saga correlation.
    /// </summary>
    public class CorrelationProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationProperty"/> class.
        /// </summary>
        /// <param name="property">The correlated <see cref="PropertyInfo">property</see>.</param>
        /// <param name="value">The value to correlate on.</param>
        /// <param name="isDefaultValue">Indicates whether the correlated value is the default value for the type.</param>
        public CorrelationProperty( PropertyInfo property, object value, bool isDefaultValue )
        {
            Property = property;
            Value = value;
            IsDefaultValue = isDefaultValue;
        }

        /// <summary>
        /// Gets the property the saga is correlated on.
        /// </summary>
        /// <value>The <see cref="PropertyInfo">property</see> the saga is correlated on.</value>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the property value the saga is correlated on.
        /// </summary>
        /// <value>The correlated property value.</value>
        public object Value { get; }

        /// <summary>
        /// Gets a value indicating whether the correlated value is the default value.
        /// </summary>
        /// <value>True if the <see cref="Value">value</see> is the default value for the type; otherwise, false.</value>
        public bool IsDefaultValue { get; }
    }
}
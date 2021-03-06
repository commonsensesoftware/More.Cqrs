﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using System;

    /// <summary>
    /// Defines the behavior of an event.
    /// </summary>
    public interface IEvent : IMessage
    {
        /// <summary>
        /// Gets or sets the associated aggregate version.
        /// </summary>
        /// <value>The version of the aggregate that generated the event.</value>
        int Version { get; set; }

        /// <summary>
        /// Gets or sets the sequence of the event relative to the aggregate version.
        /// </summary>
        /// <value>The sequence of the generated the event.</value>
        /// <remarks>A single aggregate version may generate multiple event.</remarks>
        int Sequence { get; set; }
    }
}
// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using static System.DateTime;

    /// <summary>
    /// Represents the document for a recorded event.
    /// </summary>
    /// <typeparam name="TKey">The type of key used for events.</typeparam>
    public class RecordedEvent<TKey>
    {
        /// <summary>
        /// Gets or sets the identifier of the recorded event.
        /// </summary>
        /// <value>The identifier of the recorded event.</value>
        [JsonProperty( "id" )]
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the version of the recorded event.
        /// </summary>
        /// <value>The version of the recorded event.</value>
        [JsonProperty( "version" )]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the type of the recorded event.
        /// </summary>
        /// <value>The name of the type for the recorded event.</value>
        [JsonProperty( "type" )]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the revision of the recorded event.
        /// </summary>
        /// <value>The revision of the recorded event.</value>
        [JsonProperty( "revision" )]
        public int Revision { get; set; }

        /// <summary>
        /// Gets the date and time of the recorded event.
        /// </summary>
        /// <value>The <see cref="DateTime">date and time</see> of the recorded event in Universal Coordinated Time (UTC).</value>
        [JsonProperty( "recordedOn" )]
        public DateTime RecordedOn { get; } = UtcNow;

        /// <summary>
        /// Gets or sets the recorded event message in JSON format.
        /// </summary>
        /// <value>The recorded event message as a <see cref="JObject">JSON objecct</see>.</value>
        [JsonProperty( "message" )]
#pragma warning disable CA2227 // Collection properties should be read only
        public JObject Message { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
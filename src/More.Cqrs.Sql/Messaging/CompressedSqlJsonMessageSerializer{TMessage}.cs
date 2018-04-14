// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Newtonsoft.Json;
    using System.IO;
    using System.IO.Compression;
    using static System.IO.Compression.CompressionMode;

    /// <summary>
    /// Represents a SQL database message serializer that uses the JSON format with compression.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to serialize and deserialize.</typeparam>
    /// <remarks>This class uses the <see cref="DeflateStream"/> to perform compression and decompression operations.</remarks>
    public class CompressedSqlJsonMessageSerializer<TMessage> : SqlJsonMessageSerializer<TMessage> where TMessage : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedSqlJsonMessageSerializer{TMessage}"/> class.
        /// </summary>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used by the message serializer.</param>
        /// <param name="jsonSerializer">The <see cref="JsonSerializer">JSON serializer</see> used by the message serializer.</param>
        public CompressedSqlJsonMessageSerializer( IMessageTypeResolver messageTypeResolver, JsonSerializer jsonSerializer )
            : base( messageTypeResolver, jsonSerializer ) { }

        /// <summary>
        /// Gets or sets the compression level to use.
        /// </summary>
        /// <value>One of the <see cref="CompressionLevel"/> values.</value>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <returns>A <see cref="Stream">stream</see> containing the serialized message.</returns>
        public override Stream Serialize( TMessage message ) => new DeflateStream( Serialize( message ), CompressionLevel );

        /// <summary>
        /// Deserializes the specified message.
        /// </summary>
        /// <param name="messageType">The qualified type name of the message to deserialize.</param>
        /// <param name="revision">The revision of the message being deserialized.</param>
        /// <param name="message">The <see cref="Stream">stream</see> containing the message to be deserialized.</param>
        /// <returns>The deserialized <typeparamref name="TMessage">message</typeparamref>.</returns>
        public override TMessage Deserialize( string messageType, int revision, Stream message )
        {
            Arg.NotNull( message, nameof( message ) );

            using ( var decompressedStream = new DeflateStream( message, Decompress, leaveOpen: true ) )
            {
                return Deserialize( messageType, revision, decompressedStream );
            }
        }
    }
}
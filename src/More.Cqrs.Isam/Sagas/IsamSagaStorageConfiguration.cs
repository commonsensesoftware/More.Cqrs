// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using Microsoft.Database.Isam;
    using More.Domain.Messaging;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using static Microsoft.Database.Isam.ColumnFlags;
    using static Microsoft.Database.Isam.IndexFlags;
    using static System.BitConverter;
    using static System.Linq.Expressions.Expression;
    using static System.Text.Encoding;

    /// <summary>
    /// Represents the configuration for an ISAM database saga store.
    /// </summary>
    public class IsamSagaStorageConfiguration
    {
        const int Revision = 1;
        static readonly MethodInfo SerializeMethodOfT = typeof( IsamSagaStorageConfiguration ).GetRuntimeMethods().Single( m => !m.IsPublic && m.Name == nameof( Serialize ) );
        readonly string connectionString;
        readonly IIsamMessageSerializerFactory serializerFactory;
        readonly ConcurrentDictionary<Type, Func<ISagaData, Stream>> serializers = new ConcurrentDictionary<Type, Func<ISagaData, Stream>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamSagaStorageConfiguration"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="tableName">The saga data table name.</param>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">resolver</see> used to resolve message <see cref="Type">types</see>.</param>
        /// <param name="serializerFactory">The <see cref="IIsamMessageSerializerFactory">factory</see> used to create new message serializers.</param>
        public IsamSagaStorageConfiguration(
            string connectionString,
            string tableName,
            IMessageTypeResolver messageTypeResolver,
            IIsamMessageSerializerFactory serializerFactory )
        {
            Arg.NotNullOrEmpty( connectionString, nameof( connectionString ) );
            Arg.NotNull( messageTypeResolver, nameof( messageTypeResolver ) );
            Arg.NotNull( serializerFactory, nameof( serializerFactory ) );

            this.connectionString = connectionString;
            this.serializerFactory = serializerFactory;
            TableName = tableName;
            MessageTypeResolver = messageTypeResolver;
        }

        /// <summary>
        /// Gets the identifier of the saga store table.
        /// </summary>
        /// <vaule>The identifier of the saga store table.</vaule>
        public string TableName { get; }

        /// <summary>
        /// Gets the object used to resolve message types.
        /// </summary>
        /// <value>The <see cref="IMessageTypeResolver">resolver</see> used to resolve message <see cref="Type">types</see>.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Creates a new message serializer.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to create a serializer for.</typeparam>
        /// <returns>A new <see cref="IIsamMessageSerializer{TMessage}"/>.</returns>
        public IIsamMessageSerializer<TMessage> NewMessageSerializer<TMessage>() where TMessage : class => serializerFactory.NewSerializer<TMessage>();

        /// <summary>
        /// Create a new database connection.
        /// </summary>
        /// <returns>A new, configured <see cref="IsamConnection">database connection</see>.</returns>
        public virtual IsamConnection CreateConnection() => new IsamConnection( connectionString );

        /// <summary>
        /// Creates the saga storage tables.
        /// </summary>
        public virtual void CreateTables()
        {
            var connection = CreateConnection();
            var table = NewSagaTable();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    if ( !database.Exists( table.Name ) )
                    {
                        database.CreateTable( table );
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Loads the data for a saga using the specified identifier.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> used to load the saga data.</param>
        /// <param name="sagaId">The identifier of the saga to load.</param>
        /// <returns>The loaded saga <typeparamref name="TData">data</typeparamref> or <c>null</c>.</returns>
        public TData Load<TData>( Cursor cursor, Guid sagaId ) where TData : class, ISagaData
        {
            Arg.NotNull( cursor, nameof( cursor ) );

            var key = Key.Compose( sagaId );

            cursor.SetCurrentIndex( "PK_" + TableName );

            if ( !cursor.GotoKey( key ) )
            {
                return default;
            }

            var record = cursor.Record;
            var completed = (bool) record["Completed"];

            if ( completed )
            {
                return default;
            }

            var messageTypeName = (string) record["DataType"];
            var actualMessageType = MessageTypeResolver.ResolveType( messageTypeName, Revision );
            var requestedMessageType = typeof( TData );

            if ( actualMessageType != requestedMessageType )
            {
                return default;
            }

            using ( var stream = new MemoryStream( (byte[]) record["Data"], writable: false ) )
            {
                return NewMessageSerializer<TData>().Deserialize( messageTypeName, Revision, stream );
            }
        }

        /// <summary>
        /// Loads the data for a saga using the specified property name and value.
        /// </summary>
        /// <typeparam name="TData">The type of saga data.</typeparam>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> used to load the saga data.</param>
        /// <param name="propertyName">The name of the property to retrieve the saga by.</param>
        /// <param name="propertyValue">The value of the property to retrieve the saga by.</param>
        /// <returns>The loaded saga <typeparamref name="TData">data</typeparamref> or <c>null</c>.</returns>
        public TData Load<TData>( Cursor cursor, string propertyName, object propertyValue ) where TData : class, ISagaData
        {
            Arg.NotNull( cursor, nameof( cursor ) );
            Arg.NotNullOrEmpty( propertyName, nameof( propertyName ) );
            Arg.NotNull( propertyValue, nameof( propertyValue ) );

            var dataType = typeof( TData ).GetAssemblyQualifiedName();
            var key = Key.Compose( dataType, propertyName, ValueAsBinary( propertyValue ) );

            cursor.SetCurrentIndex( $"IX_{TableName}_ByProperty" );

            if ( !cursor.GotoKey( key ) )
            {
                return default;
            }

            var record = cursor.Record;
            var completed = (bool) record["Completed"];

            if ( completed )
            {
                return default;
            }

            var messageTypeName = (string) record["DataType"];
            var actualMessageType = MessageTypeResolver.ResolveType( messageTypeName, Revision );
            var requestedMessageType = typeof( TData );

            if ( actualMessageType != requestedMessageType )
            {
                return default;
            }

            using ( var stream = new MemoryStream( (byte[]) record["Data"], writable: false ) )
            {
                return NewMessageSerializer<TData>().Deserialize( messageTypeName, Revision, stream );
            }
        }

        /// <summary>
        /// Stores the specified saga data.
        /// </summary>
        /// <param name="saga">The <see cref="ISagaData">saga data</see> to create the command for.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        public virtual void Store( ISagaData saga, CorrelationProperty correlationProperty )
        {
            Arg.NotNull( saga, nameof( saga ) );
            Arg.NotNull( correlationProperty, nameof( correlationProperty ) );

            var connection = CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var cursor = database.OpenCursor( TableName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    Store( cursor, saga, correlationProperty );
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Stores the specified saga data.
        /// </summary>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> used to store the data.</param>
        /// <param name="saga">The <see cref="ISagaData">saga data</see> to create the command for.</param>
        /// <param name="correlationProperty">The property used to correlate the stored data.</param>
        public virtual void Store( Cursor cursor, ISagaData saga, CorrelationProperty correlationProperty )
        {
            Arg.NotNull( cursor, nameof( cursor ) );
            Arg.NotNull( saga, nameof( saga ) );
            Arg.NotNull( correlationProperty, nameof( correlationProperty ) );

            var key = Key.Compose( saga.Id );
            var data = Serialize( saga ).ToBytes();
            var now = DateTime.UtcNow;

            cursor.SetCurrentIndex( "PK_" + TableName );

            if ( cursor.GotoKey( key ) )
            {
                cursor.BeginEditForUpdate();

                var record = cursor.EditRecord;

                record["Data"] = data;
                record["Modified"] = now;
            }
            else
            {
                cursor.BeginEditForInsert();

                var record = cursor.EditRecord;

                record["SagaId"] = saga.Id;
                record["DataType"] = saga.GetType().GetAssemblyQualifiedName();
                record["Data"] = data;
                record["PropertyName"] = correlationProperty.Property.Name;
                record["PropertyValue"] = ValueAsBinary( correlationProperty.Value );
                record["Created"] = now;
                record["Modified"] = now;
            }

            cursor.AcceptChanges();
        }

        /// <summary>
        /// Records that the specified saga is complete in storage.
        /// </summary>
        /// <param name="cursor">The <see cref="Cursor">cursor</see> used to store the data.</param>
        /// <param name="saga">The <see cref="ISagaData">saga data</see> to mark as complete.</param>
        public virtual void Complete( Cursor cursor, ISagaData saga )
        {
            Arg.NotNull( cursor, nameof( cursor ) );
            Arg.NotNull( saga, nameof( saga ) );

            var key = Key.Compose( saga.Id );

            cursor.SetCurrentIndex( "PK_" + TableName );

            if ( !cursor.GotoKey( key ) )
            {
                return;
            }

            cursor.BeginEditForUpdate();

            var record = cursor.EditRecord;

            record["Completed"] = true;
            cursor.AcceptChanges();
        }

        /// <summary>
        /// Serializes the specified saga data.
        /// </summary>
        /// <param name="data">The <see cref="ISagaData">data</see> to serialize.</param>
        /// <returns>A <see cref="Stream">stream</see> containing the serialized saga data.</returns>
        public virtual Stream Serialize( ISagaData data )
        {
            Arg.NotNull( data, nameof( data ) );
            Contract.Ensures( Contract.Result<Stream>() != null );

            return serializers.GetOrAdd( data.GetType(), NewSerializer )( data );
        }

        /// <summary>
        /// Creates and returns a table definition for an saga store.
        /// </summary>
        /// <returns>A new <see cref="TableDefinition">table definition</see>.</returns>
        protected virtual TableDefinition NewSagaTable()
        {
            var table = new TableDefinition( TableName );
            var columns = table.Columns;
            var indices = table.Indices;
            var primaryKeyIndex = new IndexDefinition( "PK_" + table.Name )
            {
                Flags = Primary | Unique | DisallowNull | DisallowTruncation,
                KeyColumns = { new KeyColumn( "SagaId", isAscending: true ) },
            };
            var propertyLookupIndex = new IndexDefinition( $"IX_{table.Name}_ByProperty" )
            {
                Flags = DisallowNull | DisallowTruncation,
                KeyColumns =
                {
                    new KeyColumn( "DataType", isAscending: true ),
                    new KeyColumn( "PropertyName", isAscending: true ),
                    new KeyColumn( "PropertyValue", isAscending: true ),
                },
            };

            columns.Add( new ColumnDefinition( "SagaId", typeof( Guid ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "DataType", typeof( string ), ColumnFlags.Variable | NonNull ) );
            columns.Add( new ColumnDefinition( "Data", typeof( byte[] ), NonNull | Updatable ) );
            columns.Add( new ColumnDefinition( "PropertyName", typeof( string ), ColumnFlags.Variable | NonNull ) { MaxLength = 90 } );
            columns.Add( new ColumnDefinition( "PropertyValue", typeof( byte[] ), NonNull ) { MaxLength = 200 } );
            columns.Add( new ColumnDefinition( "Created", typeof( DateTime ), Fixed | NonNull ) );
            columns.Add( new ColumnDefinition( "Modified", typeof( DateTime ), Fixed | NonNull | Updatable ) );
            columns.Add( new ColumnDefinition( "Completed", typeof( bool ), Fixed | NonNull | Updatable ) { DefaultValue = false } );
            indices.Add( primaryKeyIndex );
            indices.Add( propertyLookupIndex );

            return table;
        }

        /// <summary>
        /// Converts the specified value to binary.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value in binary form.</returns>
        protected virtual byte[] ValueAsBinary( object value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<byte[]>() != null );

            switch ( value )
            {
                case short @short:
                    return GetBytes( @short );
                case int @int:
                    return GetBytes( @int );
                case long @long:
                    return GetBytes( @long );
                case ushort @ushort:
                    return GetBytes( @ushort );
                case uint @uint:
                    return GetBytes( @uint );
                case ulong @ulong:
                    return GetBytes( @ulong );
                case Guid guid:
                    return guid.ToByteArray();
                case string @string:
                    return Unicode.GetBytes( @string );
            }

            throw new ArgumentException( SR.UnsupportedCorrelationValueType.FormatDefault( value.GetType().Name ) );
        }

        Func<ISagaData, Stream> NewSerializer( Type dataType )
        {
            var data = Parameter( typeof( ISagaData ), "data" );
            var method = SerializeMethodOfT.MakeGenericMethod( dataType );
            var body = Call( Constant( this ), method, Convert( data, dataType ) );
            var lambda = Lambda<Func<ISagaData, Stream>>( body, data );

            return lambda.Compile();
        }

        Stream Serialize<TData>( TData data ) where TData : class, ISagaData => NewMessageSerializer<TData>().Serialize( data );
    }
}
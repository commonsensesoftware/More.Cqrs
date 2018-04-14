namespace Contoso.Services.Components
{
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using static More.Domain.SqlIdentifierParts;

    sealed class EventDataDataReader<TKey> : IDataReader
    {
        readonly DateTime recordedOn = DateTime.UtcNow;
        readonly SqlEventStoreConfiguration configuration;
        bool disposed;
        IEnumerator<EventDescriptor<TKey>> iterator;
        DataTable schema;

        ~EventDataDataReader() => Dispose( false );

        internal EventDataDataReader( SqlEventStoreConfiguration configuration, IEnumerable<EventDescriptor<TKey>> events )
        {
            this.configuration = configuration;
            iterator = events.GetEnumerator();
        }

        public object this[int i] => GetValue( i );

        public object this[string name] => GetValue( GetOrdinal( name ) );

        public int Depth => 0;

        public bool IsClosed { get; private set; } = false;

        public int RecordsAffected => -1;

        public int FieldCount => 7;

        public void Close() => IsClosed = true;

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public bool GetBoolean( int i ) => Convert.ToBoolean( GetValue( i ) );

        public byte GetByte( int i ) => Convert.ToByte( GetValue( i ) );

        public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length ) => throw new NotImplementedException();

        public char GetChar( int i ) => Convert.ToChar( GetValue( i ) );

        public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length ) => throw new NotImplementedException();

        public IDataReader GetData( int i ) => this;

        public string GetDataTypeName( int i )
        {
            if ( i == 0 ) // AggregateId
            {
                var keyType = typeof( TKey );

                switch ( Type.GetTypeCode( keyType ) )
                {
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        return "SMALLINT";
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        return "INT";
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return "BIGINT";
                    case TypeCode.String:
                        return "NVARCHAR(128)";
                    case TypeCode.Object:
                        if ( typeof( Guid ).Equals( keyType ) )
                        {
                            return "UNIQUEIDENTIFIER";
                        }

                        goto default;
                    default:
                        throw new InvalidOperationException( $"The type {typeof( TKey ).Name} cannot be used for a key column." );
                }
            }

            switch ( i )
            {
                case 1: // Version
                case 2: // Sequence
                case 5: // Revision
                    return "INT";
                case 3: // RecordedOn
                    return "DATETIME2";
                case 4: // Type
                    return "NVARCHAR(256)";
                case 6: // Message
                    return "VARBINARY(MAX)";
            }

            throw new ArgumentOutOfRangeException( nameof( i ) );
        }

        public DateTime GetDateTime( int i ) => Convert.ToDateTime( GetValue( i ) );

        public decimal GetDecimal( int i ) => Convert.ToDecimal( GetValue( i ) );

        public double GetDouble( int i ) => Convert.ToDouble( GetValue( i ) );

        public Type GetFieldType( int i )
        {
            switch ( i )
            {
                case 0: // AggregateId
                    return typeof( TKey );
                case 1: // Version
                case 2: // Sequence
                case 5: // Revision
                    return typeof( int );
                case 3: // RecordedOn
                    return typeof( DateTime );
                case 4: // Type
                    return typeof( string );
                case 6: // Message
                    return typeof( byte[] );
            }

            throw new IndexOutOfRangeException();
        }

        public float GetFloat( int i ) => Convert.ToSingle( GetValue( i ) );

        public Guid GetGuid( int i ) => (Guid) Convert.ChangeType( GetValue( i ), typeof( Guid ) );

        public short GetInt16( int i ) => Convert.ToInt16( GetValue( i ) );

        public int GetInt32( int i ) => Convert.ToInt32( GetValue( i ) );

        public long GetInt64( int i ) => Convert.ToInt64( GetValue( i ) );

        public string GetName( int i )
        {
            switch ( i )
            {
                case 0:
                    return "AggregateId";
                case 1:
                    return "Version";
                case 2:
                    return "Sequence";
                case 3:
                    return "RecordedOn";
                case 4:
                    return "Type";
                case 5:
                    return "Revision";
                case 6:
                    return "Message";
            }

            throw new IndexOutOfRangeException();
        }

        public int GetOrdinal( string name )
        {
            switch ( name )
            {
                case "AggregateId":
                    return 0;
                case "Version":
                    return 1;
                case "Sequence":
                    return 2;
                case "RecordedOn":
                    return 3;
                case "Type":
                    return 4;
                case "Revision":
                    return 5;
                case "Message":
                    return 6;
            }

            switch ( name.ToUpperInvariant() )
            {
                case "AGGREGATEID":
                    return 0;
                case "VERSION":
                    return 1;
                case "SEQUENCE":
                    return 2;
                case "RECORDEDON":
                    return 3;
                case "TYPE":
                    return 4;
                case "REVISION":
                    return 5;
                case "MESSAGE":
                    return 6;
            }

            throw new IndexOutOfRangeException();
        }

        public DataTable GetSchemaTable()
        {
            if ( schema != null )
            {
                return schema;
            }

            var schemaTable = new DataTable();
            var baseSchemaName = configuration.TableName.Delimit( SchemaName );
            var baseTableName = configuration.TableName.Delimit( ObjectName );
            var builder = new SqlConnectionStringBuilder();

            using ( var connection = configuration.CreateConnection() )
            {
                builder.ConnectionString = connection.ConnectionString;
            }

            var baseCatalogName = builder.InitialCatalog;
            var baseServerName = builder.DataSource;
            var keyType = typeof( TKey );

            // REF: https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.getschematable(v=vs.110).aspx
            schemaTable.Columns.Add( "AllowDBNull", typeof( bool ) );
            schemaTable.Columns.Add( "BaseCatalogName", typeof( string ) );
            schemaTable.Columns.Add( "BaseColumnName", typeof( string ) );
            schemaTable.Columns.Add( "BaseSchemaName", typeof( string ) );
            schemaTable.Columns.Add( "BaseServerName", typeof( string ) );
            schemaTable.Columns.Add( "BaseTableName", typeof( string ) );
            schemaTable.Columns.Add( "ColumnName", typeof( string ) );
            schemaTable.Columns.Add( "ColumnOrdinal", typeof( int ) );
            schemaTable.Columns.Add( "ColumnSize", typeof( int ) );
            schemaTable.Columns.Add( "DataTypeName", typeof( string ) );
            schemaTable.Columns.Add( "IsAliased", typeof( bool ) );
            schemaTable.Columns.Add( "IsAutoIncrement", typeof( bool ) );
            schemaTable.Columns.Add( "IsColumnSet", typeof( bool ) );
            schemaTable.Columns.Add( "IsExpression", typeof( bool ) );
            schemaTable.Columns.Add( "IsHidden", typeof( bool ) );
            schemaTable.Columns.Add( "IsIdentity", typeof( bool ) );
            schemaTable.Columns.Add( "IsKey", typeof( bool ) );
            schemaTable.Columns.Add( "IsLong", typeof( bool ) );
            schemaTable.Columns.Add( "IsReadOnly ", typeof( bool ) );
            schemaTable.Columns.Add( "IsRowVersion", typeof( bool ) );
            schemaTable.Columns.Add( "IsUnique", typeof( bool ) );
            schemaTable.Columns.Add( "NonVersionedProviderType", typeof( SqlDbType ) );
            schemaTable.Columns.Add( "NumericPrecision", typeof( byte ) );
            schemaTable.Columns.Add( "NumericScale", typeof( byte ) );
            schemaTable.Columns.Add( "ProviderSpecificDataType", typeof( string ) );
            schemaTable.Columns.Add( "ProviderType", typeof( string ) );
            schemaTable.Columns.Add( "UdtAssemblyQualifiedName", typeof( string ) );
            schemaTable.Columns.Add( "XmlSchemaCollectionDatabase", typeof( string ) );
            schemaTable.Columns.Add( "XmlSchemaCollectionName", typeof( string ) );
            schemaTable.Columns.Add( "XmlSchemaCollectionOwningSchema", typeof( string ) );

            switch ( Type.GetTypeCode( keyType ) )
            {
                case TypeCode.Int16:
                    schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 2, "SMALLINT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.SmallInt, 0, 0, "SMALLINT", typeof( short ), null, null, null, null } );
                    break;
                case TypeCode.UInt16:
                    schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 2, "SMALLINT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.SmallInt, 0, 0, "SMALLINT", typeof( ushort ), null, null, null, null } );
                    break;
                case TypeCode.Int32:
                    schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 4, "INT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.Int, 0, 0, "INT", typeof( int ), null, null, null, null } );
                    break;
                case TypeCode.UInt32:
                    schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 4, "INT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.Int, 0, 0, "INT", typeof( uint ), null, null, null, null } );
                    break;
                case TypeCode.Int64:
                    schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 8, "BIGINT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.BigInt, 0, 0, "BIGINT", typeof( long ), null, null, null, null } );
                    break;
                case TypeCode.UInt64:
                    schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 8, "BIGINT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.BigInt, 0, 0, "BIGINT", typeof( ulong ), null, null, null, null } );
                    break;
                case TypeCode.String:
                    schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 128, "NVARCHAR(128)", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.NVarChar, byte.MaxValue, byte.MaxValue, "NVARCHAR(128)", typeof( string ), null, null, null, null } );
                    break;
                case TypeCode.Object:
                    if ( typeof( Guid ).Equals( keyType ) )
                    {
                        schemaTable.Rows.Add( new object[] { false, baseCatalogName, "AggregateId", baseSchemaName, baseServerName, baseTableName, "AggregateId", 0, 16, "UNIQUEIDENTIFIER", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.UniqueIdentifier, 0, 0, "UNIQUEIDENTIFIER", typeof( Guid ), null, null, null, null } );
                        break;
                    }

                    goto default;
                default:
                    throw new InvalidOperationException( $"The type {typeof( TKey ).Name} cannot be used for a key column." );
            }

            schemaTable.Rows.Add( new object[] { false, baseCatalogName, "Version", baseSchemaName, baseServerName, baseTableName, "Version", 1, 4, "INT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.Int, 0, 0, "INT", typeof( int ), null, null, null, null } );
            schemaTable.Rows.Add( new object[] { false, baseCatalogName, "Sequence", baseSchemaName, baseServerName, baseTableName, "Sequence", 2, 4, "INT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.Int, 0, 0, "INT", typeof( int ), null, null, null, null } );
            schemaTable.Rows.Add( new object[] { false, baseCatalogName, "RecordedOn", baseSchemaName, baseServerName, baseTableName, "RecordedOn", 3, 8, "DATETIME2", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.DateTime2, 7, byte.MaxValue, "DATETIME2", typeof( DateTime ), null, null, null, null } );
            schemaTable.Rows.Add( new object[] { false, baseCatalogName, "Type", baseSchemaName, baseServerName, baseTableName, "Type", 4, 256, "NVARCHAR(256)", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.NVarChar, byte.MaxValue, byte.MaxValue, "NVARCHAR(256)", typeof( string ), null, null, null, null } );
            schemaTable.Rows.Add( new object[] { false, baseCatalogName, "Revision", baseSchemaName, baseServerName, baseTableName, "Revision", 5, 4, "INT", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.Int, 0, 0, "INT", typeof( int ), null, null, null, null } );
            schemaTable.Rows.Add( new object[] { false, baseCatalogName, "Message", baseSchemaName, baseServerName, baseTableName, "Message", 6, int.MaxValue, "VARBINARY(MAX)", false, false, false, false, false, false, false, false, false, false, false, SqlDbType.VarBinary, byte.MaxValue, byte.MaxValue, "VARBINARY(MAX)", typeof( byte[] ), null, null, null, null } );

            return schema = schemaTable;
        }

        public string GetString( int i ) => GetValue( i )?.ToString();

        public object GetValue( int i )
        {
            switch ( i )
            {
                case 0:
                    return iterator.Current.AggregateId;
                case 1:
                    return iterator.Current.Event.Version;
                case 2:
                    return iterator.Current.Event.Sequence;
                case 3:
                    return recordedOn;
                case 4:
                    return iterator.Current.MessageType;
                case 5:
                    return iterator.Current.Event.Revision;
                case 6:
                    return ToBytes( configuration.EventSerializer.Serialize( iterator.Current.Event ) );
            }

            throw new IndexOutOfRangeException();
        }

        public int GetValues( object[] values )
        {
            var count = Math.Min( 7, values.Length );

            for ( var i = 0; i < count; i++ )
            {
                switch ( i )
                {
                    case 0:
                        values[i] = iterator.Current.AggregateId;
                        break;
                    case 1:
                        values[i] = iterator.Current.Event.Version;
                        break;
                    case 2:
                        values[i] = iterator.Current.Event.Sequence;
                        break;
                    case 3:
                        values[i] = recordedOn;
                        break;
                    case 4:
                        values[i] = iterator.Current.MessageType;
                        break;
                    case 5:
                        values[i] = iterator.Current.Event.Revision;
                        break;
                    case 6:
                        values[i] = ToBytes( configuration.EventSerializer.Serialize( iterator.Current.Event ) );
                        break;
                }
            }

            return count;
        }

        public bool IsDBNull( int i )
        {
            if ( i < 0 || i > 7 )
            {
                throw new IndexOutOfRangeException();
            }

            return false;
        }

        public bool NextResult() => false;

        public bool Read() => iterator.MoveNext();

        void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;

            if ( disposing )
            {
                iterator.Dispose();
                iterator = null;
                IsClosed = true;
            }
            else
            {
                iterator?.Dispose();
            }

            schema?.Dispose();
        }

        static byte[] ToBytes( Stream stream )
        {
            if ( stream is MemoryStream memoryStream )
            {
                return memoryStream.ToArray();
            }

            using ( memoryStream = new MemoryStream() )
            {
                stream.CopyTo( memoryStream );
                return memoryStream.ToArray();
            }
        }
    }
}
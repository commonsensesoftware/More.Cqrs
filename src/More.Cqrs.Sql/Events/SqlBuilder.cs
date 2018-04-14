// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Diagnostics.Contracts;
    using static SqlIdentifierParts;

    sealed class SqlBuilder
    {
        internal SqlBuilder( SqlEventStoreConfiguration eventStore, SqlSnapshotConfiguration snapshots )
        {
            Contract.Requires( eventStore != null );
            Contract.Requires( snapshots != null );

            EventStore = new EventStoreSqlBuilder( eventStore.TableName );
            Snapshots = new SnapshotSqlBuilder( snapshots.TableName );
        }

        internal EventStoreSqlBuilder EventStore { get; }

        internal SnapshotSqlBuilder Snapshots { get; }

        static string Statement( params string[] lines ) => string.Join( Environment.NewLine, lines );

        internal sealed class EventStoreSqlBuilder
        {
            readonly SqlIdentifier identifier;

            internal EventStoreSqlBuilder( SqlIdentifier identifier )
            {
                this.identifier = identifier;

                var delimitedName = identifier.Delimit();

#pragma warning disable SA1137 // Elements should have the same indentation

                CreateSchema = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.schemas WHERE name = '{identifier.SchemaName}' )",
                     "BEGIN",
                    $"    EXECUTE( 'CREATE SCHEMA {identifier.Delimit( SchemaName )};' );",
                     "END;" );

                Load = Statement(
                    "SELECT Type, Revision, Message",
                   $"FROM {delimitedName}",
                    "WHERE AggregateId = @AggregateId",
                    "ORDER BY Version, Sequence;" );

                Save = Statement(
                    $"INSERT INTO {delimitedName}( AggregateId, Version, Sequence, Type, Revision, Message )",
                     "VALUES( @AggregateId, @Version, @Sequence, @Type, @Revision, @Message );" );

#pragma warning restore SA1137 // Elements should have the same indentation
            }

            internal string CreateSchema { get; }

            internal string CreateTable( string keyDataType )
            {
                var name = identifier.AsIdentifierName();

#pragma warning disable SA1137 // Elements should have the same indentation

                return Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID( '{identifier.SchemaName}' ) AND name = '{identifier.ObjectName}' )",
                     "BEGIN",
                    $"    CREATE TABLE {identifier.Delimit()}",
                     "    (",
                    $"         AggregateId {keyDataType} NOT NULL",
                     "        ,Version INT NOT NULL",
                     "        ,Sequence INT NOT NULL",
                     "        ,RecordedOn DATETIME2 NOT NULL DEFAULT(GETUTCDATE())",
                     "        ,Type NVARCHAR(256) NOT NULL",
                     "        ,Revision INT NOT NULL DEFAULT(1)",
                     "        ,Message VARBINARY(MAX) NOT NULL",
                    $"        ,CONSTRAINT PK_{name} PRIMARY KEY( AggregateId, Version, Sequence )",
                    $"        ,CONSTRAINT CK_{name}_Version CHECK( Version >= 0 )",
                    $"        ,CONSTRAINT CK_{name}_Sequence CHECK( Sequence >= 0 )",
                     "    );",
                     "END;" );

#pragma warning restore SA1137 // Elements should have the same indentation
            }

            internal string Load { get; }

            internal string Save { get; }
        }

        internal sealed class SnapshotSqlBuilder
        {
            readonly SqlIdentifier identifier;

            internal SnapshotSqlBuilder( SqlIdentifier identifier )
            {
                this.identifier = identifier;

                var delimitedName = identifier.Delimit();
                var name = identifier.AsIdentifierName();

#pragma warning disable SA1137 // Elements should have the same indentation

                CreateSchema = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.schemas WHERE name = '{identifier.SchemaName}' )",
                    "BEGIN",
                    $"    EXECUTE( 'CREATE SCHEMA {identifier.Delimit( SchemaName )};' );",
                    "END;" );

                Load = Statement(
                    "SELECT TOP(1) Type, Version, Snapshot",
                   $"FROM {delimitedName}",
                    "WHERE AggregateId = @AggregateId",
                    "GROUP BY Type, Version, Snapshot",
                    "HAVING Version = MAX(Version);" );

                Save = Statement(
                    $"INSERT INTO {delimitedName}( AggregateId, Version, Type, Snapshot )",
                     "VALUES( @AggregateId, @Version, @Type, @Snapshot );" );

#pragma warning restore SA1137 // Elements should have the same indentation
            }

            internal string CreateSchema { get; }

            internal string CreateTable( string keyDataType )
            {
                var name = identifier.AsIdentifierName();

#pragma warning disable SA1137 // Elements should have the same indentation

                return Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID( '{identifier.SchemaName}' ) AND name = '{identifier.ObjectName}' )",
                     "BEGIN",
                    $"    CREATE TABLE {identifier.Delimit()}",
                     "    (",
                    $"         AggregateId {keyDataType} NOT NULL",
                     "        ,Version INT NOT NULL",
                     "        ,TakenOn DATETIME2 NOT NULL DEFAULT(GETUTCDATE())",
                     "        ,Type NVARCHAR(256) NOT NULL",
                     "        ,Snapshot VARBINARY(MAX) NOT NULL",
                    $"        ,CONSTRAINT PK_{name} PRIMARY KEY( AggregateId, Version )",
                    $"        ,CONSTRAINT CK_{name}_Version CHECK( Version >= 0 )",
                     "    );",
                     "END;" );

#pragma warning restore SA1137 // Elements should have the same indentation
            }

            internal string Load { get; }

            internal string Save { get; }
        }
    }
}
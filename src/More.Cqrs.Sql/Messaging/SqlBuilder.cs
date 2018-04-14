// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using static SqlIdentifierParts;

    sealed class SqlBuilder
    {
        internal SqlBuilder( SqlIdentifier messageQueueIdentifier, SqlIdentifier subscriptionIdentifier, SqlIdentifier subscriptionQueueIdentifier )
        {
            MessageQueue = new MessageQueueSqlBuilder( messageQueueIdentifier, subscriptionIdentifier, subscriptionQueueIdentifier );
            Subscription = new SubscriptionSqlBuilder( subscriptionIdentifier, messageQueueIdentifier, subscriptionQueueIdentifier );
            SubscriptionQueue = new SubscriptionQueueSqlBuilder( subscriptionQueueIdentifier, subscriptionIdentifier );
        }

        internal MessageQueueSqlBuilder MessageQueue { get; }

        internal SubscriptionSqlBuilder Subscription { get; }

        internal SubscriptionQueueSqlBuilder SubscriptionQueue { get; }

        static string Statement( params string[] lines ) => string.Join( Environment.NewLine, lines );

        internal sealed class MessageQueueSqlBuilder
        {
            internal MessageQueueSqlBuilder( SqlIdentifier identifier, SqlIdentifier subscriptionIdentifier, SqlIdentifier subscriptionQueueIdentifier )
            {
                var delimitedName = identifier.Delimit();
                var indexName = "IX_" + identifier.AsIdentifierName();
                var triggerIdentifier = new SqlIdentifier( identifier.SchemaName, "AfterInsertInto" + identifier.AsIdentifierName( ObjectName ) );

#pragma warning disable SA1137 // Elements should have the same indentation

                CreateSchema = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.schemas WHERE name = '{identifier.SchemaName}' )",
                     "BEGIN",
                    $"    EXECUTE( 'CREATE SCHEMA {identifier.Delimit( SchemaName )};' );",
                     "END;" );

                CreateTable = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID( '{identifier.SchemaName}' ) AND name = '{identifier.ObjectName}' )",
                     "BEGIN",
                    $"    CREATE TABLE {delimitedName}",
                     "    (",
                     "         MessageId UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWSEQUENTIALID()) PRIMARY KEY NONCLUSTERED",
                     "        ,EnqueueTime DATETIME2 NOT NULL DEFAULT(GETUTCDATE())",
                     "        ,DueTime DATETIME2 NOT NULL DEFAULT(GETUTCDATE())",
                     "        ,Type NVARCHAR(256) NOT NULL",
                     "        ,Revision INT NOT NULL DEFAULT(1)",
                     "        ,Message VARBINARY(MAX) NOT NULL",
                     "    );",
                     "END;" );

                CreateIndex = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.indexes WHERE name = '{indexName}' )",
                     "BEGIN",
                    $"    CREATE CLUSTERED INDEX {indexName}",
                    $"    ON {delimitedName}( EnqueueTime, MessageId );",
                     "END;" );

                Enqueue = Statement(
                    $"INSERT INTO {delimitedName}( EnqueueTime, DueTime, Type, Revision, Message )",
                     "VALUES( @EnqueueTime, @DueTime, @Type, @Revision, @Message );" );

                CreateTrigger = Statement(
                    $"CREATE OR ALTER TRIGGER {triggerIdentifier.Delimit()} ON {delimitedName} FOR INSERT AS",
                     "BEGIN",
                     "SET NOCOUNT ON;",
                    $"INSERT INTO {subscriptionQueueIdentifier.Delimit()}",
                     "SELECT",
                     "     s.SubscriptionId",
                     "    ,i.MessageId",
                     "    ,i.EnqueueTime",
                     "    ,0 -- DequeueAttempts",
                     "    ,i.DueTime",
                     "    ,i.Type",
                     "    ,i.Revision",
                     "    ,i.Message",
                     "FROM",
                    $"     {subscriptionIdentifier.Delimit()} s",
                     "    ,INSERTED i;",
                     "END;");

#pragma warning restore SA1137 // Elements should have the same indentation
            }

            internal string CreateSchema { get; }

            internal string CreateTable { get; }

            internal string CreateIndex { get; }

            internal string Enqueue { get; }

            internal string CreateTrigger { get; }
        }

        internal sealed class SubscriptionSqlBuilder
        {
            internal SubscriptionSqlBuilder( SqlIdentifier identifier, SqlIdentifier messageQueueIdentifier, SqlIdentifier subscriptionQueueIdentifier )
            {
                var delimitedName = identifier.Delimit();
                var triggerIdentifier = new SqlIdentifier( identifier.SchemaName, "AfterInsertInto" + identifier.AsIdentifierName( ObjectName ) );

#pragma warning disable SA1137 // Elements should have the same indentation

                CreateSchema = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.schemas WHERE name = '{identifier.SchemaName}' )",
                     "BEGIN",
                    $"    EXECUTE( 'CREATE SCHEMA {identifier.Delimit( SchemaName )};' );",
                     "END;" );

                CreateTable = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID( '{identifier.SchemaName}' ) AND name = '{identifier.ObjectName}' )",
                     "BEGIN",
                    $"    CREATE TABLE {delimitedName}",
                     "    (",
                     "         SubscriptionId UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWSEQUENTIALID()) PRIMARY KEY",
                     "        ,CreationTime DATETIME2 NOT NULL DEFAULT(GETUTCDATE())",
                     "    );",
                     "END;" );

                CreateTrigger = Statement(
                    $"CREATE OR ALTER TRIGGER {triggerIdentifier.Delimit()} ON {delimitedName} FOR INSERT AS",
                     "BEGIN",
                     "SET NOCOUNT ON;",
                    $"INSERT INTO {subscriptionQueueIdentifier.Delimit()}",
                     "SELECT",
                     "     i.SubscriptionId",
                     "    ,mq.MessageId",
                     "    ,mq.EnqueueTime",
                     "    ,0 -- DequeueAttempts",
                     "    ,mq.DueTime",
                     "    ,mq.Type",
                     "    ,mq.Revision",
                     "    ,mq.Message",
                     "FROM",
                     "    INSERTED i",
                    $"    INNER JOIN {messageQueueIdentifier.Delimit()} mq ON mq.EnqueueTime >= i.CreationTime;",
                     "END;" );

                CreateSubscription = Statement(
                    $"MERGE {delimitedName} AS target",
                     "USING ( SELECT @SubscriptionId, @CreationTime ) AS source ( SubscriptionId, CreationTime )",
                     "ON target.SubscriptionId = source.SubscriptionId",
                     "WHEN NOT MATCHED THEN",
                     "    INSERT ( SubscriptionId, CreationTime )",
                     "    VALUES ( source.SubscriptionId, source.CreationTime );" );

                DeleteSubscription = $"DELETE FROM {delimitedName} WHERE SubscriptionId = SubscriptionId;";

#pragma warning restore SA1137 // Elements should have the same indentation
            }

            internal string CreateSchema { get; }

            internal string CreateTable { get; }

            internal string CreateTrigger { get; }

            internal string CreateSubscription { get; }

            internal string DeleteSubscription { get; }
        }

        internal sealed class SubscriptionQueueSqlBuilder
        {
            internal SubscriptionQueueSqlBuilder( SqlIdentifier identifier, SqlIdentifier subscriptionIdentifier )
            {
                var delimitedName = identifier.Delimit();
                var identifierName = identifier.AsIdentifierName();
                var indexName = "IX_" + identifier.AsIdentifierName();

#pragma warning disable SA1137 // Elements should have the same indentation

                CreateSchema = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.schemas WHERE name = '{identifier.SchemaName}' )",
                     "BEGIN",
                    $"    EXECUTE( 'CREATE SCHEMA {identifier.Delimit( SchemaName )};' );",
                     "END;" );

                CreateTable = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID( '{identifier.SchemaName}' ) AND name = '{identifier.ObjectName}' )",
                     "BEGIN",
                    $"    CREATE TABLE {delimitedName}",
                     "    (",
                     "         SubscriptionId UNIQUEIDENTIFIER NOT NULL",
                     "        ,MessageId UNIQUEIDENTIFIER NOT NULL",
                     "        ,EnqueueTime DATETIME2 NOT NULL",
                     "        ,DequeueAttempts INT NOT NULL DEFAULT(0)",
                     "        ,DueTime DATETIME2 NOT NULL",
                     "        ,Type NVARCHAR(256) NOT NULL",
                     "        ,Revision INT NOT NULL",
                     "        ,Message VARBINARY(MAX) NOT NULL",
                    $"        ,CONSTRAINT PK_{identifierName} PRIMARY KEY NONCLUSTERED ( SubscriptionId, MessageId )",
                    $"        ,CONSTRAINT FK_{identifierName}_{subscriptionIdentifier.AsIdentifierName( ObjectName )} FOREIGN KEY ( SubscriptionId )",
                    $"             REFERENCES {subscriptionIdentifier.Delimit()} ( SubscriptionId ) ON DELETE CASCADE",
                     "    );",
                     "END;" );

                CreateIndex = Statement(
                    $"IF NOT EXISTS( SELECT * FROM sys.indexes WHERE name = '{indexName}' )",
                     "BEGIN",
                    $"    CREATE CLUSTERED INDEX {indexName}",
                    $"    ON {delimitedName}( DueTime, DequeueAttempts );",
                     "END;" );

                Enqueue = $"INSERT INTO {delimitedName} VALUES( @SubscriptionId, @MessageId, @EnqueueTime, @DequeueAttempts, @DueTime, @Type, @Revision, @Message );";

                Dequeue = Statement(
                    $"DELETE FROM {delimitedName}",
                     "OUTPUT DELETED.MessageId, DELETED.EnqueueTime, DELETED.DequeueAttempts, DELETED.DueTime,",
                     "    DELETED.Type, DELETED.Revision, DELETED.Message",
                     "WHERE MessageId IN (",
                     "    SELECT TOP(1) MessageId",
                    $"    FROM {delimitedName} WITH (XLOCK, ROWLOCK, READPAST)",
                     "    WHERE DueTime <= @DueTime",
                     "    ORDER BY DequeueAttempts );" );

#pragma warning restore SA1137 // Elements should have the same indentation
            }

            internal string CreateSchema { get; }

            internal string CreateTable { get; }

            internal string CreateIndex { get; }

            internal string Enqueue { get; }

            internal string Dequeue { get; }
        }
    }
}
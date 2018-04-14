// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics.Contracts;
    using static SqlIdentifierParts;

    sealed class SqlBuilder
    {
        internal SqlBuilder( SqlIdentifier identifier )
        {
            var delimitedName = identifier.Delimit();
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
                 "         SagaId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED",
                 "        ,DataType NVARCHAR(256) NOT NULL",
                 "        ,Data VARBINARY(MAX) NOT NULL",
                 "        ,PropertyName NVARCHAR(90) NOT NULL",
                 "        ,PropertyValue VARBINARY(200) NOT NULL",
                 "        ,Created DATETIME2 NOT NULL DEFAULT(GETUTCDATE())",
                 "        ,Modified DATETIME2 NOT NULL DEFAULT(GETUTCDATE())",
                 "        ,Completed BIT NOT NULL DEFAULT(0)",
                 "    );",
                 "END;" );

            CreateIndex = Statement(
                $"IF NOT EXISTS( SELECT * FROM sys.indexes WHERE name = 'IX_{indexName}' )",
                 "BEGIN",
                $"    CREATE CLUSTERED INDEX IX_{indexName}",
                $"    ON {delimitedName}( DataType, PropertyName, PropertyValue );",
                 "END;" );

            QueryById = Statement(
                "SELECT TOP(1) DataType, Data",
               $"FROM {delimitedName}",
                "WHERE SagaId = @SagaId AND Completed = 0;" );

            QueryByProperty = Statement(
                "SELECT TOP(1) Data",
               $"FROM {delimitedName}",
                "WHERE DataType = @DataType",
                "  AND PropertyName = @PropertyName",
                "  AND PropertyValue = @PropertyValue",
                "  AND Completed = 0;" );

            Store = Statement(
                $"MERGE {delimitedName} AS target",
                 "USING ( SELECT @SagaId, @DataType, @Data, @PropertyName, @PropertyValue ) AS source ( SagaId, DataType, Data, PropertyName, PropertyValue )",
                 "ON target.SagaId = source.SagaId",
                 "WHEN MATCHED THEN UPDATE",
                 "    SET Data = source.Data, Modified = GETUTCDATE()",
                 "WHEN NOT MATCHED THEN",
                 "    INSERT ( SagaId, DataType, Data, PropertyName, PropertyValue )",
                 "    VALUES ( source.SagaId, source.DataType, source.Data, source.PropertyName, source.PropertyValue );" );

            Completed = Statement(
                $"UPDATE {delimitedName}",
                 "SET Completed = 1, Modified = GETUTCDATE()",
                 "WHERE SagaId = @SagaId;" );

#pragma warning restore SA1137 // Elements should have the same indentation
        }

        internal string CreateSchema { get; }

        internal string CreateTable { get; }

        internal string CreateIndex { get; }

        internal string QueryById { get; }

        internal string QueryByProperty { get; }

        internal string Store { get; }

        internal string Completed { get; }

        static string Statement( params string[] lines ) => string.Join( Environment.NewLine, lines );
    }
}
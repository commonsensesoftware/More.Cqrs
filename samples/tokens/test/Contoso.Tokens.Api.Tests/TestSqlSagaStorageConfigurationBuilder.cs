namespace Contoso.Services
{
    using More.Domain;
    using More.Domain.Messaging;
    using More.Domain.Sagas;
    using System.Data.Common;

    sealed class TestSqlSagaStorageConfigurationBuilder : SqlSagaStorageConfigurationBuilder
    {
        public override SqlSagaStorageConfiguration CreateConfiguration()
        {
            return new TestSqlSagaStorageConfiguration(
                ProviderFactory,
                ConnectionString,
                TableName,
                MessageTypeResolver,
                NewMessageSerializerFactory( MessageTypeResolver ) );
        }

        sealed class TestSqlSagaStorageConfiguration : SqlSagaStorageConfiguration
        {
            internal TestSqlSagaStorageConfiguration(
                DbProviderFactory providerFactory,
                string connectionString,
                SqlIdentifier tableName,
                IMessageTypeResolver messageTypeResolver,
                ISqlMessageSerializerFactory serializerFactory )
                : base( providerFactory, connectionString, tableName, messageTypeResolver, serializerFactory ) { }

            public override DbCommand NewQueryByPropertyCommand( string dataType, string propertyName, object propertyValue )
            {
                var command = base.NewQueryByPropertyCommand( dataType, propertyName, propertyValue );

                command.CommandText = "SELECT TOP(1) Completed " +
                                     $"FROM {TableName.Delimit()} " +
                                      "WHERE DataType = @DataType " +
                                        "AND PropertyName = @PropertyName " +
                                        "AND PropertyValue = @PropertyValue;";

                return command;
            }
        }
    }
}
namespace Contoso.Services.Components
{
    using Domain.Tokens;
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Data.CommandType;
    using static System.Data.SqlClient.SqlBulkCopyOptions;

    public sealed partial class TokenVault : ITokenVault
    {
        readonly SqlEventStoreConfiguration eventStoreConfiguration;

        public TokenVault( SqlEventStoreConfiguration eventStoreConfiguration )
        {
            this.eventStoreConfiguration = eventStoreConfiguration;
        }

        public async Task Deposit( IEnumerable<Token> tokens, CancellationToken cancellationToken )
        {
            const int PrimaryKeyViolation = 2627;

            var events = from token in tokens
                         from @event in token.UncommittedEvents
                         select new EventDescriptor<string>( token.Id, @event );
            var reader = new EventDataDataReader<string>( eventStoreConfiguration, events );

            // TODO: once projections are moved outside of the database, the option to use triggers can be removed to improve performance
            using ( var connection = eventStoreConfiguration.CreateConnection() )
            using ( var bulkInsert = new SqlBulkCopy( connection.ConnectionString, UseInternalTransaction | FireTriggers ) )
            {
                // TODO: this might require additional tuning at scale
                bulkInsert.BatchSize = 1000;
                bulkInsert.DestinationTableName = eventStoreConfiguration.TableName.Delimit();
                bulkInsert.EnableStreaming = true;

                try
                {
                    await bulkInsert.WriteToServerAsync( reader, cancellationToken );
                }
                catch ( SqlException ex ) when ( ex.Errors.Cast<SqlError>().Any( e => e.Number == PrimaryKeyViolation ) )
                {
                    // if there are any duplicates, we've already committed the batch at least once
                }
            }
        }

        public async Task ReleaseToCirculation( Guid mintRequestId, string correlationId, CancellationToken cancellationToken )
        {
            using ( var connection = eventStoreConfiguration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken );

                using ( var command = connection.CreateCommand() )
                {
                    command.CommandText = "[Tokens].[ReleaseToCirculation]";
                    command.CommandType = StoredProcedure;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "MintRequestId";
                    parameter.DbType = DbType.Guid;
                    parameter.Value = mintRequestId;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "CorrelationId";
                    parameter.DbType = DbType.AnsiString;
                    parameter.Size = 50;
                    parameter.Value = correlationId;
                    command.Parameters.Add( parameter );

                    await command.ExecuteNonQueryAsync( cancellationToken );
                }
            }
        }

        public Task RemoveFromCirculation( Guid mintRequestId, string correlationId, CancellationToken cancellationToken )
        {
            // TODO: implement removing minted tokens from circulation
            throw new NotImplementedException();
        }

        public async Task<bool> Transfer( TransferRequest request, CancellationToken cancellationToken )
        {
            var result = 0;

            using ( var connection = eventStoreConfiguration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken );

                using ( var command = connection.CreateCommand() )
                {
                    command.CommandText = "[Tokens].[Transfer]";
                    command.CommandType = StoredProcedure;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "OrderId";
                    parameter.DbType = DbType.Guid;
                    parameter.Value = request.OrderId;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "BillingAccountId";
                    parameter.DbType = DbType.AnsiString;
                    parameter.Size = 25;
                    parameter.Value = request.BillingAccountId;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "CatalogId";
                    parameter.DbType = DbType.AnsiString;
                    parameter.Size = 25;
                    parameter.Value = request.CatalogId;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "Quantity";
                    parameter.DbType = DbType.Int32;
                    parameter.Value = request.Quantity;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "ActivateImmediately";
                    parameter.DbType = DbType.Boolean;
                    parameter.Value = request.ActivateImmediately;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "CorrelationId";
                    parameter.DbType = DbType.AnsiString;
                    parameter.Size = 50;
                    parameter.Value = request.CorrelationId;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "ReturnValue";
                    parameter.DbType = DbType.Int32;
                    parameter.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add( parameter );

                    await command.ExecuteNonQueryAsync( cancellationToken );

                    result = (int) parameter.Value;
                }
            }

            return result == 0;
        }

        public async Task ReverseTransfer( Guid orderId, string correlationId, CancellationToken cancellationToken )
        {
            using ( var connection = eventStoreConfiguration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken );

                using ( var command = connection.CreateCommand() )
                {
                    command.CommandText = "[Tokens].[ReverseTransfer]";
                    command.CommandType = StoredProcedure;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "OrderId";
                    parameter.DbType = DbType.Guid;
                    parameter.Value = orderId;
                    command.Parameters.Add( parameter );

                    parameter = command.CreateParameter();
                    parameter.ParameterName = "CorrelationId";
                    parameter.DbType = DbType.AnsiString;
                    parameter.Size = 50;
                    parameter.Value = correlationId;
                    command.Parameters.Add( parameter );

                    await command.ExecuteNonQueryAsync( cancellationToken );
                }
            }
        }
    }
}
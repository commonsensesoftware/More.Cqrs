namespace Contoso.Services
{
    using More.Domain.Sagas;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.TimeSpan;

    public class SagaPredicate<TData> where TData : class, ISagaData
    {
        SqlSagaStorageConfiguration configuration;
        readonly string dataType;
        readonly string propertyName;
        readonly object propertyValue;

        public SagaPredicate( SqlSagaStorageConfiguration configuration, string propertyName, object propertyValue )
        {
            var type = typeof( TData );

            this.configuration = configuration;
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
            dataType = $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        public Task ToComplete() => ToComplete( 10000 );

        public async Task ToComplete( int timeOutInMilliseconds )
        {
#if DEBUG
            if ( Debugger.IsAttached )
            {
                timeOutInMilliseconds += (int) FromMinutes( 5d ).TotalMilliseconds;
            }
#endif

            using ( var cts = new CancellationTokenSource( timeOutInMilliseconds ) )
            {
                await ToComplete( cts.Token );
            }
        }

        public async Task ToComplete( CancellationToken cancellationToken )
        {
            var completed = false;

            using ( var connection = configuration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken );

                using ( var command = configuration.NewQueryByPropertyCommand( dataType, propertyName, propertyValue ) )
                {
                    command.Connection = connection;

                    do
                    {
                        object result;

                        try
                        {
                            result = await command.ExecuteScalarAsync( cancellationToken );
                        }
                        catch ( OperationCanceledException )
                        {
                            throw new TimeoutException( $"The operation timed out while waiting for a saga correlated by {propertyName} = {propertyValue}." );
                        }
                        catch ( InvalidOperationException )
                        {
                            throw new TimeoutException( $"The operation timed out while waiting for a saga correlated by {propertyName} = {propertyValue}." );
                        }

                        switch ( result )
                        {
                            case null:
                            case DBNull @null:
                                break;
                            case bool @bool:
                                completed = @bool;
                                break;
                        }
                    }
                    while ( !completed );
                }
            }
        }
    }
}
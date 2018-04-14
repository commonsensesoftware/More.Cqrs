namespace Contoso.Domain
{
    using Contoso.Domain.Simulators;
    using Contoso.Domain.Tokens;
    using Contoso.Domain.Tokens.Minting;
    using Contoso.Domain.Tokens.Ordering;
    using Contoso.Domain.Tokens.Printing;
    using More.Domain;
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static System.Threading.Tasks.Task;
    using ReadModel = Contoso.Tokens;

    [Trait( "Kind", "Scenario" )]
    public abstract class ScenarioTest : IAsyncLifetime, IDisposable
    {
        readonly ITestOutputHelper output;
        readonly MintRequestProjector mintRequestProjector = new MintRequestProjector();
        readonly OrderProjector orderProjector = new OrderProjector();
        readonly TokenProjector tokenProjector = new TokenProjector();
        bool disposed;

        protected ScenarioTest( ITestOutputHelper output )
        {
            this.output = output;

            var ignoredMessages = new List<IMessageDescriptor>();
            var pendingOperations = new PendingOperations();

            IgnoredMessages = ignoredMessages;

            var container = CreateContainer( pendingOperations, ignoredMessages );
            var configuration = new MessageBusConfigurationBuilder().HasServiceProvider( container ).CreateConfiguration();

            Bus = new InMemoryMessageBus( configuration, pendingOperations );
            Setup = new ScenarioBuilder( Bus );
            RegisterCommandHandlers( configuration );
            RegisterEventReceivers( configuration );
            RegisterSagas( configuration );
            Bus.Start( new MessageObserver( output ) );
        }

        protected string ScenarioCorrelationId { get; } = Any.CorrelationId;

        protected VirtualClock Clock { get; } = new VirtualClock();

        protected MessageBus Bus { get; }

        protected ScenarioBuilder Setup { get; }

        protected IReadOnlyList<ReadModel.MintRequest> MintRequests => mintRequestProjector.MintRequests;

        protected IReadOnlyList<ReadModel.Order> Orders => orderProjector.Orders;

        protected IReadOnlyList<ReadModel.Token> Tokens => tokenProjector.Tokens;

        protected IReadOnlyList<IMessageDescriptor> IgnoredMessages { get; }

        protected MessageFilter Messages => Bus.Configuration.GetRequiredService<MessageFilter>();

        protected void WriteLine( string message ) => output.WriteLine( message );

        protected void WriteLine( string format, params object[] args ) => output.WriteLine( format, args );

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            Bus.Dispose();
        }

        ServiceContainer CreateContainer( PendingOperations pendingOperations, ICollection<IMessageDescriptor> ignoredMessages )
        {
            var container = new ServiceContainer();

            container.AddService( typeof( MessageFilter ), ( sc, t ) => new MessageFilter( (IObserver<IMessageDescriptor>) sc.GetRequiredService<IMessageReceiver>(), pendingOperations, Clock, ignoredMessages ) );
            container.AddService( typeof( IClock ), ( sc, t ) => Clock );
            container.AddService( typeof( IMessageSender ), ( sc, t ) => new InMemoryMessageSender( sc.GetRequiredService<MessageFilter>(), pendingOperations ) );
            container.AddService( typeof( IMessageReceiver ), ( sc, t ) => new InMemoryMessageReceiver() );
            container.AddService( typeof( IStoreSagaData ), ( sc, t ) => new InMemorySagaStorage() );
            container.AddService( typeof( IMapPersistence ), OnMapPersistence );
            container.AddService( typeof( IEventStore<string> ), ( sc, t ) => new InMemoryEventStore<string>( sc.GetRequiredService<IMapPersistence>().Map( nameof( Token ) ) ) );
            container.AddService( typeof( IRepository<string, Token> ), ( sc, t ) => new Repository<string, Token>( sc.GetRequiredService<IEventStore<string>>() ) );
            container.AddService( typeof( ITokenSecurity ), ( sc, t ) => new TokenSecurity() );
            container.AddService( typeof( ITokenVault ), ( sc, t ) => new TokenVault( sc.GetRequiredService<IRepository<string, Token>>(), Tokens ) );
            container.AddService( typeof( ISaga<MintRequestData> ), ( sc, t ) => new MintRequest() );
            container.AddService( typeof( ISaga<OrderData> ), ( sc, t ) => new Order() );
            container.AddService( typeof( SagaMetadataCollection ), ( sc, t ) => new SagaMetadataCollection( new[] { typeof( MintRequest ), typeof( Order ), typeof( PrintJob ) } ) );

            return container;
        }

        static IMapPersistence OnMapPersistence( IServiceContainer container, Type type )
        {
            var messageSender = (IMessageSender) container.GetService( typeof( IMessageSender ) );
            var sagaStorage = (IStoreSagaData) container.GetService( typeof( IStoreSagaData ) );
            var persistenceMapper = new PersistenceMapper();
            var persistence = new InMemoryPersistence( messageSender, sagaStorage );

            persistenceMapper.Add( nameof( MintRequest ), persistence );
            persistenceMapper.Add( nameof( Order ), persistence );
            persistenceMapper.Add( nameof( Token ), persistence );

            return persistenceMapper;
        }

        void RegisterCommandHandlers( IMessageBusConfiguration configuration )
        {
            RegisterTokenCommandHandlers( configuration );
        }

        void RegisterTokenCommandHandlers( IMessageBusConfiguration configuration )
        {
            var tokens = (IRepository<string, Token>) configuration.GetService( typeof( IRepository<string, Token> ) );
            var tokenVault = (ITokenVault) configuration.GetService( typeof( ITokenVault ) );
            var handler = new TokenStateMediator( tokens, tokenVault );

            configuration.CommandHandlers.Register<ReserveToken>( () => handler );
            configuration.CommandHandlers.Register<RedeemToken>( () => handler );
            configuration.CommandHandlers.Register<VoidToken>( () => handler );
            configuration.CommandHandlers.Register<DeactivateToken>( () => handler );
            configuration.CommandHandlers.Register<UnreserveToken>( () => handler );
            configuration.CommandHandlers.Register<ActivateToken>( () => handler );
        }

        void RegisterEventReceivers( IMessageBusConfiguration configuration )
        {
            RegisterMintRequestProjector( configuration );
            RegisterOrderProjector( configuration );
            RegisterTokenProjector( configuration );
        }

        void RegisterMintRequestProjector( IMessageBusConfiguration configuration )
        {
            configuration.EventReceivers.Register<MintRequested>( () => mintRequestProjector );
            configuration.EventReceivers.Register<Minted>( () => mintRequestProjector );
            configuration.EventReceivers.Register<MintCanceling>( () => mintRequestProjector );
            configuration.EventReceivers.Register<MintCanceled>( () => mintRequestProjector );
        }

        void RegisterOrderProjector( IMessageBusConfiguration configuration )
        {
            configuration.EventReceivers.Register<OrderPlaced>( () => orderProjector );
            configuration.EventReceivers.Register<OrderFulfilled>( () => orderProjector );
            configuration.EventReceivers.Register<OrderCanceled>( () => orderProjector );
            configuration.EventReceivers.Register<TokenReserved>( () => orderProjector );
            configuration.EventReceivers.Register<TokenUnreserved>( () => orderProjector );
        }

        void RegisterTokenProjector( IMessageBusConfiguration configuration )
        {
            configuration.EventReceivers.Register<TokenMinted>( () => tokenProjector );
            configuration.EventReceivers.Register<TokenCirculated>( () => tokenProjector );
            configuration.EventReceivers.Register<TokenReserved>( () => tokenProjector );
            configuration.EventReceivers.Register<TokenActivated>( () => tokenProjector );
            configuration.EventReceivers.Register<TokenRedeemed>( () => tokenProjector );
            configuration.EventReceivers.Register<TokenDeactivated>( () => tokenProjector );
            configuration.EventReceivers.Register<TokenUnreserved>( () => tokenProjector );
            configuration.EventReceivers.Register<TokenVoided>( () => tokenProjector );
        }

        void RegisterSagas( IMessageBusConfiguration configuration )
        {
            RegisterMintingSaga( configuration );
            RegisterOrderSaga( configuration );
        }

        void RegisterMintingSaga( IMessageBusConfiguration configuration )
        {
            Func<MintRequest> activateSaga = () => new MintRequest();

            configuration.CommandHandlers.Register<Mint>( activateSaga );
            configuration.CommandHandlers.Register<StartMintJob>( activateSaga );
            configuration.CommandHandlers.Register<CancelMint>( activateSaga );
            configuration.CommandHandlers.Register<StopMintJob>( activateSaga );
        }

        void RegisterOrderSaga( IMessageBusConfiguration configuration )
        {
            Func<Order> activateSaga = () => new Order();

            configuration.CommandHandlers.Register<PlaceOrder>( activateSaga );
            configuration.CommandHandlers.Register<CancelOrder>( activateSaga );
            configuration.EventReceivers.Register<OrderPlaced>( activateSaga );
            configuration.EventReceivers.Register<OrderTookTooLongToFulfill>( activateSaga );
        }

        public virtual Task InitializeAsync() => Setup.Run();

        public virtual Task DisposeAsync()
        {
            Dispose();
            return CompletedTask;
        }
    }
}
namespace The.Simplest.Possible.Thing
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class InventoryCommandHandlers :
        IHandleCommand<CreateInventoryItem>,
        IHandleCommand<RenameInventoryItem>,
        IHandleCommand<DeactivateInventoryItem>
    {
        readonly IRepository<Guid, InventoryItem> repository;

        public InventoryCommandHandlers( IRepository<Guid, InventoryItem> repository ) => this.repository = repository;

        public async ValueTask Handle( DeactivateInventoryItem command, IMessageContext context, CancellationToken cancellationToken )
        {
            var item = await repository.Single( command.AggregateId, cancellationToken );
            item.Deactivate();
            await repository.Save( item, command.ExpectedVersion, cancellationToken );
        }

        public async ValueTask Handle( RenameInventoryItem command, IMessageContext context, CancellationToken cancellationToken )
        {
            var item = await repository.Single( command.AggregateId, cancellationToken );
            item.Rename( command.NewName );
            await repository.Save( item, command.ExpectedVersion, cancellationToken );
        }

        public ValueTask Handle( CreateInventoryItem command, IMessageContext context, CancellationToken cancellationToken )
        {
            var item = new InventoryItem( command.AggregateId, command.Name );
            return new ValueTask( repository.Save( item, ExpectedVersion.Initial, cancellationToken ) );
        }
    }
}
namespace The.Simplest.Possible.Thing
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using System;
    using System.Threading.Tasks;

    class InventoryCommandHandlers :
        IHandleCommand<CreateInventoryItem>,
        IHandleCommand<RenameInventoryItem>,
        IHandleCommand<DeactivateInventoryItem>
    {
        readonly IRepository<Guid, InventoryItem> repository;

        public InventoryCommandHandlers( IRepository<Guid, InventoryItem> repository ) => this.repository = repository;

        public async Task Handle( DeactivateInventoryItem command, IMessageContext context )
        {
            var item = await repository.Single( command.AggregateId, context.CancellationToken );
            item.Deactivate();
            await repository.Save( item, command.ExpectedVersion, context.CancellationToken );
        }

        public async Task Handle( RenameInventoryItem command, IMessageContext context )
        {
            var item = await repository.Single( command.AggregateId, context.CancellationToken );
            item.Rename( command.NewName );
            await repository.Save( item, command.ExpectedVersion, context.CancellationToken );
        }

        public Task Handle( CreateInventoryItem command, IMessageContext context )
        {
            var item = new InventoryItem( command.AggregateId, command.Name );
            return repository.Save( item, ExpectedVersion.Initial, context.CancellationToken );
        }
    }
}
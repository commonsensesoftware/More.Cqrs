namespace More.Domain
{
    using System;
    using Xunit;

    /// <summary>
    /// This class has intentionally has no code and is never created. Its only purpose is simply to
    /// be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
    /// </summary>
    [CollectionDefinition( "Database" )]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
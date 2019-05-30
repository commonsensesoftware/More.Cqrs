namespace More.Domain
{
    using System;
    using Xunit;

    [Collection( "Database" )]
    public abstract class DatabaseTest<TSetup> : IClassFixture<TSetup> where TSetup : class
    {
        protected DatabaseTest( TSetup setup ) => Setup = setup;

        protected TSetup Setup { get; }
    }
}
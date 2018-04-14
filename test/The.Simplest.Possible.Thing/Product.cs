namespace The.Simplest.Possible.Thing
{
    using System;

    class Product
    {
        public Product( Guid id ) => Id = id;

        public Guid Id { get; }

        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
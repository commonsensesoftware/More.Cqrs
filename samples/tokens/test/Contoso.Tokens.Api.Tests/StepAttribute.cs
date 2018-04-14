namespace Contoso.Services
{
    using System;
    using static System.AttributeTargets;

    [AttributeUsage( Method )]
    public sealed class StepAttribute : Attribute
    {
        public StepAttribute( int number ) { Number = number; }

        public int Number { get; }
    }
}
namespace Contoso.Services.Scenarios
{
    using System;
    using System.Threading.Tasks;

    public interface IScenario
    {
        Func<Task> Create();
    }
}
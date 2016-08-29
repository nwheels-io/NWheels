namespace NWheels.Api.Components
{
    public interface IWorkerComponent
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkerComponent<in TInput, out TOutput> : IWorkerComponent
    {
        TOutput DoWork(TInput input);
    }
}

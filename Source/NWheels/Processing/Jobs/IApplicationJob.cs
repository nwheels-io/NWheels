namespace NWheels.Processing.Jobs
{
    public interface IApplicationJob
    {
        void Execute(IApplicationJobContext context);
        string JobId { get; }
        string Description { get; }
        bool IsReentrant { get; }
        bool NeedsPersistence { get; }
    }
}

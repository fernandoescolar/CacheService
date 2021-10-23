namespace CacheService.Background
{
    internal interface IJob
    { 
        DateTime? DueTime { get; }

        Task ExecuteAsync(CancellationToken cancellationToken);

        IJob UpdateJob(IJob otherJob);
    }
}

namespace CacheService.Background;

internal abstract class Job<T> : IJob
{
    private const int TenYears = 36500;

    public string Key { get; init; }

    public CacheOptions Options { get; protected set; }

    public Func<CancellationToken, ValueTask<T?>> ValueGetter { get; protected set; }

    public Job(JobParameters<T> parameters)
    {
        Key = parameters.Key;
        Options = parameters.Options;
        ValueGetter = parameters.ValueGetter;

        UpdateDueTime(Options);
    }

    public DateTime? DueTime { get; protected set; }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await OnExecuteAsync(cancellationToken);
        UpdateDueTime(Options);
    }

    public IJob UpdateJob(IJob otherJob)
    {
        var j = otherJob as Job<T>;
        if (j is null)
        {
            throw new InvalidOperationException($"otherJob doesn't is a JobDetails<{typeof(T).Name}>");
        }

        return UpdateJob(j);
    }

    public Job<T> UpdateJob(Job<T> otherJob)
    {
        Options = otherJob.Options;
        ValueGetter = otherJob.ValueGetter;
        return this;
    }

    protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);

    private void UpdateDueTime(CacheOptions options)
    {
        DueTime = DateTime.UtcNow + (options.RefreshInterval ?? TimeSpan.FromDays(TenYears));
    }
}

using CacheService.Background;
using CacheService.Core;

namespace CacheService.ChainLinks
{
    internal class StartJobTimer : IChainLink
    {

        public StartJobTimer(JobTimer timer)
        {
        }

        public ushort Order => 0;

        public IChainLink? Next { get; set; }

        public ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T : class
        {
             if (Next is not null)
                return Next.HandleAsync(context);

            return ValueTask.FromResult<T?>(default);
        }
    }
}

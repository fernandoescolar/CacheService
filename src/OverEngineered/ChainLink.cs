namespace CacheService.OverEngineered
{
    public abstract class ChainLink : IChainLink
    {
        public IChainLink? Previous { get; set; }

        public IChainLink? Next { get; set; }

        public async Task<T?> HandleAsync<T>(ChainContext<T> context)  where T: class
        {
            Next = Next ?? throw new ArgumentNullException("Next parameter should be initialized");
            Previous = Previous ?? throw new ArgumentNullException("Previous parameter should be initialized");

            var value = await OnGetAsync(context);
            if (value is null)
            {
                return await Next.HandleAsync(context);
            }

            context.Value = value;
            await Previous.CallbackAsync(context);
            return value;
        }

        public async Task CallbackAsync<T>(ChainContext<T> context)
        {
            Previous = Previous ?? throw new ArgumentNullException("Previous parameter should be initialized");

            await OnSetAsync(context);
            await Previous.CallbackAsync(context);
        }

        protected abstract Task<T?> OnGetAsync<T>(ChainContext<T> context) where T: class;

        protected abstract Task OnSetAsync<T>(ChainContext<T> context);
    }
}
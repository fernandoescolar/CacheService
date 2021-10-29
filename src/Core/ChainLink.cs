namespace CacheService.Core
{
    public abstract class ChainLink : IChainLink
    {
        private ushort _order;

        protected ChainLink(ushort order)
        {
            _order = order;
        }

        public IChainLink? Next { get; set; }

        public ushort Order => _order;

        public async ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T : class
        {
            context.Value = await OnGetAsync(context);
            if (context.Value is null && Next is not null)
            {
                context.Value = await Next.HandleAsync(context);
                if (context.Value is not null)
                {
                    await OnSetAsync(context);
                }
            }

            return context.Value;
        }

        protected virtual ValueTask<T?> OnGetAsync<T>(ChainContext<T> context) where T : class
            => ValueTask.FromResult<T?>(default);

        protected virtual Task OnSetAsync<T>(ChainContext<T> context)
            => Task.CompletedTask;
    }
}
namespace CacheService.OverEngineered
{
    public abstract class ChainOfResponsibility
    {
        private readonly List<IChainLink> _links = new List<IChainLink>();

        public Task<T?> HandleAsync<T>(ChainContext<T> context) where T: class
        {
            var first = _links.FirstOrDefault();
            if (first is null)
                return Task.FromResult<T?>(default);

            return first.HandleAsync(context);
        }

        public void AddLink(IChainLink link)
        {
            var last = _links.LastOrDefault();
            if (last is not null)
            {
                link.Previous = last;
                last.Next = link;
            }

            _links.Add(link);
        }
    }
}
namespace CacheService.Core
{
    internal abstract class ChainOfResponsibility
    {
        private readonly List<IChainLink> _links = new();

        public ValueTask<T?> HandleAsync<T>(ChainContext<T> context) where T: class
        {
            var first = _links.FirstOrDefault();
            if (first is null)
            {
                return ValueTask.FromResult<T?>(default);
            }

            return first.HandleAsync(context);
        }

        public void AddLink(IChainLink link)
        {
            var last = _links.LastOrDefault();
            if (last is not null)
            {
                last.Next = link;
            }

            _links.Add(link);
        }
    }
}
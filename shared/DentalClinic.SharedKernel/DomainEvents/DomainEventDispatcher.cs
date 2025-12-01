namespace DentalClinic.SharedKernel.DomainEvents;

public static class DomainEventDispatcher
{
    private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public static void Subscribe<T>(Action<T> handler) where T : IDomainEvent
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();

        _handlers[type].Add(handler);
    }

    public static void Publish<T>(T domainEvent) where T : IDomainEvent
    {
        var type = typeof(T);
        if (_handlers.ContainsKey(type))
        {
            foreach (var handler in _handlers[type].Cast<Action<T>>())
                handler(domainEvent);
        }
    }
}

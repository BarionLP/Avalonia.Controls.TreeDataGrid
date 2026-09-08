namespace Avalonia.Controls.TreeDataGridTests;

/// <summary>
/// A minimal observable which is also an observer, holding the last value published and
/// replaying it to new subscribers.
/// </summary>
/// <remarks>
/// Stands in for the reactive BehaviorSubject that these tests used before the dependency on
/// System.Reactive was removed. Not thread-safe: tests run on a single dispatcher thread.
/// </remarks>
public sealed class TestSubject<T> : IObservable<T>, IObserver<T>
{
    private readonly List<IObserver<T>> _observers = [];
    private T _value;

    public TestSubject(T value) => _value = value;

    public T Value => _value;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        observer.OnNext(_value);
        return new Subscription(this, observer);
    }

    public IDisposable Subscribe(Action<T> action) => Subscribe(new ActionObserver(action));

    public void OnNext(T value)
    {
        _value = value;

        foreach (var observer in _observers.ToArray())
            observer.OnNext(value);
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }

    private sealed class ActionObserver(Action<T> action) : IObserver<T>
    {
        public void OnNext(T value) => action(value);
        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }

    private sealed class Subscription(TestSubject<T> subject, IObserver<T> observer) : IDisposable
    {
        public void Dispose() => subject._observers.Remove(observer);
    }
}

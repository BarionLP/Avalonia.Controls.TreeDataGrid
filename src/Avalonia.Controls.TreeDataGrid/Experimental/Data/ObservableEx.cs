using System;
using Avalonia.Experimental.Data.Core;
using Avalonia.Reactive;

namespace Avalonia.Experimental.Data;

internal static class ObservableEx
{
    public static IObservable<T> SingleValue<T>(T value) => new SingleValueImpl<T>(value);

    private sealed class SingleValueImpl<T>(T value) : IObservable<T>
    {
        private readonly T _value = value;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(_value);
            return Disposable.Empty;
        }
    }

    extension<T>(IObservable<T> observable)
    {
        public IDisposable Subscribe(Action<T> onNext) => observable.Subscribe(new AnonymousObserver<T>(onNext));
    }
}

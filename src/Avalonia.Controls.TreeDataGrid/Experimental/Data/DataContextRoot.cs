using Avalonia.Controls.Experimental.Data.Core;

namespace Avalonia.Experimental.Data;

internal class DataContextRoot<T>(StyledElement source) : SingleSubscriberObservableBase<T?>
    where T : class
{
    private readonly StyledElement _source = source;

    protected override void Subscribed()
    {
        _source.PropertyChanged += PropertyChanged;
        PublishValue();
    }

    protected override void Unsubscribed()
    {
        _source.PropertyChanged -= PropertyChanged;
    }

    private void PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StyledElement.DataContextProperty)
        {
            PublishValue();
        }
    }

    private void PublishValue()
    {
        if (_source.DataContext is null)
        {
            PublishNext(null);
        }
        else if (_source.DataContext is T value)
        {
            PublishNext(value);
        }
        else
        {
            // TODO: Log DataContext is unexpected type.
        }
    }
}

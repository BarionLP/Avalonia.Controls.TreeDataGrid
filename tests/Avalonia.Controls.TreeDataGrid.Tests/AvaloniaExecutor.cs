using System.Threading;
using Avalonia.Headless;
using TUnit.Core;
using TUnit.Core.Executors;

[assembly: TestExecutor<Avalonia.Controls.TreeDataGridTests.AvaloniaExecutor>]

namespace Avalonia.Controls.TreeDataGridTests;

/// <summary>
/// Runs every test on the Avalonia UI thread of a headless unit test session.
/// </summary>
/// <remarks>
/// The <see cref="AvaloniaTestApplicationAttribute"/> on <see cref="TestApplication"/> only
/// describes which application to start; something has to actually start the session and marshal
/// tests onto its dispatcher thread. The Avalonia.Headless.XUnit and .NUnit packages do this via
/// their own test attributes, so with TUnit we do it here instead. Without this, anything that
/// creates a <see cref="Window"/> fails with "Unable to locate 'Avalonia.Platform.IWindowingPlatform'".
/// </remarks>
public class AvaloniaExecutor : GenericAbstractExecutor
{
    protected override async ValueTask ExecuteAsync(Func<ValueTask> action)
    {
        var session = HeadlessUnitTestSession.GetOrStartForAssembly(typeof(AvaloniaExecutor).Assembly);

        await session.Dispatch<object?>(async () =>
        {
            await action();
            return null;
        }, CancellationToken.None);
    }
}

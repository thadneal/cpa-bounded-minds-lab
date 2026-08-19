using System.Text.Json;
using Cpa.BoundedMindsLab.Experiments;

namespace Cpa.BoundedMindsLab.Observability;

public sealed class ExperimentFrameJournal : IExperimentFrameObserver, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly StreamWriter _writer;
    private bool _disposed;

    public ExperimentFrameJournal(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        _writer = new StreamWriter(path, append: false);
    }

    public void Observe(ExperimentFrame frame)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(JsonSerializer.Serialize(frame, JsonOptions));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _writer.Flush();
        _writer.Dispose();
    }
}

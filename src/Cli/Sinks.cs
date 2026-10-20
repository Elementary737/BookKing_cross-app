using Core;

namespace Cli;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(string message) =>
        Console.WriteLine($"[ConsoleSink] {message}");
}

public sealed class FileSink : INotificationSink
{
    private readonly string _filePath;

    public FileSink(string filePath)
    {
        _filePath = filePath;
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (!File.Exists(_filePath))
        {
            using var _ = File.Create(_filePath);
        }
    }

    public void Notify(string message) =>
        File.AppendAllText(_filePath, $"{DateTimeOffset.UtcNow:HH:mm:ss} [FileSink] {message}{Environment.NewLine}", System.Text.Encoding.UTF8);
}

public sealed class CompositeNotificationSink : INotificationSink
{
    private readonly IEnumerable<INotificationSink> _sinks;

    public CompositeNotificationSink(params INotificationSink[] sinks) => _sinks = sinks;

    public void Notify(string message)
    {
        foreach (var sink in _sinks)
        {
            sink.Notify(message);
        }
    }
}
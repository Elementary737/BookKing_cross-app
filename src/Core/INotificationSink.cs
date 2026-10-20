namespace Core;

public interface INotificationSink
{
    void Notify(string message);
}
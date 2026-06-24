using LibraryManagement.Blazor.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class ToastService
{
    private readonly List<ToastMessage> messages = new();

    public event Action? Changed;

    public IReadOnlyList<ToastMessage> Messages => messages;

    public void Success(string message, FollowUpAction? action = null)
    {
        Add("success", message, action);
    }

    public void Error(string message, FollowUpAction? action = null)
    {
        Add("error", message, action);
    }

    public void Info(string message, FollowUpAction? action = null)
    {
        Add("info", message, action);
    }

    public void Warning(string message, FollowUpAction? action = null)
    {
        Add("warning", message, action);
    }

    public void Remove(Guid id)
    {
        if (messages.RemoveAll(message => message.Id == id) > 0)
        {
            Changed?.Invoke();
        }
    }

    private void Add(string kind, string message, FollowUpAction? action)
    {
        var toast = new ToastMessage
        {
            Id = Guid.NewGuid(),
            Kind = kind,
            Message = message,
            Action = action
        };

        messages.Add(toast);
        Changed?.Invoke();
        _ = RemoveLaterAsync(toast.Id);
    }

    private async Task RemoveLaterAsync(Guid id)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        Remove(id);
    }
}

public sealed class ToastMessage
{
    public Guid Id { get; init; }
    public string Kind { get; init; } = "info";
    public string Message { get; init; } = string.Empty;
    public FollowUpAction? Action { get; init; }
}

#nullable enable

namespace SignalProcessingApp.Services;

public interface IDialogService
{
    void ShowInformation(string message, string title = "情報");
    void ShowWarning(string message, string title = "警告");
    void ShowError(string message, string title = "エラー");
}

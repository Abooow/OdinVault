using System.Collections.ObjectModel;

namespace OdinVault.Components.DialogsProvider;

public sealed class DialogInstance(DialogProvider dialogProvider, DialogOptions options, Task<DialogResult> result, Type component)
{
    public string? Title { get; set; }
    public DialogOptions Options { get; init; } = options;

    public required ReadOnlyDictionary<string, object?> Parameters { get; init; }
    public Type DialogType { get; } = component;
    public Task<DialogResult> Result { get; } = result;

    public void Close(object? data = null)
    {
        dialogProvider.Close(this, DialogResult.Ok(data));
    }

    public void Close(DialogResult result)
    {
        dialogProvider.Close(this, result);
    }

    public void Cancel()
    {
        dialogProvider.Close(this, DialogResult.Cancel());
    }
}

public sealed class DialogOptions
{
    public bool DisableBackdropClick { get; set; }
}
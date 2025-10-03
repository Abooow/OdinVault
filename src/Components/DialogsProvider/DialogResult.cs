namespace OdinVault.Components.DialogsProvider;

public sealed class DialogResult
{
    public object? Data { get; init; }
    public bool IsCancel { get; init; }

    public static DialogResult Ok(object? data = null) => new() { Data = data };
    public static DialogResult Cancel() => new() { IsCancel = true };
}
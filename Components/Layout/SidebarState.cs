namespace mechsystem.Components.Layout;

public sealed class SidebarState
{
    public bool IsCollapsed { get; private set; }

    public event Action? OnChange;

    public void SetCollapsed(bool collapsed)
    {
        if (IsCollapsed == collapsed)
            return;

        IsCollapsed = collapsed;
        NotifyStateChanged();
    }

    public void Toggle()
    {
        IsCollapsed = !IsCollapsed;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

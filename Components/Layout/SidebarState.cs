namespace mechsystem.Components.Layout;

/// <summary>
/// Fonte única de verdade do estado da sidebar (recolhida/expandida).
///
/// Registrado como Scoped. IMPORTANTE: o <c>MainLayout</c> é renderizado em SSR estático, em um escopo
/// de DI próprio (por requisição); o <c>NavMenu</c> é uma ilha interativa e vive no escopo do circuito.
/// Por isso o estado só pode ser consumido por componentes interativos — o elemento <c>.sidebar</c>,
/// os rótulos e os chevrons são todos renderizados pelo <c>NavMenu</c>/<c>NavGroup</c> no MESMO ciclo
/// de renderização a partir deste serviço. O estado vive no circuito, então sobrevive à navegação.
/// </summary>
public sealed class SidebarState
{
    /// <summary>Preferência do usuário (persistida): barra recolhida em modo "rail" de ícones.</summary>
    public bool IsCollapsed { get; private set; }

    /// <summary>Viewport de gaveta mobile (&lt; 768px). Na gaveta nunca há modo recolhido.</summary>
    public bool IsMobile { get; private set; }

    /// <summary>
    /// Estado efetivo usado para renderizar: recolhido SOMENTE no desktop. Quando verdadeiro, o DOM não
    /// contém rótulos e o <c>.sidebar</c> recebe a largura recolhida; quando falso, o inverso.
    /// </summary>
    public bool IsRail => IsCollapsed && !IsMobile;

    /// <summary>Indica que o valor persistido já foi aplicado neste circuito (leitura única).</summary>
    public bool IsInitialized { get; private set; }

    public event Action? OnChange;

    /// <summary>Aplica o valor persistido uma única vez por circuito.</summary>
    public void Initialize(bool collapsed)
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
        IsCollapsed = collapsed;
        NotifyStateChanged();
    }

    public void SetCollapsed(bool collapsed)
    {
        if (IsCollapsed == collapsed)
            return;

        IsCollapsed = collapsed;
        NotifyStateChanged();
    }

    public void Toggle() => SetCollapsed(!IsCollapsed);

    public void SetMobile(bool mobile)
    {
        if (IsMobile == mobile)
            return;

        IsMobile = mobile;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

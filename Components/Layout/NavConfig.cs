using Microsoft.AspNetCore.Components.Routing;

namespace mechsystem.Components.Layout;

public sealed record NavItemDefinition(
    string Title,
    string Href,
    string Icon,
    NavLinkMatch Match = NavLinkMatch.Prefix,
    string? Roles = null,
    string? GroupId = null,
    IReadOnlyList<NavItemDefinition>? Children = null)
{
    public bool IsGroup => !string.IsNullOrWhiteSpace(GroupId) && Children is { Count: > 0 };

    public static NavItemDefinition Link(
        string title,
        string href,
        string icon,
        NavLinkMatch match = NavLinkMatch.Prefix,
        string? roles = null) =>
        new(title, href, icon, match, roles);

    public static NavItemDefinition Group(
        string groupId,
        string title,
        string icon,
        string? roles,
        params NavItemDefinition[] children) =>
        new(title, string.Empty, icon, NavLinkMatch.Prefix, roles, groupId, children);

    public bool MatchesPath(string currentPath)
    {
        var normalized = NormalizePath(currentPath);
        if (!IsGroup || Children is null)
        {
            var target = NormalizePath(Href);
            return Match == NavLinkMatch.All
                ? string.Equals(normalized, target, StringComparison.OrdinalIgnoreCase)
                : IsPathPrefix(normalized, target);
        }

        foreach (var child in Children)
        {
            var childPath = NormalizePath(child.Href);
            if (!string.IsNullOrEmpty(childPath) && IsPathPrefix(normalized, childPath))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var clean = path.Split('?', '#')[0].Trim();
        if (!clean.StartsWith('/'))
            clean = "/" + clean;

        return clean.Length > 1 ? clean.TrimEnd('/') : clean;
    }

    private static bool IsPathPrefix(string currentPath, string prefixPath)
    {
        if (prefixPath == "/")
            return currentPath == "/";

        if (!currentPath.StartsWith(prefixPath, StringComparison.OrdinalIgnoreCase))
            return false;

        return currentPath.Length == prefixPath.Length || currentPath[prefixPath.Length] == '/';
    }
}

public sealed record NavSectionDefinition(
    string Title,
    string? Roles,
    IReadOnlyList<NavItemDefinition> Items)
{
    public static NavSectionDefinition Create(string title, string? roles, params NavItemDefinition[] items) =>
        new(title, roles, items);
}

public static class NavConfig
{
    public const string AdminOnly = "Administrador";
    public const string AdminOrAtendimento = "Administrador, Atendimento";

    // Para adicionar novos itens (ex.: Fila de Compras, Comissões, Pós-venda, Fornecedores),
    // basta incluir uma linha NavItemDefinition.Link(...) ou NavItemDefinition.Group(...) na seção correspondente.
    public static readonly IReadOnlyList<NavSectionDefinition> Sections =
    [
        NavSectionDefinition.Create(
            "Operação",
            roles: null,
            NavItemDefinition.Link("Home", "", "bi-house-door-fill", NavLinkMatch.All),
            NavItemDefinition.Link("Agenda", "/agenda", "bi-calendar-week-fill"),
            NavItemDefinition.Group(
                "os",
                "Ordens de Serviço",
                "bi-file-earmark-text-fill",
                roles: null,
                NavItemDefinition.Link("Nova O.S.", "/ordens-servico/nova", "bi-file-earmark-plus-fill", NavLinkMatch.Prefix, AdminOrAtendimento),
                NavItemDefinition.Link("Listar O.S.", "/ordens-servico", "bi-list-ul", NavLinkMatch.All)),
            NavItemDefinition.Link("Painel Operacional", "/operacional/painel", "bi-wrench-adjustable-circle-fill"),
            NavItemDefinition.Link("Vistorias", "/vistorias", "bi-clipboard-check-fill")),

        NavSectionDefinition.Create(
            "Cadastros",
            roles: null,
            NavItemDefinition.Group(
                "clientes",
                "Clientes",
                "bi-people-fill",
                AdminOrAtendimento,
                NavItemDefinition.Link("Novo Cliente", "/clientes/cadastro", "bi-person-plus-fill"),
                NavItemDefinition.Link("Listar Clientes", "/clientes", "bi-list-ul", NavLinkMatch.All)),
            NavItemDefinition.Group(
                "veiculos",
                "Veículos",
                "bi-car-front-fill",
                AdminOrAtendimento,
                NavItemDefinition.Link("Novo Veículo", "/veiculos/cadastro", "bi-plus-circle-fill"),
                NavItemDefinition.Link("Listar Veículos", "/veiculos", "bi-list-ul", NavLinkMatch.All)),
            NavItemDefinition.Group(
                "servicos",
                "Serviços",
                "bi-tools",
                roles: null,
                NavItemDefinition.Link("Novo Serviço", "/servicos/cadastro", "bi-plus-circle-fill", NavLinkMatch.Prefix, AdminOrAtendimento),
                NavItemDefinition.Link("Listar Serviços", "/servicos", "bi-list-ul", NavLinkMatch.All)),
            NavItemDefinition.Group(
                "estoque",
                "Estoque",
                "bi-box-seam-fill",
                roles: null,
                NavItemDefinition.Link("Nova Peça", "/estoque/cadastro", "bi-plus-circle-fill", NavLinkMatch.Prefix, AdminOrAtendimento),
                NavItemDefinition.Link("Listar Peças", "/estoque", "bi-list-ul", NavLinkMatch.All),
                NavItemDefinition.Link("Entrada", "/estoque/entrada", "bi-box-arrow-in-down", NavLinkMatch.Prefix, AdminOrAtendimento),
                NavItemDefinition.Link("Ajuste", "/estoque/ajuste", "bi-sliders", NavLinkMatch.Prefix, AdminOnly))),

        NavSectionDefinition.Create(
            "Gestão",
            roles: null,
            NavItemDefinition.Link("Relatórios", "/relatorios", "bi-file-earmark-bar-graph-fill")),

        NavSectionDefinition.Create(
            "Administrativo",
            AdminOnly,
            NavItemDefinition.Group(
                "usuarios",
                "Usuários",
                "bi-shield-lock-fill",
                AdminOnly,
                NavItemDefinition.Link("Novo Usuário", "/usuarios/cadastro", "bi-person-plus-fill"),
                NavItemDefinition.Link("Listar Usuários", "/usuarios", "bi-people-fill", NavLinkMatch.All)),
            NavItemDefinition.Link("Configurações", "/configuracoes", "bi-gear", NavLinkMatch.Prefix, AdminOnly))
    ];
}

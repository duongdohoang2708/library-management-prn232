using System.Security.Claims;

namespace LibraryManagement.Blazor.Models;

public sealed record ShellUserState(
    bool IsAuthenticated,
    bool IsAdmin,
    bool IsAdminOrManager,
    bool IsLibrarian,
    bool IsStaff,
    IReadOnlyList<string> Roles,
    string Username,
    string FullName,
    string Email,
    string DisplayName,
    string Initials)
{
    public static ShellUserState Empty { get; } = From(new ClaimsPrincipal(new ClaimsIdentity()));

    public static ShellUserState From(ClaimsPrincipal user)
    {
        var roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList();
        var username = user.FindFirst("Username")?.Value ?? user.Identity?.Name ?? string.Empty;
        var fullName = user.FindFirst("FullName")?.Value ?? username;
        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var displayName = !string.IsNullOrWhiteSpace(fullName) ? fullName : email;
        var initials = GetInitials(displayName);

        return new ShellUserState(
            IsAuthenticated: user.Identity?.IsAuthenticated ?? false,
            IsAdmin: roles.Contains("Admin", StringComparer.OrdinalIgnoreCase),
            IsAdminOrManager: roles.Any(role => role is "Admin" or "Manager"),
            IsLibrarian: roles.Any(role => role == "Librarian"),
            IsStaff: roles.Any(role => role is "Admin" or "Manager" or "Librarian"),
            Roles: roles,
            Username: username,
            FullName: fullName,
            Email: email,
            DisplayName: displayName,
            Initials: initials);
    }

    private static string GetInitials(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "?";
        }

        if (value.Contains('@'))
        {
            return value[..1].ToUpperInvariant();
        }

        return string.Concat(
                value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Take(2)
                    .Select(part => part[0]))
            .ToUpperInvariant();
    }
}

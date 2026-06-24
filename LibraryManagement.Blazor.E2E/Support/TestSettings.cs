namespace LibraryManagement.Blazor.E2E.Support;

public static class TestSettings
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("BLAZOR_BASE_URL")
        ?? "https://localhost:7001";
}

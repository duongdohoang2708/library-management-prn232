using System.Security.Claims;

namespace LibraryManagement.Client.Helpers
{
    public static class ApiActorHeaderHelper
    {
        public static void AddActorHeaders(HttpClient client, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = user.FindFirstValue("FullName")
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? user.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                client.DefaultRequestHeaders.Remove("X-Actor-UserId");
                client.DefaultRequestHeaders.Add("X-Actor-UserId", userId);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                client.DefaultRequestHeaders.Remove("X-Actor-Name");
                client.DefaultRequestHeaders.Add("X-Actor-Name", name);
            }
        }
    }
}

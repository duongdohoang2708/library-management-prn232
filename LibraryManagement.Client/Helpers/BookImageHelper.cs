namespace LibraryManagement.Client.Helpers
{
    public static class BookImageHelper
    {
        public const string Placeholder = "/images/book-placeholder.svg";

        private static readonly Dictionary<string, string> SeedBookCovers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Cho tôi xin một vé đi tuổi thơ"] = "/images/books/acb666ec-2a1c-462f-8e77-b04c3262b6bf_nxbtre_full_05342020_063449.jpg",
            ["Nhà giả kim"] = "/images/books/93b71766-57da-4495-b0b8-879b34a9e1fe_Sách-Nhà-Giả-Kim-reviewsach.net_.jpg",
            ["Rừng Na Uy"] = "/images/books/b9052de0-a9f2-4c84-a27b-e2e7023476b0_rungnauy004-f9a8f341-50e7-47b2-bccf-6923e33c998d.webp",
            ["Harry Potter"] = "/images/books/9f4c69a5-a215-48f4-8591-f0f466e72669_Philosoper%2527s_Stone_New_UK_Cover.webp",
            ["Mật mã Da Vinci"] = "/images/books/f4d97271-de9c-4783-b1be-e58d46a8c819_81CuEX3W9UL._AC_UF1000,1000_QL80_.jpg",
            ["Dế Mèn Phiêu Lưu Ký"] = "/images/books/a77ebebc-2fad-4752-9827-b97f0cc25aed_ebook-de-men-phieu-luu-ky-prc-pdf-epub.jpg",
            ["Chí Phèo"] = "/images/books/d674c587-1336-4180-bc30-8231d5fed083_truyen-ngan-chi-pheo-nam-cao.jpg",
            ["1984"] = "/images/books/ce55d444-db3f-44b3-bb97-31336c14aff5_1984__george_orwell.jpg",
            ["The Shining"] = "/images/books/7a5964d2-f07e-406b-881b-4e3738db10bf_-1747369531.jpg",
            ["Án mạng trên chuyến tàu tốc hành"] = "/images/books/2e756e18-8bbc-4edf-ab15-7629f23ebd2d_nxbtre_full_13462018_124654.jpg"
        };

        public static string Resolve(string? imageUrl, string? title = null)
        {
            var url = imageUrl?.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                if (!string.IsNullOrWhiteSpace(title) && SeedBookCovers.TryGetValue(title.Trim(), out var cover))
                {
                    return cover;
                }

                return Placeholder;
            }

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            if (url.StartsWith("~/", StringComparison.Ordinal))
            {
                return url[1..].Replace("\\", "/");
            }

            return "/" + url.TrimStart('/', '\\').Replace("\\", "/");
        }
    }
}

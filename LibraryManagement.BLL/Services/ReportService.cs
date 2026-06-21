using System.Text;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.DTO.Reports;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace LibraryManagement.BLL.Services
{
    public class ReportService
    {
        private readonly ReportRepository reportRepository;

        public ReportService(ReportRepository _reportRepository)
        {
            reportRepository = _reportRepository;
        }

        public async Task<AdvancedReportResult> GetAdvancedReportAsync(DateTime? from, DateTime? to)
        {
            var end = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
            var start = (from ?? DateTime.UtcNow.AddMonths(-6)).Date;

            return new AdvancedReportResult
            {
                From = start,
                To = end,
                MostBorrowedBooks = await reportRepository.GetMostBorrowedBooksAsync(start, end),
                TopOverdueUsers = await reportRepository.GetTopOverdueUsersAsync(start, end),
                FineRevenueByMonth = await reportRepository.GetFineRevenueByMonthAsync(start, end)
            };
        }

        public async Task<(byte[] Bytes, string ContentType, string FileName)> ExportAsync(string format, DateTime? from, DateTime? to)
        {
            var report = await GetAdvancedReportAsync(from, to);
            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                return (BuildPdf(report), "application/pdf", $"advanced-report-{DateTime.UtcNow:yyyyMMddHHmm}.pdf");
            }

            return (BuildExcel(report), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"advanced-report-{DateTime.UtcNow:yyyyMMddHHmm}.xlsx");
        }

        private static byte[] BuildExcel(AdvancedReportResult report)
        {
            ExcelPackage.License.SetNonCommercialPersonal("LMS Standard");
            using var package = new ExcelPackage();

            var mostBorrowed = package.Workbook.Worksheets.Add("Most Borrowed");
            WriteTitle(mostBorrowed, "Most Borrowed Books", report);
            WriteHeader(mostBorrowed, 3, "Book ID", "Title", "Author", "Borrow Count");
            var row = 4;
            foreach (var item in report.MostBorrowedBooks)
            {
                mostBorrowed.Cells[row, 1].Value = item.BookId;
                mostBorrowed.Cells[row, 2].Value = item.Title;
                mostBorrowed.Cells[row, 3].Value = item.AuthorName;
                mostBorrowed.Cells[row, 4].Value = item.BorrowCount;
                row++;
            }
            mostBorrowed.Cells[mostBorrowed.Dimension.Address].AutoFitColumns();

            var overdue = package.Workbook.Worksheets.Add("Overdue Users");
            WriteTitle(overdue, "Top Overdue Users", report);
            WriteHeader(overdue, 3, "User ID", "Full Name", "Email", "Overdue Count", "Total Fine");
            row = 4;
            foreach (var item in report.TopOverdueUsers)
            {
                overdue.Cells[row, 1].Value = item.UserId;
                overdue.Cells[row, 2].Value = item.FullName;
                overdue.Cells[row, 3].Value = item.Email;
                overdue.Cells[row, 4].Value = item.OverdueCount;
                overdue.Cells[row, 5].Value = item.TotalFine;
                overdue.Cells[row, 5].Style.Numberformat.Format = "#,##0";
                row++;
            }
            overdue.Cells[overdue.Dimension.Address].AutoFitColumns();

            var revenue = package.Workbook.Worksheets.Add("Fine Revenue");
            WriteTitle(revenue, "Fine Revenue By Month", report);
            WriteHeader(revenue, 3, "Month", "Amount");
            row = 4;
            foreach (var item in report.FineRevenueByMonth)
            {
                revenue.Cells[row, 1].Value = item.Month;
                revenue.Cells[row, 2].Value = item.Amount;
                revenue.Cells[row, 2].Style.Numberformat.Format = "#,##0";
                row++;
            }
            revenue.Cells[revenue.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }

        private static void WriteTitle(ExcelWorksheet worksheet, string title, AdvancedReportResult report)
        {
            worksheet.Cells[1, 1].Value = title;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[2, 1].Value = $"From {report.From:yyyy-MM-dd} to {report.To:yyyy-MM-dd}";
        }

        private static void WriteHeader(ExcelWorksheet worksheet, int row, params string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[row, i + 1].Value = headers[i];
            }

            using var range = worksheet.Cells[row, 1, row, headers.Length];
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(234, 179, 8));
        }

        private static byte[] BuildPdf(AdvancedReportResult report)
        {
            var lines = new List<string>
            {
                "LMS Advanced Report",
                $"From {report.From:yyyy-MM-dd} to {report.To:yyyy-MM-dd}",
                "",
                "Most Borrowed Books"
            };
            lines.AddRange(report.MostBorrowedBooks.Select(x => $"{x.BorrowCount} borrows - {x.Title} - {x.AuthorName}"));
            lines.Add("");
            lines.Add("Top Overdue Users");
            lines.AddRange(report.TopOverdueUsers.Select(x => $"{x.OverdueCount} overdue - {x.FullName} - {x.TotalFine:0} VND"));
            lines.Add("");
            lines.Add("Fine Revenue By Month");
            lines.AddRange(report.FineRevenueByMonth.Select(x => $"{x.Month}: {x.Amount:0} VND"));

            return SimplePdfBuilder.Build("LMS Advanced Report", lines);
        }

        private static class SimplePdfBuilder
        {
            public static byte[] Build(string title, List<string> lines)
            {
                var content = new StringBuilder();
                content.AppendLine("BT");
                content.AppendLine("/F1 18 Tf");
                content.AppendLine("50 780 Td");
                content.AppendLine($"({EscapePdf(title)}) Tj");
                content.AppendLine("/F1 10 Tf");
                content.AppendLine("0 -28 Td");

                foreach (var line in lines.Take(45))
                {
                    content.AppendLine($"({EscapePdf(line)}) Tj");
                    content.AppendLine("0 -16 Td");
                }

                content.AppendLine("ET");
                var stream = Encoding.ASCII.GetBytes(content.ToString());
                var objects = new List<byte[]>
                {
                    Encoding.ASCII.GetBytes("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"),
                    Encoding.ASCII.GetBytes("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"),
                    Encoding.ASCII.GetBytes("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n"),
                    Encoding.ASCII.GetBytes("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"),
                    Encoding.ASCII.GetBytes($"5 0 obj\n<< /Length {stream.Length} >>\nstream\n{content}\nendstream\nendobj\n")
                };

                using var ms = new MemoryStream();
                WriteAscii(ms, "%PDF-1.4\n");
                var offsets = new List<long> { 0 };
                foreach (var obj in objects)
                {
                    offsets.Add(ms.Position);
                    ms.Write(obj, 0, obj.Length);
                }

                var xref = ms.Position;
                WriteAscii(ms, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
                foreach (var offset in offsets.Skip(1))
                {
                    WriteAscii(ms, $"{offset:0000000000} 00000 n \n");
                }

                WriteAscii(ms, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
                return ms.ToArray();
            }

            private static string EscapePdf(string value)
            {
                var ascii = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                return ascii.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            }

            private static void WriteAscii(Stream stream, string value)
            {
                var bytes = Encoding.ASCII.GetBytes(value);
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}

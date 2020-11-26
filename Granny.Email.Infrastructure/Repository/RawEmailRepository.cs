using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Granny.Email.Application.Repository;
using OfficeOpenXml;

namespace Granny.Email.Infrastructure.Repository
{
    public class RawEmailRepository : IRawEmailsRepository
    {
        public IEnumerable<(int Label, string EmailText)> GetEmailsFromFile(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var resultList = new List<(int Label, string EmailText)>();

            using var excelPackage = new ExcelPackage(new FileInfo(filePath));
            var worksheet = excelPackage.Workbook.Worksheets["SMSSpamCollection"];

            foreach (var table in worksheet.Tables)
            {
                var start = table.Address.Start;
                var end = table.Address.End;

                for (var row = start.Row; row <= end.Row; ++row)
                {
                    ExcelRange range = worksheet.Cells[row, start.Column, row, end.Column];

                    var classification = range[row, 1].Value.ToString();
                    var text = range[row, 2].Value.ToString();

                    if (classification == string.Empty) break;

                    if (classification != "0" && classification != "1") continue;

                    resultList.Add((classification == "0" ? 0 : 1, text));
                }
            }

            return resultList;
        }
    }
}

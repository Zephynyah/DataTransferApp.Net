using DataTransferApp.Net.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// Service for generating compliance records for transfers.
    /// Supports CSV and Excel formats for audit and regulatory requirements.
    /// </summary>
    public class ComplianceRecordService
    {
        private readonly AppSettings _settings;

        public ComplianceRecordService(AppSettings settings)
        {
            _settings = settings;
            
            // Set EPPlus license (required for non-commercial use)
            ExcelPackage.License.SetNonCommercialPersonal("DataTransferApp.Net");
        }

        /// <summary>
        /// Generates a compliance record for a transfer.
        /// </summary>
        public async Task<string?> GenerateComplianceRecordAsync(TransferLog transfer)
        {
            if (!_settings.GenerateComplianceRecords)
            {
                return null;
            }

            try
            {
                var outputPath = GetComplianceRecordPath(transfer);
                
                if (_settings.ComplianceRecordType.Equals("Standard", StringComparison.OrdinalIgnoreCase))
                {
                    await GenerateStandardRecordAsync(transfer, outputPath);
                }
                else // Comprehensive is default
                {
                    await GenerateComprehensiveRecordAsync(transfer, outputPath);
                }

                LoggingService.Info($"Compliance record generated: {outputPath}");
                return outputPath;
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error generating compliance record", ex);
                return null;
            }
        }

        /// <summary>
        /// Generates compliance records for multiple transfers.
        /// </summary>
        public async Task<List<string>> GenerateBatchComplianceRecordsAsync(IEnumerable<TransferLog> transfers)
        {
            var generatedFiles = new List<string>();

            foreach (var transfer in transfers)
            {
                var filePath = await GenerateComplianceRecordAsync(transfer);
                if (!string.IsNullOrEmpty(filePath))
                {
                    generatedFiles.Add(filePath);
                }
            }

            return generatedFiles;
        }

        /// <summary>
        /// Generates a consolidated compliance report for multiple transfers.
        /// </summary>
        public async Task<string?> GenerateConsolidatedReportAsync(
            IEnumerable<TransferLog> transfers,
            DateTime startDate,
            DateTime endDate,
            string? outputPath = null)
        {
            try
            {
                outputPath ??= GetConsolidatedReportPath(startDate, endDate);

                if (_settings.ComplianceRecordFormat.Equals("Excel", StringComparison.OrdinalIgnoreCase))
                {
                    await GenerateConsolidatedExcelReportAsync(transfers, outputPath, startDate, endDate);
                }
                else if (_settings.ComplianceRecordFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                {
                    await GenerateConsolidatedJsonReportAsync(transfers, outputPath, startDate, endDate);
                }
                else // CSV and TXT use the same format (comma-separated values)
                {
                    await GenerateConsolidatedCsvReportAsync(transfers, outputPath, startDate, endDate);
                }

                LoggingService.Info($"Consolidated compliance report generated: {outputPath}");
                return outputPath;
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error generating consolidated compliance report", ex);
                return null;
            }
        }

        private async Task GenerateComprehensiveRecordAsync(TransferLog transfer, string outputPath)
        {
            if (_settings.ComplianceRecordFormat.Equals("Excel", StringComparison.OrdinalIgnoreCase))
            {
                await GenerateExcelRecordAsync(transfer, outputPath);
            }
            else if (_settings.ComplianceRecordFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
            {
                await GenerateJsonRecordAsync(transfer, outputPath);
            }
            else // CSV and TXT use the same format (comma-separated values)
            {
                await GenerateCsvRecordAsync(transfer, outputPath);
            }
        }

        private async Task GenerateCsvRecordAsync(TransferLog transfer, string outputPath)
        {
            var txt = new StringBuilder();

            // Header section
            txt.AppendLine("Transfer Compliance Record");
            txt.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            txt.AppendLine();

            // Transfer information
            txt.AppendLine("Transfer Information");
            txt.AppendLine($"Transfer ID,{transfer.Id}");
            txt.AppendLine($"Data Transfer Agent,{transfer.TransferInfo.DTA}");
            txt.AppendLine($"Employee ID,{transfer.TransferInfo.Employee}");
            txt.AppendLine($"Transfer Date,{transfer.TransferInfo.Date:yyyy-MM-dd HH:mm:ss}");
            txt.AppendLine($"Folder Name,{transfer.TransferInfo.FolderName}");
            txt.AppendLine($"Source Path,\"{transfer.TransferInfo.SourcePath}\"");
            txt.AppendLine($"Destination Path,\"{transfer.TransferInfo.DestinationPath}\"");
            txt.AppendLine($"Origin,{transfer.TransferInfo.Origin}");
            txt.AppendLine($"Destination,{transfer.TransferInfo.Destination}");
            txt.AppendLine();

            // Summary
            txt.AppendLine("Transfer Summary");
            txt.AppendLine($"Total Files,{transfer.Summary.TotalFiles}");
            txt.AppendLine($"Total Size (bytes),{transfer.Summary.TotalSize}");
            txt.AppendLine($"Total Size (formatted),{FormatFileSize(transfer.Summary.TotalSize)}");
            txt.AppendLine($"Transfer Started,{transfer.Summary.TransferStarted:yyyy-MM-dd HH:mm:ss}");
            txt.AppendLine($"Transfer Completed,{transfer.Summary.TransferCompleted:yyyy-MM-dd HH:mm:ss}");
            txt.AppendLine($"Duration,{(transfer.Summary.TransferCompleted - transfer.Summary.TransferStarted).TotalSeconds:F2} seconds");
            txt.AppendLine($"Status,{transfer.Summary.Status}");
            txt.AppendLine();

            // File list (always included for Comprehensive records)
            if (transfer.Files.Count > 0)
            {
                txt.AppendLine("Transferred Files");
                txt.AppendLine("File Name,Extension,Size (bytes),Size (formatted),Modified Date,Hash,Relative Path,Status");

                foreach (var file in transfer.Files)
                {
                    txt.AppendLine($"\"{EscapeCsv(file.FileName)}\"," +
                                 $"{file.Extension}," +
                                 $"{file.Size}," +
                                 $"\"{FormatFileSize(file.Size)}\"," +
                                 $"{file.Modified:yyyy-MM-dd HH:mm:ss}," +
                                 $"{file.FileHash ?? "N/A"}," +
                                 $"\"{EscapeCsv(file.RelativePath)}\"," +
                                 $"{file.Status}");
                }
            }

            await File.WriteAllTextAsync(outputPath, txt.ToString());
        }

        private async Task GenerateJsonRecordAsync(TransferLog transfer, string outputPath)
        {
            var record = new
            {
                TransferComplianceRecord = new
                {
                    Generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TransferInformation = new
                    {
                        TransferId = transfer.Id.ToString(),
                        DataTransferAgent = transfer.TransferInfo.DTA,
                        EmployeeId = transfer.TransferInfo.Employee,
                        TransferDate = transfer.TransferInfo.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        FolderName = transfer.TransferInfo.FolderName,
                        SourcePath = transfer.TransferInfo.SourcePath,
                        DestinationPath = transfer.TransferInfo.DestinationPath,
                        Origin = transfer.TransferInfo.Origin,
                        Destination = transfer.TransferInfo.Destination
                    },
                    TransferSummary = new
                    {
                        TotalFiles = transfer.Summary.TotalFiles,
                        TotalSizeBytes = transfer.Summary.TotalSize,
                        TotalSizeFormatted = FormatFileSize(transfer.Summary.TotalSize),
                        TransferStarted = transfer.Summary.TransferStarted.ToString("yyyy-MM-dd HH:mm:ss"),
                        TransferCompleted = transfer.Summary.TransferCompleted.ToString("yyyy-MM-dd HH:mm:ss"),
                        DurationSeconds = (transfer.Summary.TransferCompleted - transfer.Summary.TransferStarted).TotalSeconds,
                        Status = transfer.Summary.Status
                    },
                    TransferredFiles = transfer.Files.Select(file => new
                    {
                        FileName = file.FileName,
                        Extension = file.Extension,
                        SizeBytes = file.Size,
                        SizeFormatted = FormatFileSize(file.Size),
                        ModifiedDate = file.Modified.ToString("yyyy-MM-dd HH:mm:ss"),
                        Hash = file.FileHash ?? "N/A",
                        RelativePath = file.RelativePath,
                        Status = file.Status
                    })
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(record, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(outputPath, json);
        }

        private async Task GenerateStandardRecordAsync(TransferLog transfer, string outputPath)
        {
            if (_settings.ComplianceRecordFormat.Equals("Excel", StringComparison.OrdinalIgnoreCase))
            {
                await GenerateStandardExcelRecordAsync(transfer, outputPath);
            }
            else if (_settings.ComplianceRecordFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
            {
                await GenerateStandardJsonRecordAsync(transfer, outputPath);
            }
            else if (_settings.ComplianceRecordFormat.Equals("CSV", StringComparison.OrdinalIgnoreCase))
            {
                await GenerateStandardCsvRecordAsync(transfer, outputPath);
            }
            else // TXT is default
            {
                await GenerateStandardTxtRecordAsync(transfer, outputPath);
            }
        }

        private async Task GenerateStandardTxtRecordAsync(TransferLog transfer, string outputPath)
        {
            var content = new StringBuilder();

            // Standard format header
            content.AppendLine("Date\tFile\tFileFormat\tOriginator\tDTA\tSource\tDestination\tHash\tAlgorithm");

            // File entries
            foreach (var file in transfer.Files)
            {
                var date = transfer.TransferInfo.Date.ToString("yyyyMMdd");
                var fileName = EscapeCsv(file.FileName);
                var fileFormat = file.Extension.TrimStart('.');
                var originator = $"{transfer.TransferInfo.Employee}_{transfer.TransferInfo.Date:yyyyMMdd}_{transfer.TransferInfo.Origin}";
                var dta = transfer.TransferInfo.DTA;
                var source = _settings.ComplianceSourceLocation;
                var destination = ExtractDatasetFromFolderName(transfer.TransferInfo.FolderName);
                var hash = file.FileHash ?? "N/A";
                var algorithm = _settings.HashAlgorithm;

                content.AppendLine($"{date}\t{fileName}\t{fileFormat}\t{originator}\t{dta}\t{source}\t{destination}\t{hash}\t{algorithm}");
            }

            await File.WriteAllTextAsync(outputPath, content.ToString());
        }

        private async Task GenerateStandardCsvRecordAsync(TransferLog transfer, string outputPath)
        {
            var csv = new StringBuilder();

            // Standard format header
            csv.AppendLine("Date,File,FileFormat,Originator,DTA,Source,Destination,Hash,Algorithm");

            // File entries
            foreach (var file in transfer.Files)
            {
                var date = transfer.TransferInfo.Date.ToString("yyyyMMdd");
                var fileName = $"\"{EscapeCsv(file.FileName)}\"";
                var fileFormat = file.Extension.TrimStart('.');
                var originator = $"{transfer.TransferInfo.Employee}_{transfer.TransferInfo.Date:yyyyMMdd}_{transfer.TransferInfo.Origin}";
                var dta = transfer.TransferInfo.DTA;
                var source = $"\"{_settings.ComplianceSourceLocation}\"";
                var destination = $"\"{ExtractDatasetFromFolderName(transfer.TransferInfo.FolderName)}\"";
                var hash = file.FileHash ?? "N/A";
                var algorithm = _settings.HashAlgorithm;

                csv.AppendLine($"{date},{fileName},{fileFormat},{originator},{dta},{source},{destination},{hash},{algorithm}");
            }

            await File.WriteAllTextAsync(outputPath, csv.ToString());
        }

        private async Task GenerateStandardExcelRecordAsync(TransferLog transfer, string outputPath)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Standard Compliance");

            // Headers
            var headers = new[] { "Date", "File", "FileFormat", "Originator", "DTA", "Source", "Destination", "Hash", "Algorithm" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Data rows
            var row = 2;
            foreach (var file in transfer.Files)
            {
                var date = transfer.TransferInfo.Date.ToString("yyyyMMdd");
                var fileName = file.FileName;
                var fileFormat = file.Extension.TrimStart('.');
                var originator = $"{transfer.TransferInfo.Employee}_{transfer.TransferInfo.Date:yyyyMMdd}_{transfer.TransferInfo.Origin}";
                var dta = transfer.TransferInfo.DTA;
                var source = _settings.ComplianceSourceLocation;
                var destination = ExtractDatasetFromFolderName(transfer.TransferInfo.FolderName);
                var hash = file.FileHash ?? "N/A";
                var algorithm = _settings.HashAlgorithm;

                sheet.Cells[row, 1].Value = date;
                sheet.Cells[row, 2].Value = fileName;
                sheet.Cells[row, 3].Value = fileFormat;
                sheet.Cells[row, 4].Value = originator;
                sheet.Cells[row, 5].Value = dta;
                sheet.Cells[row, 6].Value = source;
                sheet.Cells[row, 7].Value = destination;
                sheet.Cells[row, 8].Value = hash;
                sheet.Cells[row, 9].Value = algorithm;
                row++;
            }

            // Auto-fit columns
            for (int i = 1; i <= headers.Length; i++)
            {
                sheet.Column(i).AutoFit();
            }

            await package.SaveAsAsync(new FileInfo(outputPath));
        }

        private async Task GenerateStandardJsonRecordAsync(TransferLog transfer, string outputPath)
        {
            var records = transfer.Files.Select(file => new
            {
                Date = transfer.TransferInfo.Date.ToString("yyyyMMdd"),
                File = file.FileName,
                FileFormat = file.Extension.TrimStart('.'),
                Originator = $"{transfer.TransferInfo.Employee}_{transfer.TransferInfo.Date:yyyyMMdd}_{transfer.TransferInfo.Origin}",
                DTA = transfer.TransferInfo.DTA,
                Source = _settings.ComplianceSourceLocation,
                Destination = ExtractDatasetFromFolderName(transfer.TransferInfo.FolderName),
                Hash = file.FileHash ?? "N/A",
                Algorithm = _settings.HashAlgorithm
            });

            var json = System.Text.Json.JsonSerializer.Serialize(records, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(outputPath, json);
        }

        private async Task GenerateExcelRecordAsync(TransferLog transfer, string outputPath)
        {
            using var package = new ExcelPackage();

            // Sheet 1: Transfer Information
            var infoSheet = package.Workbook.Worksheets.Add("Transfer Information");
            PopulateTransferInfoSheet(infoSheet, transfer);

            // Sheet 2: File List (always included for Comprehensive records)
            if (transfer.Files.Count > 0)
            {
                var filesSheet = package.Workbook.Worksheets.Add("Files");
                PopulateFilesSheet(filesSheet, transfer.Files);
            }

            await package.SaveAsAsync(new FileInfo(outputPath));
        }

        private void PopulateTransferInfoSheet(ExcelWorksheet sheet, TransferLog transfer)
        {
            var row = 1;

            // Title
            sheet.Cells[row, 1].Value = "Transfer Compliance Record";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Font.Size = 16;
            row += 2;

            // Generated timestamp
            sheet.Cells[row, 1].Value = "Generated:";
            sheet.Cells[row, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row += 2;

            // Transfer Information Section
            sheet.Cells[row, 1].Value = "Transfer Information";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Font.Size = 12;
            row++;

            AddInfoRow(sheet, ref row, "Transfer ID", transfer.Id.ToString());
            AddInfoRow(sheet, ref row, "Data Transfer Agent", transfer.TransferInfo.DTA);
            AddInfoRow(sheet, ref row, "Employee ID", transfer.TransferInfo.Employee);
            AddInfoRow(sheet, ref row, "Transfer Date", transfer.TransferInfo.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            AddInfoRow(sheet, ref row, "Folder Name", transfer.TransferInfo.FolderName);
            AddInfoRow(sheet, ref row, "Source Path", transfer.TransferInfo.SourcePath);
            AddInfoRow(sheet, ref row, "Destination Path", transfer.TransferInfo.DestinationPath);
            AddInfoRow(sheet, ref row, "Origin", transfer.TransferInfo.Origin);
            AddInfoRow(sheet, ref row, "Destination", transfer.TransferInfo.Destination);
            row++;

            // Summary Section
            sheet.Cells[row, 1].Value = "Transfer Summary";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Font.Size = 12;
            row++;

            AddInfoRow(sheet, ref row, "Total Files", transfer.Summary.TotalFiles.ToString());
            AddInfoRow(sheet, ref row, "Total Size (bytes)", transfer.Summary.TotalSize.ToString());
            AddInfoRow(sheet, ref row, "Total Size", FormatFileSize(transfer.Summary.TotalSize));
            AddInfoRow(sheet, ref row, "Transfer Started", transfer.Summary.TransferStarted.ToString("yyyy-MM-dd HH:mm:ss"));
            AddInfoRow(sheet, ref row, "Transfer Completed", transfer.Summary.TransferCompleted.ToString("yyyy-MM-dd HH:mm:ss"));
            AddInfoRow(sheet, ref row, "Duration (seconds)", 
                (transfer.Summary.TransferCompleted - transfer.Summary.TransferStarted).TotalSeconds.ToString("F2"));
            AddInfoRow(sheet, ref row, "Status", transfer.Summary.Status);

            // Auto-fit columns
            sheet.Column(1).AutoFit();
            sheet.Column(2).AutoFit();
        }

        private void PopulateFilesSheet(ExcelWorksheet sheet, List<TransferredFile> files)
        {
            // Headers
            var headers = new[] { "File Name", "Extension", "Size (bytes)", "Size", "Modified Date", "Hash", "Relative Path", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Data rows
            var row = 2;
            foreach (var file in files)
            {
                sheet.Cells[row, 1].Value = file.FileName;
                sheet.Cells[row, 2].Value = file.Extension;
                sheet.Cells[row, 3].Value = file.Size;
                sheet.Cells[row, 4].Value = FormatFileSize(file.Size);
                sheet.Cells[row, 5].Value = file.Modified.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cells[row, 6].Value = file.FileHash ?? "N/A";
                sheet.Cells[row, 7].Value = file.RelativePath;
                sheet.Cells[row, 8].Value = file.Status;
                row++;
            }

            // Auto-fit all columns
            for (int i = 1; i <= headers.Length; i++)
            {
                sheet.Column(i).AutoFit();
            }
        }

        private async Task GenerateConsolidatedCsvReportAsync(
            IEnumerable<TransferLog> transfers,
            string outputPath,
            DateTime startDate,
            DateTime endDate)
        {
            var csv = new StringBuilder();

            // Header
            csv.AppendLine("Consolidated Transfer Compliance Report");
            csv.AppendLine($"Period,{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine($"Total Transfers,{transfers.Count()}");
            csv.AppendLine();

            // Transfer list
            csv.AppendLine("Transfer ID,Date,DTA,Employee,Folder Name,Files,Total Size,Duration (sec),Status,Source,Destination");

            foreach (var transfer in transfers.OrderBy(t => t.TransferInfo.Date))
            {
                var duration = (transfer.Summary.TransferCompleted - transfer.Summary.TransferStarted).TotalSeconds;
                csv.AppendLine($"{transfer.Id}," +
                             $"{transfer.TransferInfo.Date:yyyy-MM-dd HH:mm:ss}," +
                             $"{transfer.TransferInfo.DTA}," +
                             $"{transfer.TransferInfo.Employee}," +
                             $"\"{EscapeCsv(transfer.TransferInfo.FolderName)}\"," +
                             $"{transfer.Summary.TotalFiles}," +
                             $"{transfer.Summary.TotalSize}," +
                             $"{duration:F2}," +
                             $"{transfer.Summary.Status}," +
                             $"\"{EscapeCsv(transfer.TransferInfo.SourcePath)}\"," +
                             $"\"{EscapeCsv(transfer.TransferInfo.DestinationPath)}\"");
            }

            await File.WriteAllTextAsync(outputPath, csv.ToString());
        }

        private async Task GenerateConsolidatedJsonReportAsync(
            IEnumerable<TransferLog> transfers,
            string outputPath,
            DateTime startDate,
            DateTime endDate)
        {
            var record = new
            {
                ConsolidatedTransferComplianceReport = new
                {
                    Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    Generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalTransfers = transfers.Count(),
                    Transfers = transfers.OrderBy(t => t.TransferInfo.Date).Select(transfer => new
                    {
                        TransferId = transfer.Id.ToString(),
                        Date = transfer.TransferInfo.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        DTA = transfer.TransferInfo.DTA,
                        Employee = transfer.TransferInfo.Employee,
                        FolderName = transfer.TransferInfo.FolderName,
                        Files = transfer.Summary.TotalFiles,
                        TotalSize = transfer.Summary.TotalSize,
                        DurationSeconds = (transfer.Summary.TransferCompleted - transfer.Summary.TransferStarted).TotalSeconds,
                        Status = transfer.Summary.Status,
                        Source = transfer.TransferInfo.SourcePath,
                        Destination = transfer.TransferInfo.DestinationPath
                    })
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(record, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(outputPath, json);
        }

        private async Task GenerateConsolidatedExcelReportAsync(
            IEnumerable<TransferLog> transfers,
            string outputPath,
            DateTime startDate,
            DateTime endDate)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Consolidated Report");

            var row = 1;

            // Title
            sheet.Cells[row, 1].Value = "Consolidated Transfer Compliance Report";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Font.Size = 16;
            row += 2;

            // Period info
            sheet.Cells[row, 1].Value = "Period:";
            sheet.Cells[row, 2].Value = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
            row++;
            sheet.Cells[row, 1].Value = "Generated:";
            sheet.Cells[row, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row++;
            sheet.Cells[row, 1].Value = "Total Transfers:";
            sheet.Cells[row, 2].Value = transfers.Count();
            row += 2;

            // Headers
            var headers = new[] { "Transfer ID", "Date", "DTA", "Employee", "Folder Name", "Files", "Total Size", "Duration (sec)", "Status", "Source", "Destination" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[row, i + 1].Value = headers[i];
                sheet.Cells[row, i + 1].Style.Font.Bold = true;
            }
            row++;

            // Data
            foreach (var transfer in transfers.OrderBy(t => t.TransferInfo.Date))
            {
                var duration = (transfer.Summary.TransferCompleted - transfer.Summary.TransferStarted).TotalSeconds;
                sheet.Cells[row, 1].Value = transfer.Id.ToString();
                sheet.Cells[row, 2].Value = transfer.TransferInfo.Date.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cells[row, 3].Value = transfer.TransferInfo.DTA;
                sheet.Cells[row, 4].Value = transfer.TransferInfo.Employee;
                sheet.Cells[row, 5].Value = transfer.TransferInfo.FolderName;
                sheet.Cells[row, 6].Value = transfer.Summary.TotalFiles;
                sheet.Cells[row, 7].Value = transfer.Summary.TotalSize;
                sheet.Cells[row, 8].Value = duration;
                sheet.Cells[row, 9].Value = transfer.Summary.Status;
                sheet.Cells[row, 10].Value = transfer.TransferInfo.SourcePath;
                sheet.Cells[row, 11].Value = transfer.TransferInfo.DestinationPath;
                row++;
            }

            // Auto-fit columns
            for (int i = 1; i <= headers.Length; i++)
            {
                sheet.Column(i).AutoFit();
            }

            await package.SaveAsAsync(new FileInfo(outputPath));
        }

        private void AddInfoRow(ExcelWorksheet sheet, ref int row, string label, string value)
        {
            sheet.Cells[row, 1].Value = label + ":";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 2].Value = value;
            row++;
        }

        private string GetComplianceRecordPath(TransferLog transfer)
        {
            var directory = _settings.TransferRecordsDirectory;

            Directory.CreateDirectory(directory);

            var timestamp = transfer.TransferInfo.Date.ToString("yyyyMMdd_HHmmss");
            
            string extension;
            if (_settings.ComplianceRecordFormat.Equals("Excel", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".xlsx";
            }
            else if (_settings.ComplianceRecordFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".json";
            }
            else if (_settings.ComplianceRecordFormat.Equals("TXT", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".txt";
            }
            else // CSV is default
            {
                extension = ".csv";
            }

            var fileName = $"Compliance_{transfer.TransferInfo.FolderName}_{timestamp}{extension}";
            return Path.Combine(directory, fileName);
        }

        private string GetConsolidatedReportPath(DateTime startDate, DateTime endDate)
        {
            var directory = _settings.TransferRecordsDirectory;

            Directory.CreateDirectory(directory);

            string extension;
            if (_settings.ComplianceRecordFormat.Equals("Excel", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".xlsx";
            }
            else if (_settings.ComplianceRecordFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".json";
            }
            else // CSV and TXT both use .csv extension for consolidated reports
            {
                extension = ".csv";
            }

            var fileName = $"ConsolidatedReport_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            return Path.Combine(directory, fileName);
        }

        private string ExtractDatasetFromFolderName(string folderName)
        {
            // Extract dataset from folder name (e.g., "Ben_2026-01-16_UG" -> "UG")
            // Look for the last part after the date pattern
            var parts = folderName.Split('_');
            if (parts.Length >= 3)
            {
                // Find the last part that looks like a dataset (typically 2-10 uppercase letters)
                for (int i = parts.Length - 1; i >= 0; i--)
                {
                    var part = parts[i];
                    if (part.Length >= 2 && part.Length <= 10 && part.All(char.IsLetter) && part.All(char.IsUpper))
                    {
                        return part;
                    }
                }
            }
            return "Unknown";
        }

        private string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return value.Replace("\"", "\"\"");
            }
            return value;
        }

        private string FormatFileSize(long bytes)
        {
            const long GB = 1024 * 1024 * 1024;
            const long MB = 1024 * 1024;
            const long KB = 1024;

            if (bytes >= GB)
                return $"{bytes / (double)GB:N2} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:N2} MB";
            if (bytes >= KB)
                return $"{bytes / (double)KB:N2} KB";

            return $"{bytes} bytes";
        }
    }
}

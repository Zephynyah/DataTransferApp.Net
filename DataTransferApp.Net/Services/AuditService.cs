using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Services
{
    public class AuditService
    {
        private readonly AppSettings _settings;

        public AuditService(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task<AuditResult> AuditFolderAsync(string folderPath, string folderName)
        {
            return await Task.Run(() => AuditFolder(folderPath, folderName));
        }

        public AuditResult AuditFolder(string folderPath, string folderName)
        {
            var auditResult = InitializeAuditResult(folderPath, folderName);

            try
            {
                PerformNameValidation(auditResult);
                PerformDatasetValidation(auditResult);
                PerformExtensionValidation(auditResult, folderPath);
                DetermineOverallStatus(auditResult);

                LoggingService.Info($"Audit completed for '{folderName}': {auditResult.OverallStatus}");
            }
            catch (Exception ex)
            {
                HandleAuditError(auditResult, ex);
            }

            return auditResult;
        }

        private static void HandleAuditError(AuditResult auditResult, Exception ex)
        {
            auditResult.OverallStatus = "Error";
            auditResult.Issues.Add($"Audit error: {ex.Message}");
            LoggingService.Error($"Audit failed for '{auditResult.FolderName}'", ex);
        }

        private static void DetermineOverallStatus(AuditResult auditResult)
        {
            if (auditResult.NameValidation?.IsValid == true &&
                auditResult.ExtensionValidation?.IsValid == true &&
                auditResult.DatasetValidation?.IsValid == true)
            {
                auditResult.OverallStatus = "Passed";
                auditResult.CanTransfer = true;
            }
            else
            {
                auditResult.OverallStatus = "Failed";
                auditResult.CanTransfer = false;
            }
        }

        private static bool IsValidDateFormat(string dateStr)
        {
            try
            {
                var year = dateStr.Substring(0, 4);
                var month = dateStr.Substring(4, 2);
                var day = dateStr.Substring(6, 2);
                DateTime.ParseExact($"{year}-{month}-{day}", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private AuditResult InitializeAuditResult(string folderPath, string folderName)
        {
            return new AuditResult
            {
                FolderName = folderName,
                FolderPath = folderPath,
                Timestamp = DateTime.Now
            };
        }

        private void PerformNameValidation(AuditResult auditResult)
        {
            auditResult.NameValidation = ValidateFolderName(auditResult.FolderName);
            if (!auditResult.NameValidation.IsValid)
            {
                auditResult.Issues.Add($"Naming: {auditResult.NameValidation.Message}");
            }
        }

        private void PerformDatasetValidation(AuditResult auditResult)
        {
            if (_settings.AuditStrategy.Contains("WhitelistDatasets"))
            {
                auditResult.DatasetValidation = ValidateDataset(auditResult.NameValidation!);
                if (!auditResult.DatasetValidation.IsValid)
                {
                    auditResult.Issues.Add($"Dataset: {auditResult.DatasetValidation.Message}");
                }
            }
            else
            {
                auditResult.DatasetValidation = new DatasetValidation
                {
                    IsValid = true,
                    Dataset = auditResult.NameValidation?.Dataset ?? string.Empty,
                    Message = "Dataset validation disabled"
                };
            }
        }

        private void PerformExtensionValidation(AuditResult auditResult, string folderPath)
        {
            if (_settings.AuditStrategy.Contains("BlacklistExtensions"))
            {
                auditResult.ExtensionValidation = ValidateFileExtensions(folderPath);
                if (!auditResult.ExtensionValidation.IsValid)
                {
                    auditResult.Issues.Add($"Extensions: {auditResult.ExtensionValidation.Message}");
                }
            }
            else
            {
                auditResult.ExtensionValidation = new ExtensionValidation
                {
                    IsValid = true,
                    Message = "Extension validation disabled"
                };
            }
        }

        private NameValidation ValidateFolderName(string folderName)
        {
            var regex = new Regex(_settings.FolderNameRegex, RegexOptions.Compiled, TimeSpan.FromSeconds(5));
            var parts = folderName.Split('_');

            if (regex.IsMatch(folderName))
            {
                return ValidateValidFormat(parts);
            }
            else if (parts.Length == 3)
            {
                return ValidateThreePartFormat(parts);
            }
            else if (parts.Length == 4)
            {
                return ValidateFourPartFormat();
            }
            else
            {
                return CreateInvalidFormatResult();
            }
        }

        private NameValidation ValidateValidFormat(string[] parts)
        {
            var result = new NameValidation
            {
                IsValid = true,
                EmployeeId = parts[0],
                Date = parts[1],
                Dataset = parts[2],
                Sequence = parts.Length > 3 ? parts[3] : null,
                Message = "Valid folder name format"
            };

            if (!IsValidDateFormat(parts[1]))
            {
                result.IsValid = false;
                result.Message = "Invalid date format in folder name";
            }

            return result;
        }

        private NameValidation ValidateThreePartFormat(string[] parts)
        {
            var result = new NameValidation
            {
                IsValid = false,
                EmployeeId = parts[0],
                Date = parts[1],
                Dataset = parts[2],
                Message = "Invalid format detected in folder name"
            };

            if (!IsValidDateFormat(parts[1]))
            {
                result.Message = "Invalid date format in folder name";
            }

            return result;
        }

        private NameValidation ValidateFourPartFormat()
        {
            return new NameValidation
            {
                IsValid = false,
                Message = "Either employee Name/ID, Date, Dataset or Sequence has an invalid format."
            };
        }

        private NameValidation CreateInvalidFormatResult()
        {
            return new NameValidation
            {
                IsValid = false,
                Message = "Folder required pattern: employee_yyyymmdd_dataset[_sequence]."
            };
        }

        private DatasetValidation ValidateDataset(NameValidation nameValidation)
        {
            if (!string.IsNullOrEmpty(nameValidation.Dataset))
            {
                var isWhitelisted = _settings.WhiteListDatasets.Any(d => string.Equals(d, nameValidation.Dataset, StringComparison.OrdinalIgnoreCase));

                return new DatasetValidation
                {
                    IsValid = isWhitelisted,
                    Dataset = nameValidation.Dataset,
                    Message = isWhitelisted
                        ? $"Dataset '{nameValidation.Dataset}' is whitelisted"
                        : $"Dataset '{nameValidation.Dataset}' is not in the whitelist. Allowed: {string.Join(", ", _settings.WhiteListDatasets)}"
                };
            }
            else
            {
                return new DatasetValidation
                {
                    IsValid = false,
                    Dataset = null,
                    Message = "Cannot validate dataset"
                };
            }
        }

        private ExtensionValidation ValidateFileExtensions(string folderPath)
        {
            var violations = new List<FileViolation>();

            try
            {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (_settings.BlacklistedExtensions.Contains(ext))
                    {
                        violations.Add(new FileViolation
                        {
                            File = file,
                            Extension = ext,
                            RelativePath = file.Replace(folderPath, string.Empty).TrimStart(Path.DirectorySeparatorChar)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error validating extensions in '{folderPath}'", ex);
            }

            return new ExtensionValidation
            {
                IsValid = violations.Count == 0,
                Violations = violations,
                Message = violations.Count == 0
                    ? "No blacklisted file extensions found"
                    : $"Found {violations.Count} file(s) with blacklisted extensions"
            };
        }
    }
}
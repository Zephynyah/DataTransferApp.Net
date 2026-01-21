using DataTransferApp.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            var auditResult = new AuditResult
            {
                FolderName = folderName,
                FolderPath = folderPath,
                Timestamp = DateTime.Now
            };

            try
            {
                // Test folder naming
                auditResult.NameValidation = ValidateFolderName(folderName);
                if (!auditResult.NameValidation.IsValid)
                {
                    auditResult.Issues.Add($"Naming: {auditResult.NameValidation.Message}");
                }

                // Test dataset whitelist (only if in audit strategy)
                if (_settings.AuditStrategy.Contains("WhitelistDatasets"))
                {
                    auditResult.DatasetValidation = ValidateDataset(auditResult.NameValidation);
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
                        Dataset = auditResult.NameValidation.Dataset,
                        Message = "Dataset validation disabled"
                    };
                }

                // Test file extensions
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

                // Determine overall status
                if (auditResult.NameValidation.IsValid &&
                    auditResult.ExtensionValidation.IsValid &&
                    auditResult.DatasetValidation.IsValid)
                {
                    auditResult.OverallStatus = "Passed";
                    auditResult.CanTransfer = true;
                }
                else
                {
                    auditResult.OverallStatus = "Failed";
                    auditResult.CanTransfer = false;
                }

                LoggingService.Info($"Audit completed for '{folderName}': {auditResult.OverallStatus}");
            }
            catch (Exception ex)
            {
                auditResult.OverallStatus = "Error";
                auditResult.Issues.Add($"Audit error: {ex.Message}");
                LoggingService.Error($"Audit failed for '{folderName}'", ex);
            }

            return auditResult;
        }

        private NameValidation ValidateFolderName(string folderName)
        {
            var regex = new Regex(_settings.FolderNameRegex);

            if (regex.IsMatch(folderName))
            {
                var parts = folderName.Split('_');

                var result = new NameValidation
                {
                    IsValid = true,
                    EmployeeId = parts[0],
                    Date = parts[1],
                    Dataset = parts[2],
                    Sequence = parts.Length > 3 ? parts[3] : null,
                    Message = "Valid folder name format"
                };

                // Validate date format
                try
                {
                    var dateStr = parts[1];
                    var year = dateStr.Substring(0, 4);
                    var month = dateStr.Substring(4, 2);
                    var day = dateStr.Substring(6, 2);
                    DateTime.ParseExact($"{year}-{month}-{day}", "yyyy-MM-dd", null);
                }
                catch
                {
                    result.IsValid = false;
                    result.Message = "Invalid date format in folder name";
                }

                return result;
            }
            else
            {
                return new NameValidation
                {
                    IsValid = false,
                    Message = "Required pattern: employeeid_yyyymmdd_dataset[_sequence]"
                };
            }
        }

        private DatasetValidation ValidateDataset(NameValidation nameValidation)
        {
            if (nameValidation.IsValid && !string.IsNullOrEmpty(nameValidation.Dataset))
            {
                var isWhitelisted = _settings.WhiteListDatasets.Contains(nameValidation.Dataset);

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
                    var ext = Path.GetExtension(file).ToLower();
                    if (_settings.BlacklistedExtensions.Contains(ext))
                    {
                        violations.Add(new FileViolation
                        {
                            File = file,
                            Extension = ext,
                            RelativePath = file.Replace(folderPath, "").TrimStart(Path.DirectorySeparatorChar)
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

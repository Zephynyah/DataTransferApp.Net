using System;
using System.IO;
using DataTransferApp.Net.Models;
using LiteDB;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1518:File may not end with a newline character", Justification = "File does end with newline", Scope = "namespace", Target = "DataTransferApp.Net.Services")]

namespace DataTransferApp.Net.Services
{
    public class SettingsService : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<AppSettings> _collection;
        private bool _disposed;

        public SettingsService(string dbPath)
        {
            // Ensure directory exists
            var dbDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
                LoggingService.Info($"Created database directory: {dbDir}");
            }

            _db = new LiteDatabase(dbPath);
            _collection = _db.GetCollection<AppSettings>("settings");

            LoggingService.Info($"Settings database initialized at: {dbPath}");

            // Ensure default settings exist
            EnsureDefaultSettings();
        }

        public AppSettings GetSettings()
        {
            var settings = _collection.FindById(1);
            return settings ?? new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            settings.Id = 1; // Ensure we're always updating the same record
            settings.LastModified = DateTime.Now;
            _collection.Update(settings);
            LoggingService.Info("Settings saved to database");
        }

        public void ResetToDefaults()
        {
            var defaultSettings = new AppSettings { Id = 1 };
            _collection.Update(defaultSettings);
            LoggingService.Info("Settings reset to defaults");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }

                _disposed = true;
            }
        }

        private void EnsureDefaultSettings()
        {
            var settings = _collection.FindById(1);
            if (settings == null)
            {
                settings = new AppSettings();
                _collection.Insert(settings);
                LoggingService.Info("Default settings created");
            }
        }
    }
}


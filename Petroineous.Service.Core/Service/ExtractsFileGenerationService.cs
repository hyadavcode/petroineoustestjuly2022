using Petroineous.Service.Core.Common;
using Petroineous.Service.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineous.Service.Core
{
    public class ExtractsFileGenerationService : IExtractsFileGenerationService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private object _powerExtractFileLock = new object();
        private Dictionary<string, IEnumerable<string>> _extractFileHeaders = new Dictionary<string, IEnumerable<string>>();

        public ExtractsFileGenerationService(ILogger logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._config = configuration;
            LoadFileHeadersMappings();
        }
      
        public async Task CreatePowerExtractFile(DateTime extractDate, IEnumerable<KeyValuePair<string, string>> data)
        {
            try
            {
                await _logger.LogInfo("Starting Power Position extract file generation");

                var outputDir = _config.PowerExtractOutputDirectory;

                if (!Directory.Exists(outputDir))
                    throw new ConfigurationErrorsException($"Invalid power extract output directory found in configuration. Diectory = {outputDir}");

                var filename = $"PowerPosition_{extractDate:yyyyMMdd_hhmm}.csv";
                var filePath = Path.Combine(outputDir, filename);

                if (!_extractFileHeaders.ContainsKey(Constants.ExtractType.IntradayPowerPosition))
                    throw new OperationCanceledException($"Extract file generation aborted. No headers defined for the extract type {Constants.ExtractType.IntradayPowerPosition}");

                var fileHeaders = string.Join(",", _extractFileHeaders[Constants.ExtractType.IntradayPowerPosition]);

                if (File.Exists(filePath))
                    throw new OperationCanceledException($"File already exists. File = {filePath}");

                lock (_powerExtractFileLock)
                {
                    using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
                    {
                        using (var strWriter = new StreamWriter(fileStream))
                        {
                            // Write file header
                            strWriter.WriteLine(fileHeaders);

                            // Write all data to the file
                            foreach(var dataPoint in data)
                            {
                                strWriter.WriteLine($"{dataPoint.Key}, {dataPoint.Value}");
                            }
                        }
                    }
                }

                await _logger.LogInfo($"Power position extract generation completed successfully. 1 file generated = {filePath}");
            }
            catch(Exception ex)
            {
                await _logger.LogError($"Error in {nameof(ExtractsFileGenerationService.CreatePowerExtractFile)}", ex);
            }
        }

        private void LoadFileHeadersMappings()
        {
            _extractFileHeaders.Add(Constants.ExtractType.IntradayPowerPosition, new List<string>() { "Local Time", "Volume" });
        }
    }
}

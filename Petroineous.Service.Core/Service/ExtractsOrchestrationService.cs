using Petroineous.Service.Core.Common;
using Petroineous.Service.Core.Configuration;
using Petroineous.Service.Core.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Petroineous.Service.Core
{
    public class ExtractsOrchestrationService : IExtractsOrchestrationService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPositionDataService _positionDataService;
        private readonly IExtractsFileGenerationService _extractFileService;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private volatile ServiceStatus _status = ServiceStatus.Stopped;

        // stores currently running extract types against each timestamp
        private readonly ConcurrentDictionary<string, List<DateTime>> _activelyRunningExtracts = new ConcurrentDictionary<string, List<DateTime>>();

        private IDisposable _powerExtractScheduler;

        public ExtractsOrchestrationService(IEventAggregator eventAggregator,
            IPositionDataService positionDataService,
            IExtractsFileGenerationService extractFileGenerationService,
            IConfiguration configuration,
            ILogger logger)
        {
            this._eventAggregator = eventAggregator;
            this._positionDataService = positionDataService;
            this._extractFileService = extractFileGenerationService;
            this._configuration = configuration;
            this._logger = logger;
        }

        public ServiceStatus Status => _status;

        public async Task Start()
        {
            try
            {
                await _logger.LogInfo("Starting: Extracts Orchestration service");

                var powerExtractInterval = _configuration.IntradayExtractScheduleInterval;

                // start extract run scheduler for power extracts
                _powerExtractScheduler = _eventAggregator
                                            .ObserveInterval(TimeSpan.FromSeconds(powerExtractInterval), Constants.ExtractType.IntradayPowerPosition)
                                            .Subscribe(async x => await RunExtract(x.EventName, x.Timestamp));

                this._status = ServiceStatus.Started;

                // run once on start
                await RunExtract(Constants.ExtractType.IntradayPowerPosition, DateTime.Today);

                await _logger.LogInfo("Started: Extracts Orchestration service");
            }
            catch (Exception ex)
            {
                this._status = ServiceStatus.Error;
                await _logger.LogError("Error: In starting Extracts Orchestration service", ex);
            }
        }

        public async Task Stop()
        {
            try
            {
                await _logger.LogInfo("Stopping: Extracts Orchestration service");

                var waitStartTimestamp = DateTime.Now;

                while (_activelyRunningExtracts.Any())
                {
                    Thread.Sleep(1000);

                    // terminate after wait
                    if ((DateTime.Now - waitStartTimestamp).TotalSeconds >= _configuration.ExtractOrchestratorStopWaitTimespan)
                        break;
                }

                // stop the timer / scheduler
                _powerExtractScheduler?.Dispose();

                // clear active tasks cache
                _activelyRunningExtracts.Clear();

                this._status = ServiceStatus.Stopped;
                await _logger.LogInfo("Stopped: Extracts Orchestration service");
            }
            catch (Exception ex)
            {
                this._status = ServiceStatus.Error;
                await _logger.LogError("Error: In stopping Extracts Orchestration service", ex);
            }
        }

        private async Task RunExtract(string extractName, DateTime runTimestamp)
        {
            try
            {
                await _logger.LogInfo($"Starting: Extract run for extract type = {extractName}. Local time = {runTimestamp:G}");

                if (string.Equals(extractName, Constants.ExtractType.IntradayPowerPosition, StringComparison.InvariantCultureIgnoreCase))
                    await RunPowerGenerationIntradayExtract(runTimestamp);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error in extract run in {nameof(ExtractsOrchestrationService.RunExtract)}", ex);
            }

        }

        private async Task RunPowerGenerationIntradayExtract(DateTime runTimestamp)
        {
            await _logger.LogInfo($"Starting: Power intraday position extract at {runTimestamp:G}");

            // Get a timestamp aligned to the minute 
            // If there is another power extrat running for the same timestamp then cancel this run - Only 1 power intraday run per minute
            var extractName = Constants.ExtractType.IntradayPowerPosition;
            var normalizedTimestampForExtract = new DateTime(runTimestamp.Year, runTimestamp.Month, runTimestamp.Day, runTimestamp.Hour, runTimestamp.Minute, 0);

            try
            {
                if (CheckForDuplicateExtractRuns(extractName, normalizedTimestampForExtract))
                {
                    await _logger.LogInfo($"Canceled: Power intraday position extract for timestamp {normalizedTimestampForExtract:G}. Another run is already in progress for same tiemstamp");
                    return;
                }

                // Add current extract run to active runs log
                AddToCurrentlyRunningExtractsLog(Constants.ExtractType.IntradayPowerPosition, normalizedTimestampForExtract);

                // Get power position aggregates
                var data = await _positionDataService.GetIntradayPowerPositions(runTimestamp.Date);

                // Write position data to file
                var formattedData = data.Select(x => new KeyValuePair<string,string>(x.Key.ToString("hh:mm"), x.Value.ToString("F")));
                await _extractFileService.CreatePowerExtractFile(runTimestamp.Date, formattedData);

                await _logger.LogInfo($"Completed: Power intraday position extract at {DateTime.Now:G}");
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error in extract run in {nameof(ExtractsOrchestrationService.RunPowerGenerationIntradayExtract)}", ex);
            }
            finally
            {
                RemoveFromCurrentlyRunningExtractsLog(extractName, normalizedTimestampForExtract);
            }
        }

        private bool CheckForDuplicateExtractRuns(string extractName, DateTime timestamp)
        {
            if (!_activelyRunningExtracts.ContainsKey(extractName))
                return false;

            return _activelyRunningExtracts[extractName].Any(x => x == timestamp);
        }

        private bool AddToCurrentlyRunningExtractsLog(string extractName, DateTime timestamp)
        {
            if (!_activelyRunningExtracts.ContainsKey(extractName))
                return _activelyRunningExtracts.TryAdd(extractName, new List<DateTime>() { timestamp });

            if (!_activelyRunningExtracts[extractName].Any(x => x == timestamp))
                _activelyRunningExtracts[extractName].Add(timestamp);

            return true;
        }

        private void RemoveFromCurrentlyRunningExtractsLog(string extractName, DateTime timestamp)
        {
            if (!_activelyRunningExtracts.ContainsKey(extractName))
                return;

            _activelyRunningExtracts[extractName].Remove(timestamp);
        }
    }
}

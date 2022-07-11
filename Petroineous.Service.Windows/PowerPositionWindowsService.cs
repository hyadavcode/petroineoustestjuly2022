using Petroineous.Service.Core;
using Petroineous.Service.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Petroineous.Service.Windows
{
    public partial class PowerPositionWindowsService : ServiceBase
    {
        private readonly IExtractsOrchestrationService _extractsOrchestrator;
        private readonly ILogger _logger;

        public PowerPositionWindowsService()
        {
            InitializeComponent();
        }

        public PowerPositionWindowsService(IExtractsOrchestrationService extractsOrchestrator, ILogger logger)
            :this()
        {
            this._extractsOrchestrator = extractsOrchestrator;
            this._logger = logger;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _logger.LogInfo($"{nameof(OnStart)} invoked");

                if(_extractsOrchestrator.Status == ServiceStatus.Error)
                {
                    _logger.LogError("Failed to start Orchestration service. It is in Error state");
                    return;
                }

                var task = _extractsOrchestrator.Start()
                                .ContinueWith(x =>
                                    {
                                        if(x.IsCompleted)
                                            _logger.LogInfo("Successfully started extracts orchestrator");
                                    });

                task.Wait();
                _logger.LogInfo($"{nameof(OnStart)} complted");
            }
            catch(Exception ex)
            {
                _logger?.LogError($"Error in {nameof(OnStart)}", ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                _logger.LogInfo($"{nameof(OnStop)} invoked");

                var task = _extractsOrchestrator.Stop()
                            .ContinueWith(x =>
                            {
                                if (x.IsCompleted)
                                    _logger.LogInfo("Successfully started extracts orchestrator");
                            });

                task.Wait();
                _logger.LogInfo($"{nameof(OnStop)} completed");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in {nameof(OnStop)}", ex);
            }
        }

        public void RunInteractiveMode()
        {
            this.OnStart(new string[0]);

            while (_extractsOrchestrator.Status == ServiceStatus.Started)
                Thread.Sleep(2000);
        }
    }
}

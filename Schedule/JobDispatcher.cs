/*
  This file is part of  https://github.com/aiten/Framework.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

namespace Framework.Schedule
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Framework.Schedule.Abstraction;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class JobDispatcher
    {
        private readonly IServiceProvider        _serviceProvider;
        private readonly ILogger                 _logger;
        private readonly Type                    _job;
        private readonly string                  _jobName;
        private readonly Type                    _paramContainer;
        private readonly object                  _param;
        private readonly CancellationTokenSource _ctSource;

        public DateTime? NextExecutionRequest { get; private set; }

        public JobDispatcher(Type job, string jobName, Type paramContainer, object param, CancellationTokenSource ctSource, IServiceProvider serviceProvider, ILogger logger)
        {
            _job             = job;
            _jobName         = jobName;
            _paramContainer  = paramContainer;
            _param           = param;
            _ctSource        = ctSource;
            _serviceProvider = serviceProvider;
            _logger          = logger;
        }

        public async Task Run()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    if (_paramContainer != null)
                    {
                        var jobStateObj = (IJobParamContainer)scope.ServiceProvider.GetService(_paramContainer);
                        jobStateObj.Param = _param;
                    }

                    var job = (IJob)scope.ServiceProvider.GetService(_job);

                    job.JobName = _jobName;
                    job.Param   = _param;
                    job.CToken  = _ctSource.Token;

                    await Run(job);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unhandled exception by job: {_jobName}.");
            }
        }

        protected async Task Run(IJob job)
        {
            await job.SetContext();

            if (job is ISupervisedJob supervisedJob)
            {
                await RunSupervised(supervisedJob);
            }
            else
            {
                await job.Execute();
            }

            if (job is ISelfScheduledJob timedJob)
            {
                NextExecutionRequest = await timedJob.GetNextExecutionTime();
            }
        }

        protected async Task RunSupervised(ISupervisedJob supervisedJob)
        {
            var wasExecutedAlready = await supervisedJob.IsAlreadyExecuted();

            if (!wasExecutedAlready)
            {
                await supervisedJob.Execute();

                await supervisedJob.SetAsExecuted();
            }
        }
    }
}
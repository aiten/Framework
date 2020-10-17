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

    using Framework.Schedule.Abstraction;
    using Framework.Tools.Abstraction;

    using Microsoft.Extensions.Logging;

    public sealed class SelfScheduledSchedule : ISchedule
    {
        public SelfScheduledSchedule()
        {
        }

        public IJobExecutor Schedule(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job)
        {
            return new TimedJobExecutor(serviceProvider, loggerFactory, currentDateTime, job);
        }

        private sealed class TimedJobExecutor : JobExecutor
        {
            private readonly ILogger          _logger;
            private readonly ICurrentDateTime _dateTimeService;

            public TimedJobExecutor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job) :
                base(serviceProvider, loggerFactory, currentDateTime, job)
            {
                _logger          = loggerFactory.CreateLogger(GetType());
                _dateTimeService = currentDateTime;
            }

            public override void Start()
            {
                Timer = new Timer(async (state) => await ((TimedJobExecutor)state).Execute(), this, (int)GetRandomDelay(), -1);
            }

            protected override void Executed()
            {
                base.Executed();

                var nextExecution = NextExecutionRequest;

                if (nextExecution == null ||
                    nextExecution < _dateTimeService.Now)
                {
                    _logger.LogError($"NextExcecutionRequest {NextExecutionRequest?.ToString()} < {_dateTimeService.Now}. Restart immediately.");
                    nextExecution = _dateTimeService.Now + TimeSpan.FromMilliseconds(GetRandomDelay());
                }

                try
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    var nextExecutionDateTimeOffset = GetOffsetToNextTick(nextExecution.Value, DateTimeOffset => _dateTimeService.Now + TimeSpan.FromMilliseconds(GetRandomDelay()));

                    Timer.Change((int)nextExecutionDateTimeOffset.TotalMilliseconds, -1);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Timer could not schedule the next runtime successfully.");
                }
            }
        }
    }
}
/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Threading;

using Framework.Schedule.Abstraction;
using Framework.Tools.Abstraction;

using Microsoft.Extensions.Logging;

namespace Framework.Schedule
{
    public sealed class DailySchedule : ISchedule
    {
        private readonly TimeSpan _timeOfDay;

        public DailySchedule(TimeSpan timeOfDay)
        {
            _timeOfDay = timeOfDay;
        }

        public IJobExecutor Schedule(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job, object state)
        {
            return new TimedJobExecutor(_timeOfDay, serviceProvider, loggerFactory, currentDateTime, job, state);
        }

        private sealed class TimedJobExecutor : JobExecutor
        {
            private readonly ILogger _logger;

            private readonly ICurrentDateTime _dateTimeService;
            private readonly TimeSpan         _timeOfDay;
            private readonly Timer            _timer;

            private volatile bool _disposed;

            private bool _wasExecutedAlready;

            public TimedJobExecutor(TimeSpan timeOfDay, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job, object state)
                : base(serviceProvider, loggerFactory, currentDateTime, job, state)
            {
                _logger          = loggerFactory.CreateLogger(GetType());
                _dateTimeService = currentDateTime;
                _timeOfDay       = timeOfDay;
                _timer           = new Timer(state => ((TimedJobExecutor)state).Execute(), this, (int)GetOffsetToNextTick().TotalMilliseconds, -1);
            }

            public override void Start()
            {
                Timer = new Timer(state => ((TimedJobExecutor)state).Execute(), this, (int)GetOffsetToNextTick().TotalMilliseconds, -1);
            }

            protected override void Executing()
            {
                base.Executing();
            }

            protected override void Executed()
            {
                base.Executed();

                _wasExecutedAlready = true;

                try
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _timer.Change((int)GetOffsetToNextTick().TotalMilliseconds, -1);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Timer could not schedule the next runtime successfully.");
                }
            }


            private TimeSpan GetOffsetToNextTick()
            {
                var currentTime                 = _dateTimeService.Now;
                var nextExecutionDateTime       = currentTime.Date.Add(_timeOfDay);
                var nextExecutionTimeUtcOffset  = TimeZoneInfo.Local.GetUtcOffset(nextExecutionDateTime);
                var nextExecutionDateTimeOffset = new DateTimeOffset(nextExecutionDateTime, nextExecutionTimeUtcOffset);
                if (nextExecutionDateTimeOffset < currentTime)
                {
                    // Already in the past. Say if we want to run at 02:00 and it is already 14:00.
                    if (!_wasExecutedAlready)
                    {
                        // If the job was not executed today, e.g. when the service was down at the scheduled time
                        var random = new Random((int)DateTime.Now.Ticks);
                        return TimeSpan.FromMilliseconds(random.Next(5000, 15000));
                    }

                    nextExecutionDateTimeOffset = nextExecutionDateTimeOffset.AddDays(1);
                }

                var executionOffset = nextExecutionDateTimeOffset - currentTime;
                return executionOffset;
            }
        }
    }
}
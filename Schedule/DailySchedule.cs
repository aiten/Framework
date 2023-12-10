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

namespace Framework.Schedule;

using System;
using System.Threading;

using Framework.Schedule.Abstraction;
using Framework.Tools.Abstraction;

using Microsoft.Extensions.Logging;

public sealed class DailySchedule : ISchedule
{
    private readonly TimeSpan _timeOfDay;

    public DailySchedule(TimeSpan timeOfDay)
    {
        _timeOfDay = timeOfDay;
    }

    public IJobExecutor Schedule(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job)
    {
        return new DailyJobExecutor(_timeOfDay, serviceProvider, loggerFactory, currentDateTime, job);
    }

    private sealed class DailyJobExecutor : JobExecutor
    {
        private readonly ILogger _logger;

        private readonly ICurrentDateTime _dateTimeService;
        private readonly TimeSpan         _timeOfDay;

        private bool _wasExecutedAlready;

        public DailyJobExecutor(TimeSpan timeOfDay, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job)
            : base(serviceProvider, loggerFactory, currentDateTime, job)
        {
            _logger          = loggerFactory.CreateLogger(GetType());
            _dateTimeService = currentDateTime;
            _timeOfDay       = timeOfDay;
        }

        public override void Start()
        {
            Timer = new Timer(async (state) => await CastAndRunJob(state), this, (int)GetOffsetToNextTick().TotalMilliseconds, -1);
        }

        protected override void Executed()
        {
            base.Executed();

            _wasExecutedAlready = true;

            try
            {
                if (IsDisposed)
                {
                    return;
                }

                Timer?.Change((int)GetOffsetToNextTick().TotalMilliseconds, -1);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Timer could not schedule the next runtime successfully.");
            }
        }

        private DateTime GetNextExecutionDateTime()
        {
            var currentTime           = _dateTimeService.Now;
            var nextExecutionDateTime = currentTime.Date.Add(_timeOfDay);
            return nextExecutionDateTime;
        }

        private TimeSpan GetOffsetToNextTick()
        {
            return GetOffsetToNextTick(GetNextExecutionDateTime(), OnTimeInPast);
        }

        private DateTimeOffset OnTimeInPast(DateTimeOffset nextExecutionDateTimeOffset)
        {
            if (!_wasExecutedAlready)
            {
                // If the job was not executed today, e.g. when the service was down at the scheduled time
                return _dateTimeService.Now + TimeSpan.FromMilliseconds(GetRandomDelay());
            }

            return nextExecutionDateTimeOffset.AddDays(1);
        }
    }
}
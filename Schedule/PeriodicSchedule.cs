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
    public sealed class PeriodicSchedule : ISchedule
    {
        private readonly TimeSpan _period;

        public PeriodicSchedule(TimeSpan period)
        {
            _period = period;
        }

        public IJobExecutor Schedule(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job, object state)
        {
            return new PeriodicJobExecutor(_period, serviceProvider, loggerFactory, currentDateTime, job, state);
        }

        private sealed class PeriodicJobExecutor : JobExecutor
        {
            private readonly TimeSpan _period;

            public PeriodicJobExecutor(TimeSpan period, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job, object state) :
                base(serviceProvider, loggerFactory, currentDateTime, job, state)
            {
                _period = period;
            }

            public override void Start()
            {
                Timer = new Timer(state => ((PeriodicJobExecutor)state).Execute(), this, TimeSpan.Zero, _period);
            }
        }
    }
}
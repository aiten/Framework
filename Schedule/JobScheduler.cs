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
using System.Collections.Generic;

using Framework.Schedule.Abstraction;
using Framework.Tools.Abstraction;

using Microsoft.Extensions.Logging;

public sealed class JobScheduler : IJobScheduler
{
    private readonly ILoggerFactory      _loggerFactory;
    private readonly IList<IJobExecutor> _jobExecutorList = new List<IJobExecutor>();
    private readonly ICurrentDateTime    _currentDateTime;
    private readonly IServiceProvider    _serviceProvider;

    public JobScheduler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory   = loggerFactory;
        _currentDateTime = currentDateTime;
    }

    public IJobExecutor ScheduleJob(ISchedule schedule, Type job)
    {
        var jobExecutor = schedule.Schedule(_serviceProvider, _loggerFactory, _currentDateTime, job);
        _jobExecutorList.Add(jobExecutor);
        return jobExecutor;
    }

    public void Start()
    {
        foreach (var jobExecutor in _jobExecutorList)
        {
            jobExecutor.Start();
        }
    }

    public void Dispose()
    {
        foreach (var executor in _jobExecutorList)
        {
            executor.CtSource.Cancel();
            executor.Dispose();
        }
    }
}
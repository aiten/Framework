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
using System.Threading;
using System.Threading.Tasks;

using Framework.Schedule.Abstraction;
using Framework.Tools.Abstraction;

using Microsoft.Extensions.Logging;

internal abstract class JobExecutor : IJobExecutor
{
    private readonly IServiceProvider    _serviceProvider;
    private readonly ICurrentDateTime    _currentDateTime;
    private readonly ILogger             _logger;
    private readonly ILoggerFactory      _loggerFactory;
    private readonly Type                _job;
    private readonly IList<IJobExecutor> _thenExecutors = new List<IJobExecutor>();

    private volatile bool _disposed;

    protected bool IsDisposed => _disposed;

    public CancellationTokenSource CtSource { get; set; } = new CancellationTokenSource();

    public string JobName        { get; set; }
    public Type   ParamContainer { get; set; } = typeof(JobParamContainer);
    public object Param          { get; set; }

    public Timer     Timer                { get; set; }
    public DateTime? NextExecutionRequest { get; private set; }

    protected JobExecutor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job)
    {
        _serviceProvider = serviceProvider;
        _currentDateTime = currentDateTime;
        _logger          = loggerFactory.CreateLogger(job);
        _loggerFactory   = loggerFactory;
        _job             = job;
        JobName          = job.Name;
    }

    /// <summary>
    /// Start a timer with "ExecuteAsync" as callback
    /// </summary>
    public abstract void Start();

    public IJobExecutor Then(Type job)
    {
        var thenJob = new ThenJobExecutor(_serviceProvider, _loggerFactory, _currentDateTime, job);
        _thenExecutors.Add(thenJob);
        return thenJob;
    }

    public void Dispose()
    {
        foreach (var thenJobs in _thenExecutors)
        {
            ((JobExecutor)thenJobs).Dispose();
        }

        _disposed = true;
        Timer?.Dispose();
    }

    protected virtual void Executing()
    {
    }

    protected virtual void Executed()
    {
    }

    public async Task Execute()
    {
        var jobController = new JobDispatcher(_job, JobName, ParamContainer, Param, CtSource, _serviceProvider, _logger);

        Executing();

        await jobController.Run();

        NextExecutionRequest = jobController.NextExecutionRequest;

        Executed();

        await ExecuteThen();
    }

    private async Task ExecuteThen()
    {
        foreach (var thenJobs in _thenExecutors)
        {
            await ((JobExecutor)thenJobs).Execute();
        }
    }

    private sealed class ThenJobExecutor : JobExecutor
    {
        public ThenJobExecutor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job) :
            base(serviceProvider, loggerFactory, currentDateTime, job)
        {
        }

        public override void Start()
        {
        }
    }

    protected TimeSpan GetOffsetToNextTick(DateTime nextExecutionDateTime, Func<DateTimeOffset, DateTimeOffset> inThePast)
    {
        var currentTime                 = _currentDateTime.Now;
        var nextExecutionTimeUtcOffset  = TimeZoneInfo.Local.GetUtcOffset(nextExecutionDateTime);
        var nextExecutionDateTimeOffset = new DateTimeOffset(nextExecutionDateTime, nextExecutionTimeUtcOffset);
        if (nextExecutionDateTimeOffset < currentTime)
        {
            // Already in the past. Als delegate for new date
            nextExecutionDateTimeOffset = inThePast(nextExecutionDateTimeOffset);
        }

        var executionOffset = nextExecutionDateTimeOffset - currentTime;
        return executionOffset;
    }

    protected int GetRandomDelay(int from = 5000, int to = 15000)
    {
        var random = new Random((int)DateTime.Now.Ticks);
        return random.Next(from, to);
    }

    protected static async Task CastAndRunJob(object state)
    {
        var jobExecutor = (JobExecutor)state;
        _ = jobExecutor ?? throw new ArgumentNullException(nameof(jobExecutor));
        await jobExecutor.Execute();
    }
}
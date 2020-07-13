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

namespace Framework.Schedule
{
    using System;

    using Framework.Schedule.Abstraction;

    public static class JobSchedulerExtension
    {
        public static IJobExecutor Periodic(this IJobScheduler scheduler, TimeSpan period, Type job, object state)
        {
            var schedule = new PeriodicSchedule(period);
            return scheduler.ScheduleJob(schedule, job, state);
        }

        public static IJobExecutor Periodic<T>(this IJobScheduler scheduler, TimeSpan period, object state) where T : IJob
        {
            return scheduler.Periodic(period, typeof(T), state);
        }

        public static IJobExecutor Daily(this IJobScheduler scheduler, TimeSpan timeOfDay, Type job, object state)
        {
            var schedule = new DailySchedule(timeOfDay);
            return scheduler.ScheduleJob(schedule, job, state);
        }

        public static IJobExecutor Daily<T>(this IJobScheduler scheduler, TimeSpan timeOfDay, object state) where T : IJob
        {
            return scheduler.Daily(timeOfDay, typeof(T), state);
        }

        public static IJobExecutor Then<T>(this IJobExecutor jobExecutor, object state) where T : IJob
        {
            return jobExecutor.Then(typeof(T), state);
        }
    }
}
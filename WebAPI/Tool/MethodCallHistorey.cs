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
using System.Collections.Generic;
using System.Linq;

namespace Framework.WebAPI.Tool
{
    public class MethodCallHistory
    {
        private class MethodCall
        {
            public DateTime CallTime { get; set; }
            public string   UserName { get; set; }
            public string   Uri      { get; set; }

            public override string ToString()
            {
                return $"{CallTime.ToString("s")}:{UserName}: {Uri}";
            }
        }

        private const int KEEPCALLHISTORYMINUTES = 10;

        private object _lock = new object();

        private List<MethodCall> _calls = new List<MethodCall>();

        public int MaxMinuteAge { get; set; } = KEEPCALLHISTORYMINUTES;

        public void Add(string uri, string userName)
        {
            CleanUp();

            lock (_lock)
            {
                _calls.Add(new MethodCall() { CallTime = DateTime.Now, Uri = uri, UserName = userName });
            }
        }

        public IEnumerable<string> GetCallList(string userName)
        {
            CleanUp();

            lock (_lock)
            {
                var calls = (IEnumerable<MethodCall>)_calls;

                if (string.IsNullOrEmpty(userName) == false)
                {
                    calls = calls.Where(c => c.UserName.Contains(userName, StringComparison.OrdinalIgnoreCase));
                }

                return calls.Select(c => c.ToString());
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _calls.Clear();
            }
        }

        private void CleanUp()
        {
            var cleanupUntil = DateTime.Now - TimeSpan.FromMinutes(MaxMinuteAge);

            lock (_lock)
            {
                while (_calls.Any() && _calls.First().CallTime < cleanupUntil)
                {
                    _calls.RemoveAt(0);
                }
            }
        }
    }
}
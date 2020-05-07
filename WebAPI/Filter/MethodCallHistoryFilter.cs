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

using Framework.WebAPI.Tool;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Framework.WebAPI.Filter
{
    public sealed class MethodCallHistoryFilter : IActionFilter
    {
        private MethodCallHistory _methodCallHistory;

        public MethodCallHistoryFilter(MethodCallHistory methodCallHistory)
        {
            _methodCallHistory = methodCallHistory;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            string userName = null;

            if (context.Controller is Microsoft.AspNetCore.Mvc.Controller controller)
            {
                userName = controller.User?.Identity?.Name;
            }

            _methodCallHistory.Add(GetCurrentUri(context.HttpContext.Request), userName);
        }

        public string GetCurrentUri(HttpRequest request)
        {
            return $"{request.Method} {request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        }
    }
}
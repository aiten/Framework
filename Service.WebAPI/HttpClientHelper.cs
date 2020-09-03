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

namespace Framework.Service.WebAPI
{
    using System;
    using System.Collections.Generic;

    using System.Net.Http;
    using System.Net.Http.Headers;

    public static class HttpClientHelper
    {
        public static void PrepareHttpClient(HttpClient httpClient, string baseUri = null, int timeOutSeconds = -1, Dictionary<string, string> defaultHeaders = null)
        {
            if (!string.IsNullOrEmpty(baseUri))
            {
                httpClient.BaseAddress = new System.Uri(baseUri);
            }

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (timeOutSeconds != -1)
            {
                httpClient.Timeout = TimeSpan.FromSeconds(timeOutSeconds);
            }

            if (defaultHeaders != null)
            {
                foreach (var defaultHeader in defaultHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(defaultHeader.Key, defaultHeader.Value);
                }
            }
        }

        public static HttpClientHandler CreateHttpClientHandlerIgnoreSSLCertificatesError()
        {
            return new HttpClientHandler
            {
                ClientCertificateOptions                  = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };
        }
    }
}
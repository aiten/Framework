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
using System.Net.Http;
using System.Net.Http.Headers;

using Framework.Pattern;

namespace Framework.Service.WebAPI
{
    public class HttpClientFactory : Singleton<HttpClientFactory>
    {
        private Dictionary<string, HttpClient> _httpClients { get; set; } = new Dictionary<string, HttpClient>();
        private List<Tuple<string, Func<string, HttpClient>>> _createHttpClient = new List<Tuple<string, Func<string, HttpClient>>>();


        public HttpClient GetHttpClient(string baseUri)
        {
            HttpClient client;

            if (!_httpClients.TryGetValue(baseUri, out client))
            {
                var createFunction = _createHttpClient.FirstOrDefault(f => f.Item1 == baseUri);

                if (createFunction == null)
                {
                    client = new HttpClient() { BaseAddress = new System.Uri(baseUri) };
                    DefaultConfigureHttpClient(client);
                }
                else
                {
                    client = createFunction.Item2(baseUri);
                }

                RegisterHttpClient(baseUri, client);
            }

            return client;
        }

        public void RegisterHttpClient(string baseUri, HttpClient client)
        {
            _httpClients[baseUri] = client;
        }

        public void RegisterCreateHttpClient(string baseUri, Func<string, HttpClient> createHttpClient)
        {
            _createHttpClient.Add(new Tuple<string, Func<string, HttpClient>>(baseUri, createHttpClient));
        }

        public static void DefaultConfigureHttpClient(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
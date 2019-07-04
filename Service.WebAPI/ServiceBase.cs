﻿/*
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

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Framework.Pattern;
using Framework.Service.WebAPI.Uri;

namespace Framework.Service.WebAPI
{
    public class ServiceBase : DisposeWrapper
    {
        public string BaseApi { get; set; }

        private HttpClient _httpClient;

        protected ServiceBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected virtual HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        public async Task<IList<T>> ReadList<T>(string uri)
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadAsAsync<IList<T>>();
                return items;
            }

            return null;
        }

        public async Task<IList<T>> ReadList<T>(UriPathBuilder pathBuilder)
        {
            return await ReadList<T>(pathBuilder.Build());
        }

        public async Task<T> Read<T>(string uri)
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadAsAsync<T>();
                return item;
            }

            return default(T);
        }

        public async Task<string> ReadString(UriPathBuilder pathBuilder)
        {
            return await ReadString(pathBuilder.Build());
        }

        public async Task<string> ReadString(string uri)
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadAsStringAsync();
                return item;
            }

            return null;
        }

        public async Task<T> Read<T>(UriPathBuilder pathBuilder)
        {
            return await Read<T>(pathBuilder.Build());
        }

        public UriPathBuilder CreatePathBuilder()
        {
            return new UriPathBuilder().AddPath(BaseApi);
        }
    }
}
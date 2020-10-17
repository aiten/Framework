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

namespace Framework.WebAPI.Controller
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Framework.WebAPI.Tool;

    using Logic.Abstraction;

    public static class ControllerExtensions
    {
        #region Url

        public static string GetCurrentUri(this Controller controller)
        {
            if (controller.Request == null)
            {
                // unit test => no Request available
                return "dummy";
            }

            return controller.Request.GetCurrentUri();
        }

        public static string GetCurrentUri(this Controller controller, string removeTrailing)
        {
            if (controller.Request == null)
            {
                // unit test => no Request available
                return "dummy";
            }

            var totalUri = controller.Request.GetCurrentUri();

            var filterIdx = totalUri.LastIndexOf('?');
            if (filterIdx > 0)
            {
                totalUri = totalUri.Substring(0, filterIdx - 1);
            }

            return totalUri.Substring(0, totalUri.Length - removeTrailing.Length);
        }

        #endregion

        #region Result

        public static async Task<ActionResult<T>> NotFoundOrOk<T>(this Controller controller, T obj)
        {
            if (obj == null)
            {
                await Task.CompletedTask; // avoid CS1998
                return controller.NotFound();
            }

            return controller.Ok(obj);
        }

        public static async Task<ActionResult<IEnumerable<T>>> NotFoundOrOk<T>(this Controller controller, IEnumerable<T> list)
        {
            if (list == null)
            {
                await Task.CompletedTask; // avoid CS1998
                return controller.NotFound();
            }

            return controller.Ok(list);
        }

        #endregion

        #region Get/GetAll

        public static async Task<ActionResult<T>> Get<T, TKey>(this Controller controller, IGetManager<T, TKey> manager, TKey id) where T : class where TKey : IComparable
        {
            var dto = await manager.Get(id);
            return await controller.NotFoundOrOk(dto);
        }

        public static async Task<ActionResult<IEnumerable<T>>> GetAll<T, TKey>(this Controller controller, IGetManager<T, TKey> manager) where T : class where TKey : IComparable
        {
            var dtos = await manager.GetAll();
            return await controller.NotFoundOrOk(dtos);
        }

        #endregion

        #region Add

        public static async Task<ActionResult<T>> Add<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, T value) where T : class where TKey : IComparable
        {
            var newId  = await manager.Add(value);
            var newUri = controller.GetCurrentUri() + "/" + newId;
            return controller.Created(newUri, await manager.Get(newId));
        }

        public static async Task<IEnumerable<UriAndValue<T>>> AddIntern<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, IEnumerable<T> values)
            where T : class where TKey : IComparable
        {
            var newIds     = await manager.Add(values);
            var newObjects = await manager.Get(newIds);

            var uri     = controller.GetCurrentUri("/bulk");
            var newUris = newIds.Select(id => uri + "/" + id);
            var results = newIds.Select((id, idx) => new UriAndValue<T>() { Uri = uri + "/" + id, Value = newObjects.ElementAt(idx) });
            return results;
        }

        public static async Task<ActionResult<IEnumerable<UriAndValue<T>>>> Add<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, IEnumerable<T> values)
            where T : class where TKey : IComparable
        {
            return controller.Ok(await AddIntern(controller, manager, values));
        }

        public static async Task<ActionResult<UrisAndValues<T>>> Add2<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, IEnumerable<T> values)
            where T : class where TKey : IComparable
        {
            return controller.Ok((await AddIntern(controller, manager, values)).ToUrisAndValues());
        }

        public static async Task<ActionResult<T>> AddNoGet<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, T value, Action<T, TKey> setIdFunc)
            where T : class where TKey : IComparable
        {
            var newId  = await manager.Add(value);
            var newUri = controller.GetCurrentUri() + "/" + newId;
            setIdFunc(value, newId);
            return controller.Created(newUri, value);
        }

        public static async Task<IEnumerable<UriAndValue<T>>> AddNoGetIntern<T, TKey>(
            this Controller       controller,
            ICrudManager<T, TKey> manager,
            IEnumerable<T>        values,
            Action<T, TKey>       setIdFunc)
            where T : class where TKey : IComparable
        {
            var newIds = await manager.Add(values);

            Func<T, TKey, T> mySetFunc = (v, k) =>
            {
                setIdFunc(v, k);
                return v;
            };

            var uri     = controller.GetCurrentUri("/bulk");
            var newUris = newIds.Select(id => uri + "/" + id);
            var results = newIds.Select((id, idx) => new UriAndValue<T>() { Uri = uri + "/" + id, Value = mySetFunc(values.ElementAt(idx), id) });
            return results;
        }

        public static async Task<ActionResult<IEnumerable<UriAndValue<T>>>> AddNoGet<T, TKey>(
            this Controller       controller,
            ICrudManager<T, TKey> manager,
            IEnumerable<T>        values,
            Action<T, TKey>       setIdFunc)
            where T : class where TKey : IComparable
        {
            return controller.Ok(await AddNoGetIntern(controller, manager, values, setIdFunc));
        }

        public static async Task<ActionResult<UrisAndValues<T>>> Add2NoGet<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, IEnumerable<T> values, Action<T, TKey> setIdFunc)
            where T : class where TKey : IComparable
        {
            return controller.Ok((await AddNoGetIntern(controller, manager, values, setIdFunc)).ToUrisAndValues());
        }

        #endregion

        #region Update

        public static async Task<ActionResult> Update<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, TKey idFromUri, TKey idFromValue, T value)
            where T : class where TKey : IComparable
        {
            if (idFromUri.CompareTo(idFromValue) != 0)
            {
                return controller.BadRequest("Mismatch between id and dto.Id");
            }

            await manager.Update(value);
            return controller.NoContent();
        }

        public static async Task<ActionResult> Update<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, IEnumerable<T> values) where T : class where TKey : IComparable
        {
            await manager.Update(values);
            return controller.NoContent();
        }

        #endregion

        #region Delete

        public static async Task<ActionResult> Delete<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, TKey id) where T : class where TKey : IComparable
        {
            await manager.Delete(id);
            return controller.NoContent();
        }

        public static async Task<ActionResult> Delete<T, TKey>(this Controller controller, ICrudManager<T, TKey> manager, IEnumerable<TKey> ids) where T : class where TKey : IComparable
        {
            await manager.Delete(ids);
            return controller.NoContent();
        }

        #endregion

        #region Upload/Download

        public static string GetContentType(this Controller controller, string path)
        {
            var types = controller.GetMimeTypes();
            var ext   = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        public static Dictionary<string, string> GetMimeTypes(this Controller controller)
        {
            return new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.ms-word" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" }
            };
        }

        #endregion
    }
}
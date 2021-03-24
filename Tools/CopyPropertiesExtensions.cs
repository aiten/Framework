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

namespace Framework.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    public static class CopyPropertiesExtensions
    {
        public static void CopyProperties<T>(this T dest, T src, params string[] excludeList) where T : class
        {
            var propertyInfos = GetCanWriteProperties(dest.GetType(), excludeList);

            foreach (var propertyInfo in propertyInfos)
            {
                propertyInfo.SetValue(dest, propertyInfo.GetValue(src));
            }
        }

        public static IList<PropertyInfo> CopyChangedProperties<T>(this T dest, T src, params string[] excludeList)
        {
            var propertyInfos = GetCanWriteProperties(dest.GetType(), excludeList);

            var assigned = new List<PropertyInfo>();

            foreach (var propertyInfo in propertyInfos)
            {
                var currentValue = propertyInfo.GetValue(dest);
                var newValue     = propertyInfo.GetValue(src);
                var type         = propertyInfo.PropertyType;

                bool isNullableType = type.IsGenericType && type.Name.StartsWith(@"Nullable");

                if (isNullableType || !type.IsValueType)
                {
                    switch ((currentValue == null ? 0 : 1) + (newValue == null ? 0 : 1))
                    {
                        case 0: break; // both null => no change
                        case 1:        // one is null => change
                            propertyInfo.SetValue(dest, newValue);
                            assigned.Add(propertyInfo);
                            break;
                        case 2:

                            if (AssignValues(isNullableType ? type.GenericTypeArguments[0] : type, propertyInfo, dest, currentValue, newValue))
                            {
                                assigned.Add(propertyInfo);
                            }

                            break;
                    }
                }
                else
                {
                    if (AssignValues(type, propertyInfo, dest, currentValue, newValue))
                    {
                        assigned.Add(propertyInfo);
                    }
                }
            }

            return assigned;
        }

        private static IList<PropertyInfo> GetCanWriteProperties(Type type, params string[] excludeList)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.CanRead &&
                    p.CanWrite &&
                    (typeof(IComparable).IsAssignableFrom(p.PropertyType) || p.PropertyType.IsPrimitive || p.PropertyType.IsValueType) &&
                    !excludeList.Contains(p.Name))
                .ToList();
        }

        private static bool AssignValues(Type type, PropertyInfo propertyInfo, object dest, object valueDesc, object valueSrc)
        {
            if (!typeof(IComparable).IsAssignableFrom(type))
            {
                throw new ArgumentException($@"{propertyInfo.Name}: cannot compare property, please exclude this property from assignment");
            }

            if ((valueDesc as IComparable).CompareTo(valueSrc) == 0)
            {
                // no change
                return false;
            }

            propertyInfo.SetValue(dest, valueSrc);

            return true;
        }
    }
}
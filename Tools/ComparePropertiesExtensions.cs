﻿/*
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

namespace Framework.Tools;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ComparePropertiesExtensions
{
    #region CompareProperties

    public static bool ArePropertiesEqual<T>(this T objectA, T objectB, params string[] ignoreList) where T : class
    {
        return AreObjectsPropertiesEqual(objectA, objectB, new HashSet<object?>(), ignoreList);
    }

    public static bool ArePropertiesEqual(object? objectA, object? objectB, params string[] ignoreList)
    {
        return AreObjectsPropertiesEqual(objectA, objectB, new HashSet<object?>(), ignoreList);
    }

    private static bool AreObjectsPropertiesEqual(object? objectA, object? objectB, HashSet<object?> compared, params string[] ignoreList)
    {
        // check for circles e.g. ClassA => ICollection<ClassB> => classA
        if (compared.Contains(objectA))
        {
            return true;
        }

        compared.Add(objectA);

        if (objectA != null && objectB != null)
        {
            var objectType = objectA.GetType();

            foreach (PropertyInfo propertyInfo in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && !ignoreList.Contains(p.Name)))
            {
                var valueA = propertyInfo.GetValue(objectA, null);
                var valueB = propertyInfo.GetValue(objectB, null);

                if (CanDirectlyCompare(propertyInfo.PropertyType))
                {
                    if (!AreValuesEqual(valueA, valueB))
                    {
                        return false;
                    }
                }
                else if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    // if it implements IEnumerable, then scan any items
                    if (valueA == null && valueB != null || valueA != null && valueB == null)
                    {
                        return false;
                    }

                    var collectionItems1      = ((IEnumerable)valueA!)!.Cast<object>().ToList();
                    var collectionItems2      = ((IEnumerable)valueB!)!.Cast<object>().ToList();
                    var collectionItemsCount1 = collectionItems1.Count;
                    var collectionItemsCount2 = collectionItems2.Count;

                    // check the counts to ensure they match
                    if (collectionItemsCount1 != collectionItemsCount2)
                    {
                        return false;
                    }

                    // and if they do, compare each item...
                    // this assumes both collections have the same order
                    for (int i = 0; i < collectionItemsCount1; i++)
                    {
                        var collectionItem1    = collectionItems1.ElementAt(i);
                        var collectionItem2    = collectionItems2.ElementAt(i);
                        var collectionItemType = collectionItem1.GetType();

                        if (CanDirectlyCompare(collectionItemType))
                        {
                            if (!AreValuesEqual(collectionItem1, collectionItem2))
                            {
                                return false;
                            }
                        }
                        else if (!AreObjectsPropertiesEqual(collectionItem1, collectionItem2, compared, ignoreList))
                        {
                            return false;
                        }
                    }
                }
                else if (propertyInfo.PropertyType.IsClass)
                {
                    if (!AreObjectsPropertiesEqual(propertyInfo.GetValue(objectA, null), propertyInfo.GetValue(objectB, null), compared, ignoreList))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Equals(objectA, objectB))
            {
                return false;
            }
        }

        return true;
    }

    ////////////////////////////////////////////////////////

    private static bool CanDirectlyCompare(Type type)
    {
        return type != null && (typeof(IComparable).IsAssignableFrom(type) || type.IsPrimitive || type.IsValueType);
    }

    ////////////////////////////////////////////////////////

    private static bool AreValuesEqual(object? valueA, object? valueB)
    {
        if (((valueA == null ? 0 : 1) + (valueB == null ? 0 : 1)) == 1)
        {
            return false; // one (and not more) of the values is null
        }

        if (valueA is IComparable selfValueComparer)
        {
            return selfValueComparer.CompareTo(valueB) == 0;
        }

        return Equals(valueA, valueB);
    }

    #endregion
}
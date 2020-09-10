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


namespace Framework.Tools.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class CsvImport<T> : CsvImportBase where T : new()
    {
        public IList<T> Read(string[] csvLines)
        {
            var lines = ReadStringMatrixFromCsv(csvLines, false);
            return MapTo(lines);
        }

        public IList<T> Read(string fileName)
        {
            var lines = ReadStringMatrixFromCsv(fileName, false);
            return MapTo(lines);
        }

        public async Task<IList<T>> ReadAsync(string fileName)
        {
            var lines = await ReadStringMatrixFromCsvAsync(fileName, false);
            return MapTo(lines);
        }

        public IList<T> MapTo(IList<IList<string>> lines)
        {
            // first line is columnLineHeader!!!!

            var  list  = new List<T>();
            var  props = GetPropertyMapping(lines[0]);
            bool first = true;

            if (props.Any(prop => prop == null))
            {
                throw new ArgumentException($"Column cannot be mapped: {string.Join(", ", lines[0].Where((p, idx) => props[idx] == null))}");
            }

            foreach (var line in lines)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    list.Add(Map(line, props));
                }
            }

            return list;
        }

        private PropertyInfo[] GetPropertyMapping(IList<string> columnNames)
        {
            Type t = typeof(T);
            return columnNames.Select((columnName) => t.GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)).ToArray();
        }

        private T Map(IList<string> line, PropertyInfo[] props)
        {
            var newT = new T();
            int idx  = 0;

            foreach (var column in line)
            {
                AssignProperty(newT, column, props[idx++]);
            }

            return newT;
        }

        private object? GetValue(string valueAsString, Type type)
        {
            if (type.IsGenericType && type.Name.StartsWith(@"Nullable"))
            {
                if (string.IsNullOrEmpty(valueAsString))
                {
                    return null;
                }

                type = type.GenericTypeArguments[0];
            }

            if (type == typeof(string))
            {
                return ExcelString(valueAsString);
            }
            else if (type == typeof(int))
            {
                return ExcelInt(valueAsString);
            }
            else if (type == typeof(long))
            {
                return ExcelLong(valueAsString);
            }
            else if (type == typeof(short))
            {
                return ExcelShort(valueAsString);
            }
            else if (type == typeof(uint))
            {
                return ExcelUInt(valueAsString);
            }
            else if (type == typeof(ulong))
            {
                return ExcelULong(valueAsString);
            }
            else if (type == typeof(ushort))
            {
                return ExcelUShort(valueAsString);
            }
            else if (type == typeof(decimal))
            {
                return ExcelDecimal(valueAsString);
            }
            else if (type == typeof(byte))
            {
                return ExcelByte(valueAsString);
            }
            else if (type == typeof(bool))
            {
                return ExcelBool(valueAsString);
            }
            else if (type == typeof(DateTime))
            {
                return ExcelDateOrDateTime(valueAsString);
            }
            else if (type == typeof(TimeSpan))
            {
                return ExcelTimeSpan(valueAsString);
            }
            else if (type == typeof(double))
            {
                return ExcelDouble(valueAsString);
            }
            else if (type.IsEnum)
            {
                return ExcelEnum(type, valueAsString);
            }
            else if (type == typeof(byte[]))
            {
                return ExcelImage(valueAsString);
            }

            throw new NotImplementedException();
        }

        private void AssignProperty(object obj, string valueAsString, PropertyInfo pi)
        {
            if (pi != null && pi.CanWrite)
            {
                pi.SetValue(obj, GetValue(valueAsString, pi.PropertyType));
            }
        }
    }
}
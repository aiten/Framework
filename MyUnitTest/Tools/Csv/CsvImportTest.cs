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

using FluentAssertions;
using FluentAssertions.Extensions;

using Framework.Tools.Csv;

using Xunit;

namespace Framework.Tools.Csv
{
    public class CsvImportTest
    {
        #region CsvImport

        public class CsvImportClass
        {
            public enum TestEnum
            {
                EnumValue1,
                EnumValue2
            }

            public string    ColString                  { get; set; }
            public int       ColInt                     { get; set; }
            public short     ColShort                   { get; set; }
            public decimal   ColDecimal                 { get; set; }
            public byte      ColByte                    { get; set; }
            public bool      ColBool                    { get; set; }
            public long      ColLong                    { get; set; }
            public double    ColDouble                  { get; set; }
            public TestEnum  ColEnum                    { get; set; }
            public DateTime  ColDate                    { get; set; }
            public DateTime  ColDateAndTime             { get; set; }
            public DateTime  ColDateAndTimeFraction     { get; set; }
            public TimeSpan  ColTimeSpan                { get; set; }
            public int?      ColIntNull                 { get; set; }
            public short?    ColShortNull               { get; set; }
            public decimal?  ColDecimalNull             { get; set; }
            public byte?     ColByteNull                { get; set; }
            public bool?     ColBoolNull                { get; set; }
            public long?     ColLongNull                { get; set; }
            public double?   ColDoubleNull              { get; set; }
            public TestEnum? ColEnumNull                { get; set; }
            public DateTime? ColDateNull                { get; set; }
            public DateTime? ColDateAndTimeNull         { get; set; }
            public DateTime? ColDateAndTimeFractionNull { get; set; }
            public TimeSpan? ColTimeSpanNull            { get; set; }
        }

        [Fact]
        public void CsvImportFileTest()
        {
            var lines = new[]
            {
                "ColString;ColInt;ColShort;ColDecimal;ColByte;ColBool;ColLong;ColEnum;ColDate;ColDateAndTime;ColDateAndTimeFraction;ColTimeSpan;ColDouble;ColIntNull;ColShortNull;ColDecimalNull;ColByteNull;ColBoolNull;ColLongNull;ColEnumNull;ColDateNull;ColDateAndTimeNull;ColDateAndTimeFractionNull;ColDoubleNull;ColTimeSpanNull",
                "Str;1;2;2.5;127;true;1234567890;EnumValue1;2018/12/31;2018/12/31 15:56:45;2018/12/31 15:56:45.123;15:56:45;34.12;1;2;2.5;127;false;9876543210;EnumValue1;2018/12/31;2018/12/31 15:56:45;2018/12/31 15:56:45.123;34.12;15:56:45",
                ";1;2;2.5;127;true;1234567890;EnumValue2;2018/12/31;2018/12/31 15:56:45;2018/12/31 15:56:45.123;15:56:45.123;34.12;;;;;;;;;;;"
            };

            var csvList = new CsvImport<CsvImportClass>().Read(lines);

            csvList.Should().HaveCount(lines.Length - 1);

            var csvObjectShouldBe = new CsvImportClass
            {
                ColString                  = "Str",
                ColInt                     = 1,
                ColShort                   = 2,
                ColDecimal                 = 2.5m,
                ColByte                    = 127,
                ColBool                    = true,
                ColLong                    = 1234567890,
                ColEnum                    = CsvImportClass.TestEnum.EnumValue1,
                ColDate                    = 31.December(2018),
                ColDateAndTime             = new DateTime(2018, 12, 31, 15, 56, 45),
                ColDateAndTimeFraction     = new DateTime(2018, 12, 31, 15, 56, 45, 123),
                ColTimeSpan                = new TimeSpan(15, 56, 45),
                ColDouble                  = 34.12,
                ColIntNull                 = 1,
                ColShortNull               = 2,
                ColDecimalNull             = 2.5m,
                ColByteNull                = 127,
                ColBoolNull                = false,
                ColLongNull                = 9876543210,
                ColEnumNull                = CsvImportClass.TestEnum.EnumValue1,
                ColDateNull                = 31.December(2018),
                ColDateAndTimeNull         = new DateTime(2018, 12, 31, 15, 56, 45),
                ColDateAndTimeFractionNull = new DateTime(2018, 12, 31, 15, 56, 45, 123),
                ColTimeSpanNull            = new TimeSpan(15, 56, 45),
                ColDoubleNull              = 34.12
            };
            var csvObjectShouldBeNull = new CsvImportClass
            {
                ColString              = string.Empty,
                ColInt                 = 1,
                ColShort               = 2,
                ColDecimal             = 2.5m,
                ColByte                = 127,
                ColBool                = true,
                ColLong                = 1234567890,
                ColEnum                = CsvImportClass.TestEnum.EnumValue2,
                ColDate                = 31.December(2018),
                ColDateAndTime         = new DateTime(2018, 12, 31, 15, 56, 45),
                ColDateAndTimeFraction = new DateTime(2018, 12, 31, 15, 56, 45, 123),
                ColTimeSpan            = new TimeSpan(0, 15, 56, 45, 123),
                ColDouble              = 34.12,
            };

            var csvObject     = csvList[0];
            var csvObjectNull = csvList[1];

            csvObject.Should().BeEquivalentTo(csvObjectShouldBe);
            csvObjectNull.Should().BeEquivalentTo(csvObjectShouldBeNull);
        }

        #endregion

        #region StringQuoating 

        public class CsvImportStringClass
        {
            public string ColString1 { get; set; }
            public string ColString2 { get; set; }
        }

        [Fact]
        public void CsvImportQoutedStringTest()
        {
            var lines = new[]
            {
                "ColString1;ColString2",
                "No qoute1;No qoute2",
                "\"Single Quoute1\";\"Single Quoute2\"",
                "\"Semicolon in string ; Quouted1\";\"Semicolon in string ; Quouted2\"",
                "\"newline in string ", "Quouted1\";\"newline in string ", "Quouted2\""
            };

            var csvList = new CsvImport<CsvImportStringClass>().Read(lines);

            csvList.Should().HaveCount(5 - 1);

            csvList[0].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "No qoute1", ColString2                      = "No qoute2" });
            csvList[1].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "Single Quoute1", ColString2                 = "Single Quoute2" });
            csvList[2].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "Semicolon in string ; Quouted1", ColString2 = "Semicolon in string ; Quouted2" });
            csvList[3].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "newline in string \nQuouted1", ColString2   = "newline in string \nQuouted2" });
        }

        #endregion
    }
}
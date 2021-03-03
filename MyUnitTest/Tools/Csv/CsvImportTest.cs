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

namespace Framework.MyUnitTest.Tools.Csv
{
    using System;
    using System.Globalization;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Framework.CsvImport;
    using Framework.Tools;

    using Xunit;

    public class CsvImportTest
    {
        #region CsvDateTime

        [Fact]
        public void DateTest()
        {
            var csvImport = new CsvImportBase();

            var testDate = 9.January(2009);

            csvImport.ExcelDate(testDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)).Should().Be(testDate);
        }

        [Fact]
        public void TimeTest()
        {
            var csvImport = new CsvImportBase();

            var date             = 9.January(2009).AddHours(13).AddMinutes(59).AddSeconds(58);
            var dateFraction     = date.AddMilliseconds(123);
            var dateLongFraction = date.AddMilliseconds(12345);

            csvImport.ExcelDateTime(date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)).Should().Be(date);
            csvImport.ExcelDateTime(dateFraction.ToString("yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture)).Should().Be(dateFraction);
            csvImport.ExcelDateTime(dateLongFraction.ToString("yyyy/MM/dd HH:mm:ss.fffff", CultureInfo.InvariantCulture)).Should().Be(dateLongFraction);
        }

        [Fact]
        public void DateTimeOverwriteTest()
        {
            var csvImport = new CsvImportBase();
            var format    = "MM/dd/yyyy";
            csvImport.DateFormat = format;

            var date             = 9.January(2009);
            var time             = date.AddHours(13).AddMinutes(59).AddSeconds(58);
            var timeFraction     = time.AddMilliseconds(123);
            var timeLongFraction = time.AddMilliseconds(12345);

            csvImport.ExcelDate(date.ToString(format, CultureInfo.InvariantCulture)).Should().Be(date);
            csvImport.ExcelDateTime(time.ToString($"{format} HH:mm:ss", CultureInfo.InvariantCulture)).Should().Be(time);
            csvImport.ExcelDateTime(timeFraction.ToString($"{format} HH:mm:ss.fff", CultureInfo.InvariantCulture)).Should().Be(timeFraction);
            csvImport.ExcelDateTime(timeLongFraction.ToString($"{format} HH:mm:ss.fffff", CultureInfo.InvariantCulture)).Should().Be(timeLongFraction);
        }

        #endregion

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
            public byte[]    ColByteArr                 { get; set; }
        }

        [Fact]
        public void CsvImportFileTest()
        {
            var lines = new[]
            {
                "ColString;ColInt;ColShort;ColDecimal;ColByte;ColBool;ColLong;ColEnum;ColDate;ColDateAndTime;ColDateAndTimeFraction;ColTimeSpan;ColDouble;ColIntNull;ColShortNull;ColDecimalNull;ColByteNull;ColBoolNull;ColLongNull;ColEnumNull;ColDateNull;ColDateAndTimeNull;ColDateAndTimeFractionNull;ColDoubleNull;ColTimeSpanNull;ColByteArr",
                "Str;1;2;2.5;127;true;1234567890;EnumValue1;2018/12/31;2018/12/31 15:56:45;2018/12/31 15:56:45.123;15:56:45;34.12;1;2;2.5;127;false;9876543210;EnumValue1;2018/12/31;2018/12/31 15:56:45;2018/12/31 15:56:45.123;34.12;15:56:45;0x12345678",
                ";1;2;2.5;127;true;1234567890;EnumValue2;2018/12/31;2018/12/31 15:56:45;2018/12/31 15:56:45.123;15:56:45.123;34.12;;;;;;;;;;;;"
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
                ColDoubleNull              = 34.12,
                ColByteArr                 = new byte[] { 0x12, 0x34, 0x56, 0x78 }
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

        #region StringQuoting

        public class CsvImportStringClass
        {
            public string ColString1 { get; set; }
            public string ColString2 { get; set; }
        }

        [Fact]
        public void CsvImportQuotedStringTest()
        {
            var lines = new[]
            {
                "ColString1;ColString2",
                "No quote1;No quote2",
                "\"Single Quote1\";\"Single Quote2\"",
                "\"Semicolon in string ; Quoted1\";\"Semicolon in string ; Quoted2\"",
                "\"newline in string ", "Quoted1\";\"newline in string ", "Quoted2\"",
                "\"Quote \"\"in\"\" string1\";\"Quote \"\"in\"\" string2\"",
                "\"\"\"\"\"\"\"\";\"\"\"\""
            };

            var csvList = new CsvImport<CsvImportStringClass>().Read(lines);

            csvList.Should().HaveCount(lines.Length - 3);

            csvList[0].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "No quote1", ColString2                     = "No quote2" });
            csvList[1].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "Single Quote1", ColString2                 = "Single Quote2" });
            csvList[2].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "Semicolon in string ; Quoted1", ColString2 = "Semicolon in string ; Quoted2" });
            csvList[3].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "newline in string \nQuoted1", ColString2   = "newline in string \nQuoted2" });
            csvList[4].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "Quote \"in\" string1", ColString2          = "Quote \"in\" string2" });
            csvList[5].Should().BeEquivalentTo(new CsvImportStringClass() { ColString1 = "\"\"\"", ColString2                        = "\"" });
        }

        #endregion

        #region Byte / Base64

        [Fact]
        public void CsvImageImportTest()
        {
            var csvImport = new CsvImportBase();

            var img1 = csvImport.ExcelImage("0x");
            img1.Should().HaveCount(0);

            var img2 = csvImport.ExcelImage("0x12");
            img2.Should().HaveCount(1);
            img2[0].Should().Be(0x12);

            var img3 = csvImport.ExcelImage("0x1234");
            img3.Should().HaveCount(2);
            img3[0].Should().Be(0x12);
            img3[1].Should().Be(0x34);


            Action act = () => csvImport.ExcelImage("0x12345");
            act.Should().Throw<ArgumentException>().Where(e => e.Message.Contains("odd"));
        }

        [Fact]
        public void CsvImageBase64ImportTest()
        {
            var csvImport = new CsvImportBase();

            string fromBase64 = "Test base 64";
            string toBase64   = Base64Helper.StringToBase64(fromBase64);

            var img1 = csvImport.ExcelImage(toBase64);
            var str2 = System.Text.Encoding.UTF8.GetString(img1);

            str2.Should().Be(fromBase64);
        }

        #endregion
    }
}
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

namespace Framework.Repository.Mappings;

using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class MappingExtensions
{
    #region text

    public static PropertyBuilder<string> AsText(this PropertyBuilder<string> builder, int maxLength)
    {
        return builder.IsUnicode().HasMaxLength(maxLength);
    }

    public static PropertyBuilder<string> AsRequiredText(this PropertyBuilder<string> builder, int maxLength)
    {
        return builder.IsUnicode().IsRequired().HasMaxLength(maxLength);
    }

    #endregion

    #region decimal

    public static PropertyBuilder<decimal> AsDecimal(this PropertyBuilder<decimal> builder, int length, int scale)
    {
        return builder.HasColumnType($"decimal({length},{scale})");
    }

    public static PropertyBuilder<decimal?> AsDecimal(this PropertyBuilder<decimal?> builder, int length, int scale)
    {
        return builder.HasColumnType($"decimal({length},{scale})");
    }

    #endregion

    #region date / time

    public static PropertyBuilder<DateTime> AsDate(this PropertyBuilder<DateTime> builder)
    {
        return builder.HasColumnType("date");
    }

    public static PropertyBuilder<DateTime?> AsDate(this PropertyBuilder<DateTime?> builder)
    {
        return builder.HasColumnType("date");
    }

    public static PropertyBuilder<DateTime> AsTime(this PropertyBuilder<DateTime> builder)
    {
        return builder.HasColumnType("time");
    }

    public static PropertyBuilder<DateTime?> AsTime(this PropertyBuilder<DateTime?> builder)
    {
        return builder.HasColumnType("time");
    }

    #endregion
}
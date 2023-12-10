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

namespace Framework.Repository.Abstraction.Entities;

using System;

public class Log
{
    public int      Id            { get; set; }
    public DateTime LogDate       { get; set; }
    public string?  Application   { get; set; }
    public string?  Level         { get; set; }
    public string?  Message       { get; set; }
    public string?  UserName      { get; set; }
    public string?  ServerName    { get; set; }
    public string?  MachineName   { get; set; }
    public string?  Port          { get; set; }
    public string?  Url           { get; set; }
    public string?  ServerAddress { get; set; }
    public string?  RemoteAddress { get; set; }
    public string?  Logger        { get; set; }
    public string?  Exception     { get; set; }
    public string?  StackTrace    { get; set; }
}
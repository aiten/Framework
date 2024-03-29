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

namespace Framework.Arduino.SerialCommunication;

using System;

public class SerialEventArgs : EventArgs
{
    public SerialEventArgs(string info, SerialCommand? cmd)
    {
        Command = cmd;
        if (cmd != null && string.IsNullOrEmpty(info))
        {
            Info = cmd.CommandText;
        }
        else
        {
            Info = info;
        }

        if (cmd != null)
        {
            SeqId = cmd.SeqId;
        }
    }

    public SerialEventArgs(SerialCommand? cmd)
    {
        Command = cmd;
        if (cmd != null)
        {
            Info  = cmd.CommandText;
            SeqId = cmd.SeqId;
        }
    }

    public SerialEventArgs(int queueLength, SerialCommand? cmd)
    {
        QueueLength = queueLength;
        Command     = cmd;
        if (cmd != null)
        {
            Info  = cmd.CommandText;
            SeqId = cmd.SeqId;
        }
    }

    public SerialEventArgs()
    {
    }

    public bool    Continue { get; set; } = false;
    public bool    Abort    { get; set; } = false;
    public string? Result   { get; set; }

    public string? Info        { get; }
    public int     QueueLength { get; }

    public int SeqId { get; }

    public SerialCommand? Command { get; }
}
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

namespace Framework.Arduino.SerialCommunication;

using System;
using System.Collections.Generic;

using Framework.Arduino.SerialCommunication.Abstraction;
using Framework.Pattern;

using Microsoft.Extensions.Logging;

public class HpglSerial : Serial
{
    private const int MaxMessageLength = 128;

    public HpglSerial(IFactory<ISerialPort> serialPortFactory, ILogger<Serial> logger) : base(serialPortFactory, logger)
    {
    }

    #region Overrrids

    protected override string[] SplitCommand(string line)
    {
        return SplitHpgl(line);
    }

    protected string[] SplitHpgl(string line)
    {
        var cmds    = line.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        var cmdList = new List<string>();

        foreach (string l in cmds)
        {
            var message = l;
            while (message.Length > MaxMessageLength)
            {
                var cmd = message.Substring(0, 2);
                var idx = 0;
                while (idx < MaxMessageLength && idx != -1)
                {
                    idx = message.IndexOf(',', idx + 1);
                    idx = message.IndexOf(',', idx + 1);
                }

                if (idx == -1)
                {
                    break;
                }

                string sendMessage = message.Substring(0, idx);
                message = cmd + message.Substring(idx + 1);
                cmdList.Add(sendMessage);
            }

            cmdList.Add(message);
        }

        return cmdList.ToArray();
    }

    #endregion
}
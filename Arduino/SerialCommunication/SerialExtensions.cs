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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Abstraction;

public static class SerialExtensions
{
    const int DefaultTimeout = 10 * 60 * 1000;

    /// <summary>
    /// Send command and wait until the command is transferred and we got a reply (no command pending)
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="line">command line to send</param>
    public static IEnumerable<SerialCommand> SendCommand(this ISerial serial, string line)
    {
        return serial.SendCommandsAsync(new[] { line }, DefaultTimeout).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Send command and wait until the command is transferred and we got a reply (no command pending)
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="line">command line to send</param>
    /// <param name="waitForMilliseconds"></param>
    public static async Task<IEnumerable<SerialCommand>> SendCommandAsync(this ISerial serial, string line, int waitForMilliseconds = DefaultTimeout)
    {
        return await serial.SendCommandsAsync(new[] { line }, waitForMilliseconds);
    }

    /// <summary>
    /// Send multiple command lines to the arduino. Wait until the commands are transferred and we got a reply (no command pending)
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="commands"></param>
    public static IEnumerable<SerialCommand> SendCommands(this ISerial serial, IEnumerable<string> commands)
    {
        return serial.SendCommandsAsync(commands, DefaultTimeout).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Queue command - do not wait - not for transfer and not for replay
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="line">command line to send</param>
    public static IEnumerable<SerialCommand> QueueCommand(this ISerial serial, string line)
    {
        return serial.QueueCommandsAsync(new[] { line }).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Send commands stored in a file. Wait until the commands are transferred and we got a reply (no command pending)
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="filename">used for a StreamReader</param>
    /// <param name="waitForMilliseconds"></param>
    public static async Task<IEnumerable<SerialCommand>> SendFileAsync(this ISerial serial, string filename, int waitForMilliseconds = DefaultTimeout)
    {
        var list = await serial.QueueFileAsync(filename);
        await serial.WaitUntilQueueEmptyAsync(waitForMilliseconds);
        return list;
    }

    /// <summary>
    /// Send commands stored in a file. Wait until the commands are transferred and we got a reply (no command pending)
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="filename">used for a StreamReader</param>
    public static async Task<IEnumerable<SerialCommand>> QueueFileAsync(this ISerial serial, string filename)
    {
        using (var sr = new StreamReader(filename))
        {
            string? line;
            var     lines = new List<string>();
            while ((line = await sr.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }

            return await serial.QueueCommandsAsync(lines.ToArray());
        }
    }

    /// <summary>
    /// Send a command to the arduino and wait until a (OK) reply
    /// Queue must be empty
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="line">command line</param>
    /// <param name="waitForMilliseconds"></param>
    /// <returns>ok result from arduino or empty(if error)</returns>
    public static async Task<string?> SendCommandAndReadOKReplyAsync(this ISerial serial, string line, int waitForMilliseconds)
    {
        var ret = (await serial.SendCommandAsync(line, waitForMilliseconds)).ToList();
        if (ret.Any())
        {
            var last = ret.Last();
            if (last.ReplyType.HasFlag(EReplyType.ReplyOk))
            {
                return last.ResultText;
            }
        }

        return null;
    }

    /// <summary>
    /// write all pending (command with no reply) to file
    /// Intended to be used if user abort queue because of an error
    /// </summary>
    /// <param name="serial"></param>
    /// <param name="filename"></param>
    public static void WritePendingCommandsToFile(this ISerial serial, string filename)
    {
        using (var sw = new StreamWriter(Environment.ExpandEnvironmentVariables(filename)))
        {
            foreach (SerialCommand cmd in serial.PendingCommands)
            {
                sw.WriteLine(cmd.CommandText);
            }
        }
    }

    public static void WriteCommandHistory(this ISerial serial, string filename)
    {
        using (var sr = new StreamWriter(Environment.ExpandEnvironmentVariables(filename)))
        {
            foreach (SerialCommand cmds in serial.CommandHistoryCopy)
            {
                sr.Write(cmds.SentTime);
                sr.Write(":");
                sr.Write(cmds.CommandText);
                sr.Write(" => ");
                sr.WriteLine(cmds.ResultText);
            }
        }
    }

    private const int MaxMessageLength = 60;

    public static IEnumerable<string> SplitHpglCommands(IEnumerable<string> commands)
    {
        var cmdList = new List<string>();

        foreach (var command in commands)
        {
            var cmds = command.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

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

                    // TODO => idx + 3 must be < MaxMessageLength
                    throw new NotImplementedException();

                    string sendMessage = message.Substring(0, idx);
                    message = cmd + message.Substring(idx + 1);
                    cmdList.Add(sendMessage);
                }

                cmdList.Add(message);
            }
        }

        return cmdList;
    }
}
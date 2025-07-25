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

namespace Framework.MyUnitTest.SerialCommunication;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Framework.Arduino.SerialCommunication;
using Framework.Arduino.SerialCommunication.Abstraction;
using Framework.Pattern;
using Framework.UnitTest;

using Microsoft.Extensions.Logging;

using NLog;

using NSubstitute;

using Xunit;

public class SerialTest : UnitTestBase
{
    int  _resultIdx;
    bool _sendReply;

    string _OkTag    = @"ok";
    string _errorTag = @"error:";
    string _infoTag  = @"info:";

    private ISerialPort CreateSerialPortMock(string[] responseStrings)
    {
        var serialPort = Substitute.For<ISerialPort>();
        var baseStream = Substitute.For<MemoryStream>();

        serialPort.BaseStream.ReturnsForAnyArgs(baseStream);

        Encoding encoding = Encoding.GetEncoding(1200);
        serialPort.Encoding.ReturnsForAnyArgs(encoding);

        _resultIdx = 0;
        _sendReply = false;

        baseStream.When(x => x.Write(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()))
            .Do(x =>
            {
                _sendReply = true;
                /*
                                var cancellationToken = (CancellationToken)x[3];
                                while (!cancellationToken.IsCancellationRequested)
                                {
                                    Thread.Sleep(10);
                                }
                */
            });

        baseStream.Read(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(x =>
        {
            if (_sendReply)
            {
                _sendReply = false;
                byte[] encodedStr = encoding.GetBytes(responseStrings[_resultIdx++]);
                for (int i = 0; i < encodedStr.Length; i++)
                {
                    ((byte[])x[0])[i] = encodedStr[i];
                }

                return encodedStr.Length;
            }


            Thread.Sleep(100);

            return 0;
        });

        return serialPort;
    }

    private ILogger<Serial> CreateLogger()
    {
        return Substitute.For<ILogger<Serial>>();
    }

    [Fact]
    public async Task ConnectSerialTest()
    {
        using (var serialPort = CreateSerialPortMock(Array.Empty<string>()))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            await serial.ConnectAsync("com2", null, null, null);
            serial.CommandsInQueue.Should().Be(0);

            await serial.DisconnectAsync();
        }
    }

    [Fact]
    public async Task WriteOneCommandSerialTest()
    {
        using (var serialPort = CreateSerialPortMock(
                   new[]
                   {
                       _OkTag + "\n\r"
                   }))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            await serial.ConnectAsync("com2", null, null, null);

            await serial.SendCommandAsync("?", 1000);

            await serial.DisconnectAsync();

            serialPort.BaseStream.Received(1).Write(Arg.Is<byte[]>(e => (char)e[0] == '?'), 0, 2);
            await Task.FromResult(0);
        }
    }

    [Fact]
    public async Task WriteTwoCommandSerialTest()
    {
        using (var serialPort = CreateSerialPortMock(
                   new[]
                   {
                       _OkTag + "\n\r", _OkTag + "\n\r"
                   }))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            await serial.ConnectAsync("com2", null, null, null);

            await serial.SendCommandAsync("?");
            await serial.SendCommandAsync("?");

            await serial.DisconnectAsync();
            serialPort.BaseStream.Received(2).Write(Arg.Is<byte[]>(e => (char)e[0] == '?'), 0, 2);
            await Task.FromResult(0);
        }
    }

    #region events

    sealed class EventCalls
    {
        public int EventWaitForSend         { get; set; }
        public int EventCommandSending      { get; set; }
        public int EventCommandSent         { get; set; }
        public int EventWaitCommandSent     { get; set; }
        public int EventReplyReceived       { get; set; }
        public int EventReplyOK             { get; set; }
        public int EventReplyError          { get; set; }
        public int EventReplyInfo           { get; set; }
        public int EventReplyUnknown        { get; set; }
        public int EventCommandQueueChanged { get; set; }
        public int EventCommandQueueEmpty   { get; set; }
    }

    private EventCalls SubscribeForEventCall(ISerial serial)
    {
        var eventCounts = new EventCalls();
        serial.WaitForSend         += (_, _) => eventCounts.EventWaitForSend++;
        serial.CommandSending      += (_, _) => eventCounts.EventCommandSending++;
        serial.CommandSent         += (_, _) => eventCounts.EventCommandSent++;
        serial.WaitCommandSent     += (_, _) => eventCounts.EventWaitCommandSent++;
        serial.ReplyReceived       += (_, _) => eventCounts.EventReplyReceived++;
        serial.ReplyOk             += (_, _) => eventCounts.EventReplyOK++;
        serial.ReplyError          += (_, _) => eventCounts.EventReplyError++;
        serial.ReplyInfo           += (_, _) => eventCounts.EventReplyInfo++;
        serial.ReplyUnknown        += (_, _) => eventCounts.EventReplyUnknown++;
        serial.CommandQueueChanged += (_, _) => eventCounts.EventCommandQueueChanged++;
        serial.CommandQueueEmpty   += (_, _) => eventCounts.EventCommandQueueEmpty++;
        return eventCounts;
    }

    [Fact]
    public async Task OkEventSerialTest()

    {
        using (var serialPort = CreateSerialPortMock(
                   new[]
                   {
                       _OkTag + "\n\r"
                   }))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            var eventCalls = SubscribeForEventCall(serial);

            await serial.ConnectAsync("com2", null, null, null);

            await serial.SendCommandAsync("?");

            await serial.DisconnectAsync();

            eventCalls.EventWaitForSend.Should().Be(0);
            eventCalls.EventCommandSending.Should().Be(1);
            eventCalls.EventCommandSent.Should().Be(1);
            eventCalls.EventWaitCommandSent.Should().BeGreaterThanOrEqualTo(1);
            eventCalls.EventReplyReceived.Should().Be(1);
            eventCalls.EventReplyOK.Should().Be(1);
            eventCalls.EventReplyError.Should().Be(0);
            eventCalls.EventReplyInfo.Should().Be(0);
            eventCalls.EventReplyUnknown.Should().Be(0);
            eventCalls.EventCommandQueueChanged.Should().Be(2);
            eventCalls.EventCommandQueueEmpty.Should().Be(2);

            await Task.FromResult(0);
        }
    }

    [Fact]
    public async Task InfoEventSerialTest()
    {
        using (var serialPort = CreateSerialPortMock(
                   new[]
                   {
                       _infoTag + "\n\r" + _OkTag + "\n\r"
                   }))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            var eventCalls = SubscribeForEventCall(serial);

            await serial.ConnectAsync("com2", null, null, null);

            await serial.SendCommandAsync("?");

            await serial.DisconnectAsync();

            eventCalls.EventWaitForSend.Should().Be(0);
            eventCalls.EventCommandSending.Should().Be(1);
            eventCalls.EventCommandSent.Should().Be(1);
            eventCalls.EventWaitCommandSent.Should().BeGreaterThanOrEqualTo(1);
            eventCalls.EventReplyReceived.Should().Be(2);
            eventCalls.EventReplyOK.Should().Be(1);
            eventCalls.EventReplyError.Should().Be(0);
            eventCalls.EventReplyInfo.Should().Be(1);
            eventCalls.EventReplyUnknown.Should().Be(0);
            eventCalls.EventCommandQueueChanged.Should().Be(2);
            eventCalls.EventCommandQueueEmpty.Should().Be(2);

            await Task.FromResult(0);
        }
    }

    [Fact]
    public async Task ErrorEventWithOkSerialTest()
    {
        using (var serialPort = CreateSerialPortMock(
                   new[]
                   {
                       _errorTag + "\n\r" + _OkTag + "\n\r"
                   }))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            var eventCalls = SubscribeForEventCall(serial);

            serial.ErrorIsReply = false;

            await serial.ConnectAsync("com2", null, null, null);

            await serial.SendCommandAsync("?");

            await serial.DisconnectAsync();

            eventCalls.EventWaitForSend.Should().Be(0);
            eventCalls.EventCommandSending.Should().Be(1);
            eventCalls.EventCommandSent.Should().Be(1);
            eventCalls.EventWaitCommandSent.Should().BeGreaterThanOrEqualTo(1);
            eventCalls.EventReplyReceived.Should().Be(2);
            eventCalls.EventReplyOK.Should().Be(1);
            eventCalls.EventReplyError.Should().Be(1);
            eventCalls.EventReplyInfo.Should().Be(0);
            eventCalls.EventReplyUnknown.Should().Be(0);
            eventCalls.EventCommandQueueChanged.Should().Be(2);
            eventCalls.EventCommandQueueEmpty.Should().Be(2);

            await Task.FromResult(0);
        }
    }

    [Fact]
    public async Task ErrorEventWithOutOkSerialTest()
    {
        using (var serialPort = CreateSerialPortMock(
                   new[]
                   {
                       _errorTag + "\n\r"
                   }))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            var eventCalls = SubscribeForEventCall(serial);

            serial.ErrorIsReply = true; // no OK is needed

            await serial.ConnectAsync("com2", null, null, null);

            await serial.SendCommandAsync("?");

            await serial.DisconnectAsync();

            eventCalls.EventWaitForSend.Should().Be(0);
            eventCalls.EventCommandSending.Should().Be(1);
            eventCalls.EventCommandSent.Should().Be(1);
            eventCalls.EventWaitCommandSent.Should().BeGreaterThanOrEqualTo(1);
            eventCalls.EventReplyReceived.Should().Be(1);
            eventCalls.EventReplyOK.Should().Be(0);
            eventCalls.EventReplyError.Should().Be(1);
            eventCalls.EventReplyInfo.Should().Be(0);
            eventCalls.EventReplyUnknown.Should().Be(0);
            eventCalls.EventCommandQueueChanged.Should().Be(2);
            eventCalls.EventCommandQueueEmpty.Should().Be(2);
        }
    }

    [Fact]
    public async Task UnknownEventSerialTest()
    {
        using (var serialPort = CreateSerialPortMock(
                   new[]
                   {
                       "Hallo\n\r" + _OkTag + "\n\r"
                   }))
        using (var serial = new Serial(new FactoryInstance<ISerialPort>(serialPort), CreateLogger()))
        {
            var eventCalls = SubscribeForEventCall(serial);

            serial.ErrorIsReply = true; // no OK is needed

            await serial.ConnectAsync("com2", null, null, null);

            await serial.SendCommandAsync("?");

            await serial.DisconnectAsync();

            eventCalls.EventWaitForSend.Should().Be(0);
            eventCalls.EventCommandSending.Should().Be(1);
            eventCalls.EventCommandSent.Should().Be(1);
            eventCalls.EventWaitCommandSent.Should().BeGreaterThanOrEqualTo(1);
            eventCalls.EventReplyReceived.Should().Be(2);
            eventCalls.EventReplyOK.Should().Be(1);
            eventCalls.EventReplyError.Should().Be(0);
            eventCalls.EventReplyInfo.Should().Be(0);
            eventCalls.EventReplyUnknown.Should().Be(1);
            eventCalls.EventCommandQueueChanged.Should().Be(2);
            eventCalls.EventCommandQueueEmpty.Should().Be(2);
        }
    }

    #endregion
}
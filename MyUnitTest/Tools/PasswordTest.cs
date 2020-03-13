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

using System.Linq;

using FluentAssertions;

using Framework.Tools;
using Framework.Tools.Password;

using Xunit;

namespace Framework.MyUnitTest.Tools
{
    public class PasswordTest
    {
        [Fact]
        public void Pbkdf2PasswordTest()
        {
            var password1 = "MySecretPassword1";
            var password2 = "MySecretPassword2";
            var password3 = "Serial.Server";

            var hashProvider = new Pbkdf2PasswordProvider();

            var hash1 = hashProvider.GetPasswordHash(password1);
            var hash2 = hashProvider.GetPasswordHash(password2);
            var hash3 = hashProvider.GetPasswordHash(password3);

            hash1.Should().NotContain(password1);
            hash1.Length.Should().BeGreaterThan(1);

            hashProvider.ValidatePassword(password1, hash1).Should().BeTrue();
            hashProvider.ValidatePassword(password2, hash2).Should().BeTrue();
            hashProvider.ValidatePassword(password2, hash1).Should().BeFalse();
            hashProvider.ValidatePassword(password1, hash2).Should().BeFalse();
        }
    }
}
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

namespace Framework.MyUnitTest.Tools
{
    using FluentAssertions;

    using Framework.Tools.Password;

    using Xunit;

    public class PasswordTest
    {
        [Fact]
        public void Pbkdf2PasswordTest()
        {
            var password1 = "MySecretPassword1";
            var password2 = "MySecretPassword2";

            var hashProvider = new Pbkdf2PasswordProvider();

            var hash1 = hashProvider.GetPasswordHash(password1);
            var hash2 = hashProvider.GetPasswordHash(password2);

            hash1.Should().NotContain(password1);
            hash1.Length.Should().BeGreaterThan(1);

            hashProvider.ValidatePassword(password1, hash1).Should().BeTrue();
            hashProvider.ValidatePassword(password2, hash2).Should().BeTrue();
            hashProvider.ValidatePassword(password2, hash1).Should().BeFalse();
            hashProvider.ValidatePassword(password1, hash2).Should().BeFalse();
        }

        [Fact]
        public void AesPasswordTest()
        {
            var password1 = "MySecretPassword1";
            var password2 = "MySecretPassword2";

            var key1 = "MySecretPasswordKey1";
            var key2 = "MySecretPasswordKey2";

            var aesProvider1 = new AesPasswordProvider(key1);
            var aesProvider2 = new AesPasswordProvider(key2);

            var hash11 = aesProvider1.GetPasswordHash(password1);
            var hash21 = aesProvider2.GetPasswordHash(password1);

            var hash12 = aesProvider1.GetPasswordHash(password2);
            var hash22 = aesProvider2.GetPasswordHash(password2);

            hash11.Should().NotBeEquivalentTo(hash21);
            hash12.Should().NotBeEquivalentTo(hash22);

            aesProvider1.ValidatePassword(password1, hash11).Should().BeTrue();
            aesProvider1.ValidatePassword(password2, hash12).Should().BeTrue();

            aesProvider2.ValidatePassword(password1, hash21).Should().BeTrue();
            aesProvider2.ValidatePassword(password2, hash22).Should().BeTrue();

            aesProvider1.ValidatePassword(password2, hash11).Should().BeFalse();
            aesProvider1.ValidatePassword(password1, hash12).Should().BeFalse();

            aesProvider2.ValidatePassword(password2, hash21).Should().BeFalse();
            aesProvider2.ValidatePassword(password1, hash22).Should().BeFalse();
        }
    }
}
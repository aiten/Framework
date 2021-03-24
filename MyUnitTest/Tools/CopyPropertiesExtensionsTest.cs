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

    using Framework.Tools;

    using Xunit;

    public class CopyPropertiesExtensionsTest
    {
        public class MyObject
        {
            public int Ignore { get; set; }

            public int PropInt { get; set; }

            public int? PropIntNull { get; set; }

            public string PropString { get; set; }
        }

        [Fact]
        public void CopyPropertiesExtensions()
        {
            var objSrc = new MyObject()
            {
                Ignore      = 1234,
                PropInt     = 1,
                PropIntNull = 2,
                PropString  = "3"
            };
            var objDest = new MyObject();

            objDest.CopyProperties(objSrc, nameof(MyObject.Ignore));

            objDest.Ignore.Should().Be(0);
            objDest.Ignore = objSrc.Ignore;

            objDest.Should().BeEquivalentTo(objSrc);
        }

        [Fact]
        public void CopyChangedPropertiesExtensions()
        {
            var objSrc = new MyObject()
            {
                Ignore      = 1234,
                PropInt     = 1,
                PropIntNull = 2,
                PropString  = "3"
            };
            var objSrc2 = new MyObject();

            var objDest = new MyObject();

            // null + null  => null
            var changedFields = objDest.CopyChangedProperties(objDest, nameof(MyObject.Ignore));
            changedFields.Should().HaveCount(0);

            // null + not null => not null
            changedFields = objDest.CopyChangedProperties(objSrc, nameof(MyObject.Ignore));

            objDest.Ignore.Should().Be(0);
            objDest.Ignore = objSrc.Ignore;

            objDest.Should().BeEquivalentTo(objSrc);
            changedFields.Should().HaveCount(3);

            // not null + not null => not null
            objSrc.PropIntNull = 22;
            objSrc.PropString  = "33";
            changedFields      = objDest.CopyChangedProperties(objSrc, nameof(MyObject.Ignore));

            objDest.Should().BeEquivalentTo(objSrc);
            changedFields.Should().HaveCount(2);

            // not null + null => null
            changedFields = objDest.CopyChangedProperties(objSrc2, nameof(MyObject.Ignore));

            objDest.Ignore.Should().Be(objSrc.Ignore);
            objDest.Ignore = objSrc2.Ignore;

            objDest.Should().BeEquivalentTo(objSrc2);
            changedFields.Should().HaveCount(3);
        }
    }
}
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

namespace Framework.MyUnitTest.Logic;

using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using FluentAssertions;

using Framework.Logic;
using Framework.Logic.Abstraction;
using Framework.Tools;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Repository.Abstraction;

using Xunit;

public class CrudManagerTest
{
    public class MyDto
    {
        public int     Id                { get; set; }
        public string? StringProperty    { get; set; }
        public int     NotMappedProperty { get; set; }
    }

    public class MyEntity
    {
        public int     Id             { get; set; }
        public string? StringProperty { get; set; }
    }

    private class MyCrudManager : CrudManager<MyDto, int, MyEntity>
    {
        public MyCrudManager(IUnitOfWork unitOfWork, ICrudRepository<MyEntity, int> repository, IMapper mapper) : base(unitOfWork, repository, mapper)
        {
        }

        protected override int GetKey(MyEntity entity)
        {
            return entity.Id;
        }
    }

    private ICrudManager<MyDto, int> CreateManager()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repository = Substitute.For<ICrudRepository<MyEntity, int>>();

        _repository
            .When(x => x.SetValueGraphAsync(Arg.Any<MyEntity>(), Arg.Any<MyEntity>()))
            .Do(
                Callback.Always(x => ((MyEntity)x[0]).CopyProperties((MyEntity)x[1]))
            );

        var configuration = new MapperConfiguration(cfg => { cfg.CreateMap<MyEntity, MyDto>().ReverseMap(); },new LoggerFactory());

        return new MyCrudManager(_unitOfWork, _repository, configuration.CreateMapper());
    }

    private ICrudRepository<MyEntity, int>? _repository;
    private IUnitOfWork?                    _unitOfWork;

    #region UpdateAsync

    [Fact]
    public async Task UpdateOne()
    {
        var manager      = CreateManager();
        var getTracking  = new MyEntity() { Id = 1, StringProperty = "Hallo1" };
        var getTrackings = (IList<MyEntity>)new List<MyEntity>() { getTracking };

        _repository!.GetTrackingAsync(Arg.Any<IEnumerable<int>>()).Returns(Task.FromResult(getTrackings));

        var dtoTo = new MyDto() { Id = 1, StringProperty = "Hallo1-Neu" };

        await manager.UpdateAsync(dtoTo);

        getTracking.StringProperty.Should().Be("Hallo1-Neu");
    }

    [Fact]
    public async Task UpdateMany()
    {
        var manager      = CreateManager();
        var getTracking1 = new MyEntity() { Id = 1, StringProperty = "Hallo1" };
        var getTracking2 = new MyEntity() { Id = 2, StringProperty = "Hallo2" };
        var getTrackings = (IList<MyEntity>)new List<MyEntity>() { getTracking1, getTracking2 };

        _repository!.GetTrackingAsync(Arg.Any<IEnumerable<int>>()).Returns(Task.FromResult(getTrackings));

        var dtoTo1 = new MyDto() { Id = 1, StringProperty = "Hallo1-Neu" };
        var dtoTo2 = new MyDto() { Id = 2, StringProperty = "Hallo2-Neu" };

        await manager.UpdateAsync(new[] { dtoTo1, dtoTo2 });

        getTrackings.Should().OnlyContain(x => x.StringProperty!.EndsWith("-Neu"));
    }

    #endregion

    #region PatchAsync

    [Fact]
    public async Task PatchOne()
    {
        var manager      = CreateManager();
        var getTracking  = new MyEntity() { Id = 1, StringProperty = "Hallo1" };
        var getTrackings = (IList<MyEntity>)new List<MyEntity>() { getTracking };

        _repository!.GetAsync(1)!.Returns(Task.FromResult(getTracking));
        _repository!.GetTrackingAsync(Arg.Any<IEnumerable<int>>()).Returns(Task.FromResult(getTrackings));

        var patch = new JsonPatchDocument<MyDto>();
        patch.Replace(x => x.StringProperty, "Hallo1-Neu");

        await manager.PatchAsync(1, patch);

        getTracking.StringProperty.Should().Be("Hallo1-Neu");
    }

    #endregion
}
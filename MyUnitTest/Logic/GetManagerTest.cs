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
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using FluentAssertions;

using Framework.Logic;
using Framework.Logic.Abstraction;

using NSubstitute;

using Repository.Abstraction;

using Xunit;

public class GetManagerTest
{
    public class MyDto
    {
        public int    Id                { get; set; }
        public string StringProperty    { get; set; }
        public int    NotMappedProperty { get; set; }
    }

    public class MyEntity
    {
        public int    Id             { get; set; }
        public string StringProperty { get; set; }
    }

    private class MyGetManager : GetManager<MyDto, int, MyEntity>
    {
        public MyGetManager(IUnitOfWork unitOfWork, IGetRepository<MyEntity, int> repository, IMapper mapper) : base(unitOfWork, repository, mapper)
        {
        }

        protected override int GetKey(MyEntity entity)
        {
            return entity.Id;
        }

        protected override Task<MyDto> SetDtoAsync(MyDto dto)
        {
            if (dto != null)
            {
                dto.NotMappedProperty = dto.Id;
            }

            return base.SetDtoAsync(dto);
        }
    }

    private IGetManager<MyDto, int> CreateManager()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repository = Substitute.For<IGetRepository<MyEntity, int>>();
        var configuration = new MapperConfiguration(cfg => { cfg.CreateMap<MyEntity, MyDto>(); });

        return new MyGetManager(_unitOfWork, _repository, configuration.CreateMapper());
    }

    private IGetRepository<MyEntity, int> _repository;
    private IUnitOfWork                   _unitOfWork;

    [Fact]
    public async Task GetAllEmpty()
    {
        var manager = CreateManager();
        var all     = (await manager.GetAllAsync()).ToList();
        all.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetAllMany()
    {
        var manager = CreateManager();
        var getAllResult = (IList<MyEntity>)new List<MyEntity>()
        {
            new() { Id = 1, StringProperty = "Hallo1" },
            new() { Id = 2, StringProperty = "Hallo2" }
        };

        _repository.GetAllAsync().Returns(Task.FromResult(getAllResult));

        var all = (await manager.GetAllAsync()).ToList();
        all.Should().HaveCount(2);
        all.Should().Contain(x => x.Id == 1 && x.StringProperty == "Hallo1" && x.NotMappedProperty == 1);
        all.Should().Contain(x => x.Id == 2 && x.StringProperty == "Hallo2" && x.NotMappedProperty == 2);
    }

    [Fact]
    public async Task GetByIdNotFound()
    {
        var manager = CreateManager();

        var id1 = await manager.GetAsync(2);
        id1.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOne()
    {
        var manager = CreateManager();

        _repository.GetAsync(1).Returns(Task.FromResult(new MyEntity() { Id = 1, StringProperty = "Hallo1" }));

        var id1 = await manager.GetAsync(1);
        id1.Should().NotBeNull();
        id1.Should().BeEquivalentTo(new MyDto() { Id = 1, StringProperty = "Hallo1", NotMappedProperty = 1 });
    }

    [Fact]
    public async Task GetByIdMany()
    {
        // we ask for 3 but only get 2!

        var manager = CreateManager();

        var ids = new[] { 1, 10, 100 };

        var getAllResult = (IList<MyEntity>)new List<MyEntity>()
        {
            new() { Id = 1, StringProperty  = "Hallo1" },
            new() { Id = 10, StringProperty = "Hallo10" }
        };

        _repository.GetAsync(ids).Returns(Task.FromResult(getAllResult));

        var dtos = await manager.GetAsync(ids);
        dtos.Should().NotBeNull();
        dtos.Should().HaveCount(2);
        dtos.Should().Contain(x => x.Id == 1 && x.StringProperty == "Hallo1" && x.NotMappedProperty == 1);
        dtos.Should().Contain(x => x.Id == 10 && x.StringProperty == "Hallo10" && x.NotMappedProperty == 10);
    }
}
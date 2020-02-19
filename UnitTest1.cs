using System;
using AutoMapper;
using AutoMapper.Configuration;
using NUnit.Framework;

namespace AutoMapperTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test基本用法()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<UserInfoModel, UserInfoViewModel>());
            //config.AssertConfigurationIsValid();//←證驗應對 ,比對不到會exception
            var mapper = config.CreateMapper();

            var result = mapper.Map<UserInfoViewModel>(new UserInfoModel {Age = 10, Name = "kenny", RowId = 1});
            Assert.AreEqual("kenny", result.Name);
        }

        [Test]
        public void Test_profile用法()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<UserInfoProfile>(); });
            config.AssertConfigurationIsValid();//←證驗應對 ,比對不到會exception
            var mapper = config.CreateMapper();
            
            var result = mapper.Map<UserInfoViewModel>(new UserInfoModel { Age = 10, Name = "kenny", RowId = 1 });
            Assert.AreEqual("kenny 10", result.Detail);
        }

        [Test]
        public void Test_ConfigForDI()
        {
            var mapperFactory = new UserInfoConfigFactory();
            MapperConfiguration config = mapperFactory.GetConfig();
            var mapper = config.CreateMapper();

            var result = mapper.Map<UserInfoViewModel>(new UserInfoModel { Age = 10, Name = "kenny", RowId = 1 , CrTime = new DateTime(2020,1,1)});
            Assert.AreEqual("kenny 10", result.Detail);
            Assert.AreEqual("2020-01-01", result.CrTime);
        }

        [Test]
        public void Test_ProfileForDI()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile(new UserInfoProfileImpl()); });

            config.AssertConfigurationIsValid();//←證驗應對 ,比對不到會exception
            //var mapper = config.CreateMapper();
            var mapper = new Mapper(config);

            var result = mapper.Map<UserInfoViewModel>(new UserInfoModel { Age = 10, Name = "kenny", RowId = 1, CrTime = new DateTime(2020, 1, 1) });
            Assert.AreEqual("kenny 10", result.Detail);
            Assert.AreEqual("2020-01-01", result.CrTime);
        }


        public class UserInfoProfile : Profile
        {
            public UserInfoProfile()
            {
                CreateMap<DateTime, string>().ConvertUsing(s => s.ToString("yyyy-MM-dd"));
                CreateMap<UserInfoModel, UserInfoViewModel>()
                    .ForMember(t => t.Detail,
                        s => s.MapFrom(m=> $"{m.Name} {m.Age}"));
            }
        }

        public class UserInfoProfileImpl : UserInfoProfile
        {
        }

        public class UserInfoModel
        {
            public int RowId { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }

            public DateTime CrTime { set; get; }
        }

        public class UserInfoViewModel
        {

            public string Name { get; set; }
            public string Detail { get; set; }

            public string CrTime { set; get; }

        }
    }

    public interface IAutoMapperConfigFactory
    {
        MapperConfiguration GetConfig();
    }

    public abstract class AutoMapperConfigFactory : IAutoMapperConfigFactory
    {
        private readonly MapperConfigurationExpression _cfg;

        protected AutoMapperConfigFactory()
        {
            _cfg = new MapperConfigurationExpression();

            //default
            _cfg.CreateMap<string, int>().ConvertUsing(s => Convert.ToInt32(s));
            _cfg.CreateMap<DateTime, string>().ConvertUsing(s => s.ToString("yyyy-MM-dd"));
        }

        public MapperConfiguration GetConfig()
        {
            _cfg.AddProfile<Tests.UserInfoProfile>();
            var mapperConfig = new MapperConfiguration(_cfg);

            return mapperConfig;
        }
    }

    public class UserInfoConfigFactory : AutoMapperConfigFactory
    {
    }
}
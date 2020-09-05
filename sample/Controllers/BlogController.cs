using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.AspNetCore.Mvc;
using MQuery;

[Route("api/blogs")]
[ApiController]
public class BlogController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Blog>> Query(Query<BlogDTO> query)
    {
        var blogs = new List<Blog>
        {
            new Blog{Id = 1,Title = "Asp.Net Core", CreateTime = new DateTime(2020,4,1),Likes = 1024},
            new Blog{Id = 2,Title = "EntityFramework Core", CreateTime = new DateTime(2020,3,2),Likes = 8888},
            new Blog{Id = 3,Title = "Asp.Net Core MVC", CreateTime = new DateTime(2020,5,8),Likes = 6412},
            new Blog{Id = 4,Title = "Asp.Net Core WebApi", CreateTime = new DateTime(2020,4,12),Likes = 102},
            new Blog{Id = 5,Title = "Mirosoft SqlServer", CreateTime = new DateTime(2020,3,26),Likes = null},
            new Blog{Id = 6,Title = null, CreateTime = new DateTime(2020,3,26),Likes = 3},
        };

        var mapperConfig = new MapperConfiguration(config =>
        {
            config.AddExpressionMapping();
            config.CreateMap<Blog, BlogDTO>();
        });
        var mapper = mapperConfig.CreateMapper();
        var dtoQuery = query.BuildOrderPlan(query.BuildWherePlan());
        var exp = mapper.MapExpression<Expression<Func<IQueryable<BlogDTO>, IQueryable<BlogDTO>>>, Expression<Func<IQueryable<Blog>, IQueryable<Blog>>>>(dtoQuery);
        var result = exp.Compile()(blogs.AsQueryable());

        return Ok(result);
    }
}

public class BlogDTO
{
    public int Id { get; set; }
}

public class Blog
{
    public int Id { get; set; }

    public string Title { get; set; }

    public DateTime CreateTime { get; set; }

    public int? Likes { get; set; }
}

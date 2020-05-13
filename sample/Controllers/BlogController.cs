using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MQuery;

[Route("api/blogs")]
[ApiController]
public class BlogController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Blog>> Query(Query<Blog> query)
    {
        var blogs = new List<Blog>
        {
            new Blog {Id = 1, Title = "IOC-DI", CreateTime = new DateTime(2020, 3, 26), Likes = null},
            new Blog {Id = 2, Title = "MVC", CreateTime = new DateTime(2020, 4, 1), Likes = 1024},
            new Blog {Id = 3, Title = "Blazor", CreateTime = new DateTime(2020, 5, 8), Likes = 6412},
            new Blog {Id = 4, Title = "Web Api", CreateTime = new DateTime(2020, 3, 2), Likes = 8888},
            new Blog {Id = 5, Title = "SignalR", CreateTime = new DateTime(2020, 4, 12), Likes = 102},
            new Blog {Id = 6, Title = "gRPC", CreateTime = new DateTime(2020, 6, 30), Likes = 102},
            new Blog {Id = 7, Title = null, CreateTime = new DateTime(2020, 3, 26), Likes = 3},
        };

        var result = blogs.AsQueryable().Query(query);
        return Ok(result);
    }
}

public class Blog
{
    public int Id { get; set; }

    public string Title { get; set; }

    public DateTime CreateTime { get; set; }

    public int? Likes { get; set; }
}

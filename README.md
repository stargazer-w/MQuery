# MQuery

### 介绍
基于类似MongoDB查询语法的Http QueryString查询模式。

目前实现了`$eq`（等于）`$ne`（不等于）`$gt`（大于）`$gte`（大于等于）`$lt`（小于）`$lte`（小于等于）`$in`（包含于）`$nin`（不包含于）操作符。

### 使用说明
#### 数据源和Action配置

```CSharp
List<Blog> Blogs = new List<Blog>
{
    new Blog {Id = 1, Title = "IOC-DI", CreateTime = new DateTime(2020, 3, 26), Likes = null},
    new Blog {Id = 2, Title = "MVC", CreateTime = new DateTime(2020, 4, 1), Likes = 1024},
    new Blog {Id = 3, Title = "Blazor", CreateTime = new DateTime(2020, 5, 8), Likes = 6412},
    new Blog {Id = 4, Title = "Web Api", CreateTime = new DateTime(2020, 3, 2), Likes = 8888},
    new Blog {Id = 5, Title = "SignalR", CreateTime = new DateTime(2020, 4, 12), Likes = 102},
    new Blog {Id = 6, Title = "gRPC", CreateTime = new DateTime(2020, 6, 30), Likes = 102},
    new Blog {Id = 7, Title = null, CreateTime = new DateTime(2020, 3, 26), Likes = 3},
};

[HttpGet("api/blogs")]
public ActionResult<IEnumerable<Blog>> Query(Query<Blog> query)
{
    var result = query.ApplyTo(Blogs.AsQueryable());
    return Ok(result);
}
```

#### 筛选

##### 等于筛选

`https://localhost:44396/api/blogs?title=Web%20Api`

```JSON
[
  {
    "id": 4,
    "title": "Web Api",
    "createTime": "2020-03-02T00:00:00",
    "likes": 8888
  }
]
```

##### 大于筛选

`https://localhost:44396/api/blogs?likes[$gt]=500`

```JSON
[
  {
    "id": 2,
    "title": "MVC",
    "createTime": "2020-04-01T00:00:00",
    "likes": 1024
  },
  {
    "id": 3,
    "title": "Blazor",
    "createTime": "2020-05-08T00:00:00",
    "likes": 6412
  },
  {
    "id": 4,
    "title": "Web Api",
    "createTime": "2020-03-02T00:00:00",
    "likes": 8888
  }
]
```

##### 枚举筛选

枚举是多值的，所以要额外带一对中括号

`https://localhost:44396/api/blogs?title[$in][]=MVC&title[$in][]=Blazor`

```JSON
[
  {
    "id": 2,
    "title": "MVC",
    "createTime": "2020-04-01T00:00:00",
    "likes": 1024
  },
  {
    "id": 3,
    "title": "Blazor",
    "createTime": "2020-05-08T00:00:00",
    "likes": 6412
  }
]
```

除此上面这些之外还包括大于等于`$gte`、小于`$lt`、小于等于`$lte`、不等于`$ne`


##### AND筛选
筛选4月的博客

`https://localhost:44396/api/blogs?createTime[$gte]=2020-4-1&createTime[$lt]=2020-5-1`

```JSON
[
  {
    "id": 2,
    "title": "MVC",
    "createTime": "2020-04-01T00:00:00",
    "likes": 1024
  },
  {
    "id": 5,
    "title": "SignalR",
    "createTime": "2020-04-12T00:00:00",
    "likes": 102
  }
]
```

##### 空值筛选

`https://localhost:44396/api/blogs?title=`

key=value中value为空即表示为null，没有对空字符串的筛选，空字符串被认为是没有额外意义的，与null相同。

```JSON
[
  {
    "id": 7,
    "title": null,
    "createTime": "2020-03-26T00:00:00",
    "likes": 3
  }
]
```

不可为空的值会验证失败

`https://localhost:44396/api/blogs?id=`

```JSON
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "|2d44fb44-4ac839ff4e4fe16e.",
  "errors": {
    "id": [
      "Can not convert {null} to type Int32."
    ]
  }
}
```

##### 字段过滤

只提供部分可查询字段，自动忽略不可查询字段

```CSharp
[HttpGet("api/blogs")]
public ActionResult<IEnumerable<Blog>> Query([Bind("Id", "Title")]Query<Blog> query)
{
    //...
}
```
筛选Id

`https://localhost:44396/api/blogs?id=1`

```JSON
[
  {
    "id": 1,
    "title": "IOC-DI",
    "createTime": "2020-03-26T00:00:00",
    "likes": null
  }
]
```
筛选Likes

`https://localhost:44396/api/blogs?likes=1024`

```JSON
[
  {
    "id": 1,
    "title": "IOC-DI",
    "createTime": "2020-03-26T00:00:00",
    "likes": null
  },
  {
    "id": 2,
    "title": "MVC",
    "createTime": "2020-04-01T00:00:00",
    "likes": 1024
  },
  {
    "id": 3,
    "title": "Blazor",
    "createTime": "2020-05-08T00:00:00",
    "likes": 6412
  },
  {
    "id": 4,
    "title": "Web Api",
    "createTime": "2020-03-02T00:00:00",
    "likes": 8888
  },
  {
    "id": 5,
    "title": "SignalR",
    "createTime": "2020-04-12T00:00:00",
    "likes": 102
  },
  {
    "id": 6,
    "title": "gRPC",
    "createTime": "2020-06-30T00:00:00",
    "likes": 102
  },
  {
    "id": 7,
    "title": null,
    "createTime": "2020-03-26T00:00:00",
    "likes": 3
  }
]
```

#### 排序

`https://localhost:44396/api/blogs?$sort[likes]=-1`

根据likes倒序，1为正序，其他值非法。

```JSON
[
  {
    "id": 4,
    "title": "Web Api",
    "createTime": "2020-03-02T00:00:00",
    "likes": 8888
  },
  {
    "id": 3,
    "title": "Blazor",
    "createTime": "2020-05-08T00:00:00",
    "likes": 6412
  },
  {
    "id": 2,
    "title": "MVC",
    "createTime": "2020-04-01T00:00:00",
    "likes": 1024
  },
  {
    "id": 5,
    "title": "SignalR",
    "createTime": "2020-04-12T00:00:00",
    "likes": 102
  },
  {
    "id": 6,
    "title": "gRPC",
    "createTime": "2020-06-30T00:00:00",
    "likes": 102
  },
  {
    "id": 7,
    "title": null,
    "createTime": "2020-03-26T00:00:00",
    "likes": 3
  },
  {
    "id": 1,
    "title": "IOC-DI",
    "createTime": "2020-03-26T00:00:00",
    "likes": null
  }
]
```

#### 切片(分页)

`https://localhost:44396/api/blogs?$skip=3&$limit=2`

跳过3项，取2项

```JSON
[
  {
    "id": 4,
    "title": "Web Api",
    "createTime": "2020-03-02T00:00:00",
    "likes": 8888
  },
  {
    "id": 5,
    "title": "SignalR",
    "createTime": "2020-04-12T00:00:00",
    "likes": 102
  }
]
```


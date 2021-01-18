# MQuery

### 介绍
基于MongoDB查询语法的Http QueryString查询模式。

目前实现了`$eq`（等于）`$ne`（不等于）`$gt`（大于）`$gte`（大于等于）`$lt`（小于）`$lte`（小于等于）`$in`（包含于）`$nin`（不包含于）筛选操作符；`$sort`排序操作符；`$skip`、`$limit`分页操作符。

### 使用说明

#### 从ASP.NET Core中创建Query\<T>
```CSharp
// 在StartUp中
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers(options=> 
    {
        options.AddMQuery(o => 
        {
            o.DefaultLimit = 50; // 设置默认的分页条目数
            o.MaxLimit = 50; // 设置最大的分页条目数
        });
    });
}

// 在ApiController中
[HttpGet("api/blogs")]
public ActionResult<IEnumerable<Blog>> Query(Query<Blog> query)
{
    // ...
}
```
过滤属性

只提供部分可查询属性，自动忽略对不可查询属性的查询操作

```CSharp
[HttpGet("api/blogs")]
public ActionResult<IEnumerable<Blog>> Query([Bind("Id", "Title")]Query<Blog> query)
{
    //...
}
```
`AddMQuery`会在MvcOptions中添加Query\<T>的模型绑定，从而在HTTP请求中解析并创建实例。

#### 从Query字符串中创建Query\<T>
```CSharp
var query = new QueryParser<Foo>().Parse("name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
```
##### 语法规则
从JSON形式去看
```JSON
{
  "<propName1>": { // 对与一个属性
    "<compareOperator>": "<value1>", // 一个比较操作
    "<compareOperator>": "<value2>" // 另一个比较操作
    // ...
  },
  "<propName2>": { // 其他属性
    "<compareOperator>": "<value1>", // 一个比较操作
    "<compareOperator>": "<value2>" // 另一个比较操作
    // ...
  }, // 每一个属性与其操作都是and连接，暂不支持or查询
  "$sort":{
    "<propName>": 1 // 根据一个属性进行正序排序，-1则为倒序。可以指定多个，先指定的有更高优先级。
  },
  "$skip": 0, // 指定分页起始位置
  "$limit": 20 // 指定分页条目数
}
```
用QueryString表示
```
<propName1>[<compareOperator1>]=<value1>
&<propName2>[<compareOperator2>]=<value2>
&<propName1>[<compareOperator1>]=<value1>
&<propName2>[<compareOperator2>]=<value2>
&$sort[propName]=1
&$skip=0
&$limit=20
```


#### 使用Query\<T>来查询IQueryable<T>
```CSharp
var blogs = new List<Blog>
{
    new Blog {Id = 1, Title = "IOC-DI", CreateTime = new DateTime(2020, 3, 26), Likes = null},
    new Blog {Id = 2, Title = "MVC", CreateTime = new DateTime(2020, 4, 1), Likes = 1024},
    new Blog {Id = 3, Title = "Blazor", CreateTime = new DateTime(2020, 5, 8), Likes = 6412},
    new Blog {Id = 4, Title = "Web Api", CreateTime = new DateTime(2020, 3, 2), Likes = 8888},
    new Blog {Id = 5, Title = "SignalR", CreateTime = new DateTime(2020, 4, 12), Likes = 102},
    new Blog {Id = 6, Title = "gRPC", CreateTime = new DateTime(2020, 6, 30), Likes = 102},
    new Blog {Id = 7, Title = null, CreateTime = new DateTime(2020, 3, 26), Likes = 3},
}.AsQueryable();
```
以筛选、排序和分页的顺序执行所有查询
```CSharp
var result = query.ApplyTo(blogs);
```
单独进行筛选
```CSharp
var result = query.FilterTo(blogs);
```
单独进行排序
```CSharp
var result = query.SortTo(blogs);
```
单独进行分页
```CSharp
var result = query.SliceTo(blogs);
```

### 例子

#### 等于筛选

`/api/blogs?title=Web%20Api`

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

#### 大于筛选

`/api/blogs?likes[$gt]=500`

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

#### 包含于筛选

包含于是多值的，所以要额外带一对中括号

`/api/blogs?title[$in][]=MVC&title[$in][]=Blazor`

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

#### 多条件筛选
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

#### 空值筛选

`https://localhost:44396/api/blogs?title=`

key value对中value为空即表示为null，没有对空字符串的筛选，空字符串被认为是没有额外意义的，与null相同。

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

#### 分页

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


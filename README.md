# MQuery

## 使用 MQuery 你能得到什么？

使用 MQuery 只需要个位数的代码量，就能构建 ASP.NET Core WebApi 上的任意字段的简单查询，例如

原始数据

```JSON
[
    { "id": 1, "username": "Alice", "age": 18, "country": "USA" },
    { "id": 2, "username": "Bob", "age": 20, "country": "UK" },
    { "id": 3, "username": "Carl", "age": 47, "country": "Canada" },
    { "id": 4, "username": "Daniel", "age": 50, "country": "USA" },
    { "id": 5, "username": "Eva", "age": 35, "country": "Poland" },
    { "id": 6, "username": "Fiona", "age": null, "country": "Russia" },
]
```

查询 username 为“Alice”的数据

```
{your-api-path}?username=Alice
```

结果

```JSON
[
    { "id": 1, "username": "Alice", "age": 18, "country": "USA" },
]
```

☞ [具体语法](docs/syntax.md)

## 如何使用

### 在 ASP.NET Core WebApi 中使用MQuery

Startup.cs

```CSharp

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
```

BlogsController.cs

```CSharp
[HttpGet("api/blogs")]
public ActionResult<IEnumerable<Blog>> Query(Query<Blog> query)
{
    return query.ApplyTo(db.Blogs).ToList();
}
```

你也可以单独进行筛选、排序以及分页

单独进行筛选

```CSharp
query.FilterTo(blogs);
```

单独进行排序

```CSharp
query.SortTo(blogs);
```

单独进行分页

```CSharp
query.SliceTo(blogs);
```

### 从字符串中创建Query

```CSharp
var query = new QueryParser<Foo>().Parse("name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
```

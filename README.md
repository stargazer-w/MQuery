# MQuery

## 使用 MQuery 你能得到什么？

使用 MQuery 只需要个位数的代码行数，就能构建 ASP.NET Core WebApi 上的任意字段的简单查询接口，例如

原始数据

```JSON
[
  { "id": 1, "item": "journal", "qty": 25, "dim_cm":  [14, 21] },
  { "id": 2, "item": "notebook", "qty": 50, "dim_cm": [14, 21] },
  { "id": 3, "item": "paper", "qty": 100, "dim_cm": null },
  { "id": 4, "item": "planner", "qty": 75, "dim_cm": [22.85, 30] },
  { "id": 5, "item": "postcard", "qty": 45, "dim_cm": [10, 15.25] },
  { "id": 6, "item": "postcard", "qty": null, "dim_cm": [10, 15.25] },
]
```

查询`item`为`"journal"`的数据

```
?item=journal
```

结果

```JSON
[
  { "id": 1, "item": "journal", "qty": 25, "dim_cm":  [14, 21] },
]
```

☞ [具体语法](docs/syntax.md)

## 如何使用

### 安装

目前该包为自用，不能保证质量，所以未在nuget中公开。但可以直接使用命令行安装。

在ASP.NET Core中

`dotnet add package MQuery.AspNetCore`

在其他环境直接解析querystring字符串

`dotnet add package MQuery.QueryString`

### 在 ASP.NET Core WebApi 中使用

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

如此便完成了数据的筛选、排序、分页接口。你也可以单独进行其中一种操作：

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

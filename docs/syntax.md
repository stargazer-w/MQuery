# MQuery 语法

MQuery 借鉴了 MongoDB 的查询语法，并通过 querystring 与 JSON 的互相转化让其应用在 querystring 上。以下语法介绍与例子都会列出对应的 JSON 形式以便理解。

## 语法规则

querystring

```
<prop1>[<op1>]=<val1>
&<prop1>[<op2>]=<val2>
&<prop2>[<op1>]=<val1>
&<prop2>[<op2>]=<val2>
&$sort[<prop>]=1
&$skip=0
&$limit=20
```

JSON

```JSON
{
  "<prop1>": { // 对于一个属性
    "<op1>": "<val1>", // 一个比较操作
    "<op2>": "<val2>" // 另一个比较操作
  },
  "<prop2>": { // 其他属性
    "<op1>": "<val1>", // 一个比较操作
    "<op2>": "<val2>" // 另一个比较操作
  },
  "$sort":{
    "<prop>": 1 // 根据一个属性进行正序排序
  },
  "$skip": 0, // 指定分页起始位置
  "$limit": 20 // 指定分页条目数
}
```

其中`<prop>`对应要查询元素的属性名，支持嵌套 object 的查询，使用点表示法，如：`rootProp.nestedProp`。

而`<op>`表示几个比较操作符，目前实现的操作符有`$eq`（等于，可省略）、`$ne`（不等于）、`$gt`（大于）、`$gte`（大于等于）、`$lt`（小于）、`$lte`（小于等于）、`$in`（包含于）以及`$nin`（不包含于）。

`$sort`是排序操作，根据指定的`<prop>`与排序值(1 正序,-1 倒序)进行排序，支持多个属性排序，第一个属性为第一优先级。

`$skip`和`$limit`这对为切片（分页）操作符，`$skip`指示跳过几项元素，`$limit`指示要获取的元素数量。

每一个属性与其操作之间都是 and 连接，不支持 or 查询，所有操作符均为可选。

## 例子

样例数据

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

#### 等于筛选

查询 username 为“Alice”的数据

```
?username=Alice
```

```JSON
{ "username": "Alice" }
```

或

```
?username[$eq]=Alice
```

```JSON
{ "username": { "$eq": "Alice" } }
```

结果

```JSON
[
    { "id": 1, "username": "Alice", "age": 18, "country": "USA" },
]
```

#### 大于筛选

查询 age 大于 35 数据

```
?age[$gt]=35
```

```JSON
{ "username": { "$gt": 35 } }
```

结果

```JSON
[
    { "id": 3, "username": "Carl", "age": 47, "country": "Canada" },
    { "id": 4, "username": "Daniel", "age": 50, "country": "USA" },
]
```

#### 包含于筛选

查询国家为 USA 或 UK，多值可以重复传值，也可以带上`[]`或索引

```
?country[$in]=USA&country[$in]=UK
```

```JSON
{ "country": { "$in": [ "USA", "UK" ] } }
```

结果

```JSON
[
    { "id": 1, "username": "Alice", "age": 18, "country": "USA" },
    { "id": 2, "username": "Bob", "age": 20, "country": "UK" },
    { "id": 4, "username": "Daniel", "age": 50, "country": "USA" },
]
```

#### 多条件筛选

查询国家为 USA 且 age 大于 20

```
?country=USA&age[$gt]]=20
```

```JSON
{ "country": "USA", "age": { "$gt": 20 } } }
```

结果

```JSON
[
    { "id": 4, "username": "Daniel", "age": 50, "country": "USA" },
]
```

#### 空值筛选

key value 对中 value 为空即表示为 null，没有对空字符串的筛选，空字符串被认为是没有额外意义的，与 null 相同。

筛选年龄为 null

```
?age=
```

```JSON
{ "age": null }
```

结果

```JSON
[
    { "id": 5, "username": "Fiona", "age": null, "country": "Russia" },
]
```

不可为空的值会验证失败

```
?id=
```

```JSON
{ "id": null }
```

结果

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

根据 age 正序

```
?$sort[age]=1
```

```JSON
{ "$sort": { "age": 1 } }
```

结果[^1]

```JSON
[
    { "id": 6, "username": "Fiona", "age": null, "country": "Russia" },
    { "id": 1, "username": "Alice", "age": 18, "country": "USA" },
    { "id": 2, "username": "Bob", "age": 20, "country": "UK" },
    { "id": 5, "username": "Eva", "age": 35, "country": "Poland" },
    { "id": 3, "username": "Carl", "age": 47, "country": "Canada" },
    { "id": 4, "username": "Daniel", "age": 50, "country": "USA" },
]
```

#### 分页

跳过 3 项，取 2 项

```
?$skip=3&$limit=2
```

```JSON
{ "$skip": 3, "$limit":2 }
```

结果

```JSON
[
    { "id": 4, "username": "Daniel", "age": 50, "country": "USA" },
    { "id": 5, "username": "Eva", "age": 35, "country": "Poland" },
]
```

[^1]: null 值的实际顺序未必与文档中给出的相同

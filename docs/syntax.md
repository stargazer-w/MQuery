# MQuery 语法

MQuery 借鉴了 MongoDB 的查询语法（这也是 MQuery 名字的由来），并通过 querystring 与 JSON 的互相转化让其应用在 querystring 上。以下语法介绍与例子都会列出对应的 JSON 形式以便理解。

## 语法规则

大体语法结构

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

```JSONC
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

`<prop>`是对应查询属性的选择器。支持使用点表示法查询嵌套object属性，如：`rootProp.nestedProp`。

`<op>`表示操作符。操作符分为比较操作符、逻辑操作符以及集合操作符。

1. 比较操作符：
   - `$eq` 等于，可省略
   - `$ne` 不等于
   - `$gt` 大于
   - `$gte` 大于等于
   - `$lt` 小于
   - `$lte` 小于等于
   - `$in` 包含于
   - `$nin` 不包含于
2. 逻辑操作符
   - `$not` 非
3. 集合操作符
   - `$any` 任意元素匹配，可省略。仅当筛选集合`$eq`或`$nin` `null`时，为避免歧义不可省略。
   - `$all` 所有元素匹配

`$sort`对象是排序操作。根据指定的`<prop>`与排序值`<val>`（1：正序；-1：倒序）进行排序。支持多个属性排序，第一个属性为第一优先级。

`$skip`和`$limit`这对为切片（分页）操作符，`$skip`指示跳过几项元素，`$limit`指示要获取的元素数量。

每一个属性与其操作之间都是 and 连接，不支持 or 查询，所有操作符均为可选。

## 例子

样例数据

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

### 筛选

#### 等于筛选

查询`item`为`"journal"`的数据

```
?item=journal
```

```JSON
{ "item": "journal" }
```

或

```
?item[$eq]=journal
```

```JSON
{ "item": { "$eq": "journal" } }
```

结果

```JSON
[
  { "id": 1, "item": "journal", "qty": 25, "dim_cm":  [14, 21] },
]
```

#### 大于筛选

查询`qty`大于`60`的数据

```
?qty[$gt]=60
```

```JSON
{ "qty": { "$gt": 60 } }
```

结果

```JSON
[
  { "id": 3, "item": "paper", "qty": 100, "dim_cm": null },
  { "id": 4, "item": "planner", "qty": 75, "dim_cm": [22.85, 30] },
]
```

#### 包含于筛选

查询`item`为`"notebook"`或`"paper"`的数据

```
?item[$in]=notebook&item[$in]=paper
```

```JSON
{ "item": { "$in": [ "notebook", "paper" ] } }
```

key也可以带上`[]`或索引

```
?item[$in][]=notebook&item[$in][]=paper
```

```
?item[$in][0]=notebook&item[$in][1]=paper
```

结果

```JSON
[
  { "id": 2, "item": "notebook", "qty": 50, "dim_cm": [14, 21] },
  { "id": 3, "item": "paper", "qty": 100, "dim_cm": null },
]
```

#### 多条件筛选

查询`item`为`"postcard"`且`qty`大于`20`的数据

```
?item=postcard&qty[$gt]=20
```

```JSON
{ "item": "postcard", "qty": { "$gt": 20 } }
```

结果

```JSON
[
  { "id": 5, "item": "postcard", "qty": 45, "dim_cm": [10, 15.25] },
]
```

#### 空值筛选

key-value对中，value为空即表示为`null`。没有对空字符串`""`的筛选，空字符串被认为是没有额外意义的，与`null`等价。

筛选`qty`为`null`的数据

```
?qty=
```

```JSON
{ "qty": null }
```

结果

```JSON
[
  { "id": 6, "item": "postcard", "qty": null, "dim_cm": [10, 15.25] },
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

### 集合任意元素匹配筛选

查询`dim_cm`中有任何一个元素为`22.85`的数据

```
?dim_cm[$any]=22.85
```

```JSON
{ "dim_cm": { "$any": 22.85 } }
```

或者

```
?dim_cm=22.85
```

```JSON
{ "dim_cm": 22.85 }
```

结果

```JSON
[
  { "id": 4, "item": "planner", "qty": 75, "dim_cm": [22.85, 30] },
]
```

#### 集合所有元素匹配筛选

查询`dim_cm`中有所有元素都小于`20`的数据

```
?dim_cm[$all][$lt]=20
```

```JSON
{ "dim_cm": { "$all": { "$lt":20 } } }
```

结果

```JSON
[
  { "id": 5, "item": "postcard", "qty": 45, "dim_cm": [10, 15.25] },
  { "id": 6, "item": "postcard", "qty": null, "dim_cm": [10, 15.25] },
]
```

#### 集合的空值筛选

集合属性可以直接比较而非使用其元素比较的操作只有`$eq` `null`和`$ne` `null`。所以特殊地，当集合属性与`null`进行`$eq`或`$ne`比较时，不会默认使用`$any`。

```
?dim_cm=null
```

```JSON
{ "dim_cm": null }
```

结果

```JSON
[
  { "id": 3, "item": "paper", "qty": 100, "dim_cm": null },
]
```

### 排序

根据 qty 正序

```
?$sort[qty]=1
```

```JSON
{ "$sort": { "qty": 1 } }
```

结果[^1]

```JSON
[
  { "id": 6, "item": "postcard", "qty": null, "dim_cm": [10, 15.25] },
  { "id": 1, "item": "journal", "qty": 25, "dim_cm":  [14, 21] },
  { "id": 5, "item": "postcard", "qty": 45, "dim_cm": [10, 15.25] },
  { "id": 2, "item": "notebook", "qty": 50, "dim_cm": [14, 21] },
  { "id": 4, "item": "planner", "qty": 75, "dim_cm": [22.85, 30] },
  { "id": 3, "item": "paper", "qty": 100, "dim_cm": null },
]
```

### 分页

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
  { "id": 4, "item": "planner", "qty": 75, "dim_cm": [22.85, 30] },
  { "id": 5, "item": "postcard", "qty": 45, "dim_cm": [10, 15.25] },
]
```

[^1]: null 值的实际顺序未必与文档中给出的相同

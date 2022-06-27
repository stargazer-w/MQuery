using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MQuery.NSwag
{
    class MQueryOpenApiDocumentProcessor : IOperationProcessor
    {
        readonly Dictionary<JsonSchema, JsonSchema> _compareSchemas = new();

        static readonly JsonSchemaProperty _sortingPropSchema = new()
        {
            Type = JsonObjectType.Integer,
            Format = JsonFormatStrings.Integer,
            Description = "根据key值所指定的属性排序。value值为正时正序，为负时倒序，0则忽略。",
        };

        static readonly OpenApiParameter _skipParam = new()
        {
            Name = "$skip",
            Kind = OpenApiParameterKind.Query,
            IsRequired = false,
            Schema = new()
            {
                Type = JsonObjectType.Integer,
                Format = JsonFormatStrings.Integer,
            },
            Description = "跳过几项",
        };

        static readonly OpenApiParameter _limitParam = new()
        {
            Name = "$limit",
            Kind = OpenApiParameterKind.Query,
            IsRequired = false,
            Schema = new()
            {
                Type = JsonObjectType.Integer,
                Format = JsonFormatStrings.Integer,
            },
            Description = "获取几项",
        };

        bool IOperationProcessor.Process(OperationProcessorContext context)
        {
            foreach(var (info, param) in context.Parameters)
            {
                var type = info.ParameterType;
                if(!type.IsGenericType)
                    continue;

                var genDef = type.GetGenericTypeDefinition();
                if(genDef == typeof(Query<>))
                    throw new NotSupportedException("如果要使MQuery支持NSwag，形参类型请使用MQuery.IQuery<T>");

                if(genDef != typeof(IQuery<>))
                    continue;

                var parameters = context.OperationDescription.Operation.Parameters;
                parameters.Remove(param);
                var originSchema = context.SchemaResolver.GetSchema(type, false);
                var componentSchema = context.Document.Components.Schemas
                    .FirstOrDefault(x => x.Value == originSchema);
                if(componentSchema.Key is string key)
                    context.Document.Components.Schemas.Remove(key);

                var modelType = type.GetGenericArguments()[0];
                var modelTypeSchema = context.SchemaResolver.HasSchema(modelType, false)
                    ? context.SchemaResolver.GetSchema(modelType, false)
                    : context.SchemaGenerator.Generate(modelType, context.SchemaResolver);

                var sortingSchema = new JsonSchema();
                foreach(var (propSelector, propSchema) in GetSchemaPropertySelectors(modelTypeSchema))
                {
                    parameters.Add(new()
                    {
                        Name = propSelector,
                        Kind = OpenApiParameterKind.Query,
                        Style = OpenApiParameterStyle.DeepObject,
                        Schema = GetCompareSchema(propSchema),
                    });
                    sortingSchema.Properties.Add(propSelector, new JsonSchemaProperty { Reference = _sortingPropSchema });
                }

                parameters.Add(new()
                {
                    Name = "$sort",
                    Kind = OpenApiParameterKind.Query,
                    Style = OpenApiParameterStyle.DeepObject,
                    Schema = sortingSchema,
                });

                parameters.Add(_skipParam);
                parameters.Add(_limitParam);
            }

            return true;
        }

        public void OpenApiDocumentPostProcess(OpenApiDocument doc)
        {
            doc.Components.Schemas.Add("MQuery.Sorting.SortingProp", _sortingPropSchema);
        }

        JsonSchema GetCompareSchema(JsonSchema schema)
        {
            if(!_compareSchemas.TryGetValue(schema, out var compareSchema))
            {
                _compareSchemas.Add(schema, compareSchema = generateCompareSchema(schema));
            }
            return compareSchema;

            static JsonSchema generateCompareSchema(JsonSchema schema)
            {
                var prop = new JsonSchemaProperty
                {
                    Reference = schema,
                };
                var arrayProp = new JsonSchemaProperty { Type = JsonObjectType.Array, Item = prop };

                var equalsSchema = new JsonSchema() { Properties = { ["$eq"] = prop } };

                var inSchema = new JsonSchema() { Properties = { ["$in"] = arrayProp } };

                var lessThenSchema = new JsonSchema()
                {
                    OneOf =
                    {
                        new() { Properties = { ["$lt"] = prop } },
                        new() { Properties = { ["$lte"] = prop } },
                    }
                };

                var greaterThenSchema = new JsonSchema()
                {
                    OneOf =
                    {
                        new() { Properties = { ["$gt"] = prop } },
                        new() { Properties = { ["$gte"] = prop } },
                    }
                };

                var notEqualsSchema = new JsonSchema()
                {
                    OneOf =
                    {
                        new() { Properties = { ["$ne"] = prop } },
                        new() { Properties = { ["$nin"] = arrayProp } },
                    }
                };

                return new JsonSchema()
                {
                    OneOf =
                    {
                        prop,
                        equalsSchema,
                        inSchema,
                        new ()
                        {
                            AnyOf =
                            {
                                lessThenSchema,
                                greaterThenSchema,
                                notEqualsSchema,
                            }
                        },
                    }
                };
            }
        }

        static IEnumerable<(string name, JsonSchema schema)> GetSchemaPropertySelectors(JsonSchema schema)
        {
            foreach(var (propName, propSchema) in schema.ActualProperties)
            {
                switch(propSchema)
                {
                    case { ActualProperties.Count: > 0 }:
                        foreach(var (nestedPropName, nestedPropSchema) in GetSchemaPropertySelectors(propSchema))
                        {
                            yield return (propName + "." + nestedPropName, nestedPropSchema);
                        }
                        break;
                    default:
                        yield return (propName, propSchema);
                        break;
                }
            }
        }

    }
}
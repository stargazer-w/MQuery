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
            Description = "Specify in the sort parameter the field or fields to sort by and a value of 1 or -1 to specify an ascending or descending sort respectively.",
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
            Description = "Skips over the specified number of items",
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
            Description = "Limits the number of items",
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
                    Description = "Specifies the order in which the query returns.",
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
                var shortEqualsSchema = new JsonSchemaProperty
                {
                    Reference = schema,
                    Description = "Matches values that are equal to a specified value.",
                };

                var equalsSchema = new JsonSchema()
                {
                    Properties =
                    {
                        ["$eq"] = new JsonSchemaProperty
                        {
                            Reference = schema,
                            Description = "Matches values that are equal to a specified value. Equivalent to using the field=<value>",
                        },
                    },
                };

                var inSchema = new JsonSchema()
                {
                    Properties =
                    {
                        ["$in"] = new JsonSchemaProperty
                        {
                            Type = JsonObjectType.Array,
                            Item = new JsonSchemaProperty { Reference = schema },
                            Description = "Matches any of the values specified in an array.",
                        }
                    }
                };

                var lessThenSchema = new JsonSchema()
                {
                    OneOf =
                    {
                        new()
                        {
                            Properties =
                            {
                                ["$lt"] =  new JsonSchemaProperty
                                {
                                    Reference = schema,
                                    Description = "Matches values that are less than a specified value.",
                                },
                            }
                        },
                        new()
                        {
                            Properties =
                            {
                                ["$lte"] = new JsonSchemaProperty
                                {
                                    Reference = schema,
                                    Description = "Matches values that are less than or equal to a specified value.",
                                },
                            }
                        },
                    }
                };

                var greaterThenSchema = new JsonSchema()
                {
                    OneOf =
                    {
                        new()
                        {
                            Properties =
                            {
                                ["$gt"] = new JsonSchemaProperty
                                {
                                    Reference = schema,
                                    Description = "Matches values that are greater than a specified value.",
                                },
                            }
                        },
                        new()
                        {
                            Properties =
                            {
                                ["$gte"] = new JsonSchemaProperty
                                {
                                    Reference = schema,
                                    Description = "Matches values that are greater than or equal to a specified value."
                                }
                            }
                        },
                    }
                };

                var notEqualsSchema = new JsonSchema()
                {
                    OneOf =
                    {
                        new()
                        {
                            Properties =
                            {
                                ["$ne"] = new JsonSchemaProperty
                                {
                                    Reference = schema,
                                    Description = "Matches all values that are not equal to a specified value."
                                }
                            }
                        },
                        new()
                        {
                            Properties =
                            {
                                ["$nin"] = new JsonSchemaProperty
                                {
                                    Type = JsonObjectType.Array,
                                    Item = new JsonSchemaProperty { Reference = schema },
                                    Description = "Matches none of the values specified in an array."
                                }
                            }
                        },
                    }
                };

                return new JsonSchema()
                {
                    OneOf =
                    {
                        shortEqualsSchema,
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
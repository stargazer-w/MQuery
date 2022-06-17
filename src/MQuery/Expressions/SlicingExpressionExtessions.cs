﻿using MQuery.Slicing;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Expressions
{
    public static class SlicingExpressionExtessions
    {
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression<T>(this SlicingDocument slicing)
        {
            return slicing switch
            {
                null => throw new ArgumentNullException(nameof(slicing)),
                { Skip: null, Limit: null } => qy => qy,
                { Skip: null, Limit: int limit } => qy => qy.Take(limit),
                { Skip: int skip, Limit: null } => qy => qy.Skip(skip),
                { Skip: int skip, Limit: int limit } => qy => qy.Skip(skip).Take(limit),
            };
        }
    }
}

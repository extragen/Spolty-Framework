using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace Spolty.Framework.Designers
{
    public interface IEnumeratorProvider
    {
        Type EntityType { get; }
        Expression Expression { get; }
        IQueryProvider QueryProvider { get; }
        IEnumerator GetEnumerator();
    }
}
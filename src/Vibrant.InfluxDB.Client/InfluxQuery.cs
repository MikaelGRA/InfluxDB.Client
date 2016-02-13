using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxQuery<TElement> : IQueryable<TElement>
   {
      private Expression _expression;
      private InfluxQueryProvider _provider;

      public InfluxQuery( InfluxQueryProvider provider )
      {
         _provider = provider;
         _expression = Expression.Constant( this );
      }

      public InfluxQuery( InfluxQueryProvider provider, Expression expression )
      {
         _provider = provider;
         _expression = expression;
      }

      public Type ElementType
      {
         get
         {
            return typeof( TElement );
         }
      }

      public Expression Expression
      {
         get
         {
            return _expression;
         }
      }

      public IQueryProvider Provider
      {
         get
         {
            return _provider;
         }
      }

      public IEnumerator<TElement> GetEnumerator()
      {
         return ( (IEnumerable<TElement>)_provider.Execute( _expression ) ).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return ( (IEnumerable)_provider.Execute( _expression ) ).GetEnumerator();
      }

      public override string ToString()
      {
         return QueryText;
      }

      public string QueryText
      {
         get
         {
            return _provider.GetQueryText( _expression );
         }
      }
   }
}

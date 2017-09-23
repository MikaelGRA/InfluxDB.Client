//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.Linq;

//namespace Vibrant.InfluxDB.Client.Helpers
//{
//   /// <summary>
//   /// Rewrites an expression tree so that locally isolatable sub-expressions are evaluated and converted into ConstantExpression nodes.
//   /// </summary>
//   internal static class PartialEvaluator
//   {
//      /// <summary>
//      /// Performs evaluation and replacement of independent sub-trees
//      /// </summary>
//      /// <param name="expression">The root of the expression tree.</param>
//      /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
//      public static Expression Eval( Expression expression )
//      {
//         return Eval( expression, null );
//      }

//      /// <summary>
//      /// Performs evaluation and replacement of independent sub-trees
//      /// </summary>
//      /// <param name="expression">The root of the expression tree.</param>
//      /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
//      /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
//      public static Expression Eval( Expression expression, Func<Expression, bool> fnCanBeEvaluated )
//      {
//         if( fnCanBeEvaluated == null )
//            fnCanBeEvaluated = PartialEvaluator.CanBeEvaluatedLocally;
//         return SubtreeEvaluator.Eval( Nominator.Nominate( fnCanBeEvaluated, expression ), expression );
//      }

//      private static bool CanBeEvaluatedLocally( Expression expression )
//      {
//         return expression.NodeType != ExpressionType.Parameter;
//      }

//      /// <summary>
//      /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
//      /// </summary>
//      class SubtreeEvaluator : ExpressionVisitor
//      {
//         HashSet<Expression> candidates;

//         private SubtreeEvaluator( HashSet<Expression> candidates )
//         {
//            this.candidates = candidates;
//         }

//         internal static Expression Eval( HashSet<Expression> candidates, Expression exp )
//         {
//            return new SubtreeEvaluator( candidates ).Visit( exp );
//         }

//         public override Expression Visit( Expression exp )
//         {
//            if( exp == null )
//            {
//               return null;
//            }

//            if( exp is MemberInitExpression )
//            {
//               return exp;
//            }

//            if( this.candidates.Contains( exp ) )
//            {
//               return this.Evaluate( exp );
//            }

//            return base.Visit( exp );
//         }

//         private Expression Evaluate( Expression e )
//         {
//            Type type = e.Type;

//            // check for nullable converts & strip them
//            if( e.NodeType == ExpressionType.Convert )
//            {
//               var u = (UnaryExpression)e;
//               if( TypeHelper.GetNonNullableType( u.Operand.Type ) == TypeHelper.GetNonNullableType( type ) )
//               {
//                  e = ( (UnaryExpression)e ).Operand;
//               }
//            }

//            // if we now just have a constant, return it
//            if( e.NodeType == ExpressionType.Constant )
//            {
//               var ce = (ConstantExpression)e;

//               // if we've lost our nullable typeness add it back
//               if( e.Type != type && TypeHelper.GetNonNullableType( e.Type ) == TypeHelper.GetNonNullableType( type ) )
//               {
//                  e = ce = Expression.Constant( ce.Value, type );
//               }

//               return e;
//            }

//            var me = e as MemberExpression;
//            if( me != null )
//            {
//               // member accesses off of constant's are common, and yet since these partial evals
//               // are never re-used, using reflection to access the member is faster than compiling  
//               // and invoking a lambda
//               var ce = me.Expression as ConstantExpression;
//               if( ce != null )
//               {
//                  return Expression.Constant( me.Member.GetValue( ce.Value ), type );
//               }
//            }

//            if( type.GetTypeInfo().IsValueType )
//            {
//               e = Expression.Convert( e, typeof( object ) );
//            }

//            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>( e );
//            Func<object> fn = lambda.Compile();
//            return Expression.Constant( fn(), type );
//         }
//      }

//      /// <summary>
//      /// Performs bottom-up analysis to determine which nodes can possibly
//      /// be part of an evaluated sub-tree.
//      /// </summary>
//      class Nominator : ExpressionVisitor
//      {
//         Func<Expression, bool> fnCanBeEvaluated;
//         HashSet<Expression> candidates;
//         bool cannotBeEvaluated;

//         private Nominator( Func<Expression, bool> fnCanBeEvaluated )
//         {
//            this.candidates = new HashSet<Expression>();
//            this.fnCanBeEvaluated = fnCanBeEvaluated;
//         }

//         internal static HashSet<Expression> Nominate( Func<Expression, bool> fnCanBeEvaluated, Expression expression )
//         {
//            Nominator nominator = new Nominator( fnCanBeEvaluated );
//            nominator.Visit( expression );
//            return nominator.candidates;
//         }

//         protected override Expression VisitConstant( ConstantExpression c )
//         {
//            return base.VisitConstant( c );
//         }

//         public override Expression Visit( Expression expression )
//         {
//            if( expression != null )
//            {
//               bool saveCannotBeEvaluated = this.cannotBeEvaluated;
//               this.cannotBeEvaluated = false;
//               base.Visit( expression );
//               if( !this.cannotBeEvaluated )
//               {
//                  if( this.fnCanBeEvaluated( expression ) )
//                  {
//                     this.candidates.Add( expression );
//                  }
//                  else
//                  {
//                     this.cannotBeEvaluated = true;
//                  }
//               }
//               this.cannotBeEvaluated |= saveCannotBeEvaluated;
//            }
//            return expression;
//         }
//      }
//   }
//}

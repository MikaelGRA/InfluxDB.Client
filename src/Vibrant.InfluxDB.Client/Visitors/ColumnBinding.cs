using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class ColumnBinding
   {
      public ColumnBinding( Expression sourceExpression, MemberInfo targetMember, ColumnBinding innerColumnBinding )
      {
         SourceExpression = sourceExpression;
         TargetMember = targetMember;
         InnerBinding = innerColumnBinding;
         OriginalSourceMember = innerColumnBinding.OriginalSourceMember;
      }

      public ColumnBinding( Expression sourceExpression, MemberInfo targetMember, MemberInfo originalSourceMember )
      {
         SourceExpression = sourceExpression;
         TargetMember = targetMember;
         OriginalSourceMember = originalSourceMember;
      }

      public Expression SourceExpression { get; private set; }

      public MemberInfo TargetMember { get; private set; }

      public ColumnBinding InnerBinding { get; private set; }

      public MemberInfo OriginalSourceMember { get; private set; }
   }
}

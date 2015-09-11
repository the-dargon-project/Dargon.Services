using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Dargon.Services.Utilities;

namespace Dargon.Services {
   public class AsyncServiceInvokerImpl : AsyncServiceInvoker {
      public const bool kDebugEnabled = false;

      public static object GetExpressionResult(Expression expression) {
         return Expression.Lambda(expression).Compile().DynamicInvoke();
      }

      public async Task<object> EvaluateAsync(Expression expression) {
         DebugWriteLine("At expression: " + expression);
         if (expression is ConstantExpression) {
            return ((ConstantExpression)expression).Value;
         } else if (expression is MethodCallExpression) {
            var methodCallExpression = expression as MethodCallExpression;
            var methodObjectExpression = methodCallExpression.Object;
            var methodArgumentExpressions = methodCallExpression.Arguments;
            var methodArguments = new object[methodArgumentExpressions.Count];
            for (var i = 0; i < methodArgumentExpressions.Count; i++) {
               var methodArgumentExpression = methodArgumentExpressions[i];
               DebugWriteLine($"{i}: {methodArgumentExpression.NodeType}, {methodArgumentExpression.Type}");
               methodArguments[i] = await EvaluateAsync(methodArgumentExpression);
            }
            object invocationReturnValue = null;
            if (methodObjectExpression == null) {
               var method = methodCallExpression.Method;
               invocationReturnValue = method.Invoke(null, methodArguments);
            } else {
               var methodObject = await EvaluateAsync(methodCallExpression.Object);

               IAsyncInterceptor asyncInterceptor;
               if (AsyncInterceptorUtilities.TryGetAsyncInterceptor(methodObject, out asyncInterceptor)) {
                  DebugWriteLine($"!> Asynchronously invoking service method {methodCallExpression.Method.Name}!");
                  invocationReturnValue = await asyncInterceptor.InterceptAsync(methodCallExpression.Method, methodArguments);
               } else {
                  var method = methodCallExpression.Method;
                  invocationReturnValue = method.Invoke(methodObject, methodArguments);
               }
            }
            var methodParameters = methodCallExpression.Method.GetParameters();
            for (var i = 0; i < methodParameters.Length; i++) {
               var methodParameter = methodParameters[i];
               if (methodParameter.IsOut || methodParameter.ParameterType.IsByRef) {
                  await SetOutOrRef(methodArgumentExpressions[i], methodArguments[i]);
               }
            }
            return invocationReturnValue;
         } else if (expression is MemberExpression) {
            var memberExpression = (MemberExpression)expression;
            switch (memberExpression.Member.MemberType) {
               case MemberTypes.Field:
                  var objectExpression = Expression.Convert(memberExpression, typeof(object));
                  var getterExpression = Expression.Lambda<Func<object>>(objectExpression);
                  var memberValue = getterExpression.Compile().Invoke();
                  DebugWriteLine("!! member: " + memberValue + " " + memberExpression.NodeType + " " + memberExpression.Member.Name + " " + memberExpression.Member.MemberType);
                  return memberValue;
               case MemberTypes.Property:
                  var property = ((PropertyInfo)memberExpression.Member);
                  var getterMethod = property.GetGetMethod();
                  var thisExpression = await EvaluateAsync(memberExpression.Expression);
                  DebugWriteLine("!! property: " + getterMethod + " " + memberExpression.NodeType + " " + memberExpression.Member.Name + " " + memberExpression.Member.MemberType);
                  return await EvaluateAsync(Expression.Call(Expression.Constant(thisExpression), getterMethod));
               default:
                  DebugWriteLine(memberExpression.ToString());
                  Console.WriteLine(memberExpression.NodeType + " " + memberExpression.GetType() + " " + memberExpression.CanReduce);
                  throw new NotImplementedException();
            }
         } else if (expression is UnaryExpression) {
            var unaryExpression = (UnaryExpression)expression;
            var operand = await EvaluateAsync(unaryExpression.Operand);
            switch (unaryExpression.NodeType) {
               case ExpressionType.Convert:
                  return Convert.ChangeType(operand, unaryExpression.Type);
               default:
                  DebugWriteLine(unaryExpression.ToString());
                  Console.WriteLine(unaryExpression.NodeType + " " + unaryExpression.GetType() + " " + unaryExpression.CanReduce);
                  throw new NotImplementedException();
            }
         } else if (expression is BinaryExpression) {
            var binaryExpression = (BinaryExpression)expression;
            var left = await EvaluateAsync(binaryExpression.Left);
            var right = await EvaluateAsync(binaryExpression.Right);
            switch (binaryExpression.NodeType) {
               case ExpressionType.ArrayIndex:
                  var indexingExpression = Expression.ArrayIndex(Expression.Constant(left), Expression.Constant(right));
                  return GetExpressionResult(indexingExpression);
               case ExpressionType.Add:
               case ExpressionType.Subtract:
               case ExpressionType.Multiply:
               case ExpressionType.Divide:
                  return GetExpressionResult(Expression.MakeBinary(binaryExpression.NodeType, Expression.Constant(left), Expression.Constant(right)));
               default:
                  DebugWriteLine(binaryExpression.ToString());
                  Console.WriteLine(binaryExpression.NodeType + " " + binaryExpression.GetType() + " " + binaryExpression.CanReduce);
                  throw new NotImplementedException();
            }
         } else {
            Console.WriteLine(expression.NodeType + " " + expression.GetType() + " " + expression.CanReduce);
            throw new NotImplementedException();
         }
      }

      private async Task SetOutOrRef(Expression argumentExpression, object value) {
         DebugWriteLine($"SetOutOrRef: {argumentExpression} ({argumentExpression.Type} {argumentExpression.NodeType}) to {value}.");
         if (!(argumentExpression is MemberExpression)) {
            throw new InvalidOperationException("Expected MemberExpression from " + argumentExpression + " to " + value);
         }
         var memberExpression = (MemberExpression)argumentExpression;
         var thisObject = await EvaluateAsync(memberExpression.Expression);
         var member = memberExpression.Member;
         if (member.MemberType != MemberTypes.Field) {
            throw new InvalidOperationException("Expected field member when for expression " + argumentExpression + " setting to " + value);
         }

         var fieldMember = (FieldInfo)member;
         fieldMember.SetValue(thisObject, value);
      }

      private static void DebugWriteLine(string output) {
#pragma warning disable 162
         if (kDebugEnabled) {
            Debug.WriteLine(output);
         }
#pragma warning restore 162
      }
   }
}

﻿using ItzWarty;
using ItzWarty.Collections;
using System;
using System.Reflection;

namespace Dargon.Services.Client {
   public interface RemoteServiceInvocationValidator {
      void ValidateInvocationOrThrow(string methodName, object[] methodArguments);
   }

   public class RemoteServiceInvocationValidatorImpl : RemoteServiceInvocationValidator {
      private readonly Type serviceInterface;
      private readonly Guid serviceGuid;
      private readonly IMultiValueDictionary<string, MethodInfo> methodsByName;

      public RemoteServiceInvocationValidatorImpl(Type serviceInterface, Guid serviceGuid, IMultiValueDictionary<string, MethodInfo> methodsByName) {
         this.serviceInterface = serviceInterface;
         this.serviceGuid = serviceGuid;
         this.methodsByName = methodsByName;
      }

      public void ValidateInvocationOrThrow(string methodName, object[] methodArguments) {
         HashSet<MethodInfo> candidates;
         if (methodsByName.TryGetValue(methodName, out candidates)) {
            foreach (var candidate in candidates) {
               var parameters = candidate.GetParameters();
               if (parameters.Length != methodArguments.Length) {
                  continue;
               }
               return;
            }
         }
         throw new InvalidOperationException("Invocation validation failed - could not find Method `{0}` for {1} arguments in interface {2}".F(methodName, methodArguments.Length, serviceInterface.FullName));
      }
   }
}
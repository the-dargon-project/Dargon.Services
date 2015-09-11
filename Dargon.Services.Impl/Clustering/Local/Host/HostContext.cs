using System;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using ItzWarty.Collections;
using NLog;

namespace Dargon.Services.Clustering.Local.Host {
   public interface HostContext : IDisposable {
      Task<object> Invoke(Guid serviceGuid, string methodName, Type[] genericArguments, object[] methodArguments);
      Task<object> Invoke(Guid serviceGuid, string methodName, PortableObjectBox genericArguments, PortableObjectBox methodArgumentsDto);
      void AddRemoteInvokable(RemoteInvokable remoteInvokable);
      void RemoveRemoteInvokable(RemoteInvokable remoteInvokable);
   }

   public interface RemoteInvokable {
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, Type[] genericArguments, object[] arguments);
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, PortableObjectBox genericArguments, PortableObjectBox argumentsDto);
   }

   public class HostContextImpl : HostContext {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly PortableObjectBoxConverter portableObjectBoxConverter;
      private readonly LocalServiceContainer localServiceContainer;
      private readonly IConcurrentSet<RemoteInvokable> remoteInvokables;

      public IConcurrentSet<RemoteInvokable> RemoteInvokables => remoteInvokables;

      public HostContextImpl(
         PortableObjectBoxConverter portableObjectBoxConverter, 
         LocalServiceContainer localServiceContainer
      ) : this(
         portableObjectBoxConverter, 
         localServiceContainer, 
         new ConcurrentSet<RemoteInvokable>()) {}

      public HostContextImpl(PortableObjectBoxConverter portableObjectBoxConverter, LocalServiceContainer localServiceContainer, IConcurrentSet<RemoteInvokable> remoteInvokables) {
         this.portableObjectBoxConverter = portableObjectBoxConverter;
         this.localServiceContainer = localServiceContainer;
         this.remoteInvokables = remoteInvokables;
      }

      public async Task<object> Invoke(Guid serviceGuid, string methodName, Type[] genericArguments, object[] methodArguments) {
         logger.Trace($"Invoke called for service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {methodArguments.Length} arguments.");
         object result;
         try {
            if (localServiceContainer.TryInvoke(serviceGuid, methodName, genericArguments, methodArguments, out result)) {
               logger.Trace($"LocalServiceContainer successfully invoked service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {methodArguments.Length} arguments.");
            } else {
               logger.Trace($"LocalServiceContainer failed to invoke service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {methodArguments.Length} arguments.");
               bool invocationSuccessful = false;
               foreach (var remoteInvokable in remoteInvokables) {
                  var invocation = await remoteInvokable.TryRemoteInvoke(serviceGuid, methodName, genericArguments, methodArguments);
                  if (invocation.Success) {
                     result = invocation.ReturnValue;
                     invocationSuccessful = true;
                     break;
                  }
               }
               if (!invocationSuccessful) {
                  logger.Trace($"All remote invokables failed to invoke service {serviceGuid} method {methodName} with{genericArguments.Length} generic arguments and {methodArguments.Length} arguments.");
                  throw new ServiceUnavailableException(serviceGuid, methodName);
               }
            }
            logger.Trace($"Successfully invoked service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {methodArguments.Length} arguments!");
         } catch (Exception e) {
            logger.Trace($"Invocation of service {serviceGuid} method {methodName} with {genericArguments.Length} generic arguments and {methodArguments.Length} arguments, {e}");
            if (e is IPortableObject) {
               result = e;
            } else {
               result = new PortableException(e);
            }
         }
         return result;
      }

      public async Task<object> Invoke(Guid serviceGuid, string methodName, PortableObjectBox genericArgumentsDto, PortableObjectBox methodArgumentsDto) {
         logger.Trace($"Invoke called for service {serviceGuid} method {methodName} with {methodArgumentsDto.Length} bytes of arguments.");
         object result = null;
         try {
            Type[] genericArguments;
            object[] methodArguments;
            if (portableObjectBoxConverter.TryConvertFromDataTransferObject(genericArgumentsDto, out genericArguments) &&
                portableObjectBoxConverter.TryConvertFromDataTransferObject(methodArgumentsDto, out methodArguments) &&
                localServiceContainer.TryInvoke(serviceGuid, methodName, genericArguments, methodArguments, out result)) {
               logger.Trace($"Invoke succeeded at local service container for service {serviceGuid} method {methodName} with {methodArgumentsDto.Length} bytes of arguments.");
            } else {
               logger.Trace($"Trying remote invocation for service {serviceGuid} method {methodName} with {methodArgumentsDto.Length} bytes of arguments against {remoteInvokables.Count} remote invokables.");
               bool invocationSuccessful = false;
               foreach (var remoteInvokable in remoteInvokables) {
                  var invocation = await remoteInvokable.TryRemoteInvoke(serviceGuid, methodName, genericArgumentsDto, methodArgumentsDto);
                  if (invocation.Success) {
                     logger.Trace($"Successfully remotely invoked service {serviceGuid} method {methodName} with {methodArgumentsDto.Length} bytes of arguments.");
                     result = invocation.ReturnValue;
                     invocationSuccessful = true;
                     break;
                  }
               }

               if (!invocationSuccessful) {
                  logger.Trace($"Could not remotely invoke service {serviceGuid} method {methodName} with {methodArgumentsDto.Length} bytes of arguments.");
                  throw new ServiceUnavailableException(serviceGuid, methodName);
               }
            }
         } catch (Exception e) {
            logger.Trace($"Remove invocation of service {serviceGuid} method {methodName} with {methodArgumentsDto.Length} bytes of arguments threw", e);
            if (e is IPortableObject) {
               result = e;
            } else {
               result = new PortableException(e);
            }
         }
         return result;
      }

      public void AddRemoteInvokable(RemoteInvokable remoteInvokable) {
         logger.Trace($"Added remote invokable: {remoteInvokable}.");
         remoteInvokables.TryAdd(remoteInvokable);
      }

      public void RemoveRemoteInvokable(RemoteInvokable remoteInvokable) {
         logger.Trace($"Removed remote invokable: {remoteInvokable}.");
         remoteInvokables.TryRemove(remoteInvokable);
      }

      public void Dispose() {
      }
   }
}
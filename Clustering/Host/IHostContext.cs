using System;
using System.Threading.Tasks;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services.Clustering.Host {
   public interface IHostContext : IDisposable {
      Task<object> Invoke(Guid serviceGuid, string methodName, object[] methodArguments);
      Task<object> Invoke(Guid serviceGuid, string methodName, MethodArgumentsDto methodArguments);
      void AddRemoteInvokable(IRemoteInvokable remoteInvokable);
      void RemoveRemoteInvokable(IRemoteInvokable remoteInvokable);
   }

   public interface IRemoteInvokable {
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, object[] arguments);
      Task<RemoteInvocationResult> TryRemoteInvoke(Guid serviceGuid, string methodName, MethodArgumentsDto argumentsDto);
   }

   public class HostContext : IHostContext {
      public IConcurrentSet<IRemoteInvokable> RemoteInvokables { get; set; }
      private readonly MethodArgumentsConverter methodArgumentsConverter;
      private readonly LocalServiceContainer localServiceContainer;
      private readonly IConcurrentSet<IRemoteInvokable> remoteInvokables;

      public HostContext(
         MethodArgumentsConverter methodArgumentsConverter, 
         LocalServiceContainer localServiceContainer
      ) : this(
         methodArgumentsConverter, 
         localServiceContainer, 
         new ConcurrentSet<IRemoteInvokable>()) {}

      public HostContext(MethodArgumentsConverter methodArgumentsConverter, LocalServiceContainer localServiceContainer, IConcurrentSet<IRemoteInvokable> remoteInvokables) {
         this.methodArgumentsConverter = methodArgumentsConverter;
         this.localServiceContainer = localServiceContainer;
         this.remoteInvokables = remoteInvokables;
      }

      public async Task<object> Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {
         object result;
         try {
            if (!localServiceContainer.TryInvoke(serviceGuid, methodName, methodArguments, out result)) {
               bool invocationSuccessful = false;
               foreach (var remoteInvokable in remoteInvokables) {
                  var invocation = await remoteInvokable.TryRemoteInvoke(serviceGuid, methodName, methodArguments);
                  if (invocation.Success) {
                     result = invocation.ReturnValue;
                     invocationSuccessful = true;
                     break;
                  }
               }

               if (!invocationSuccessful) {
                  throw new ServiceUnavailableException(serviceGuid, methodName);
               }
            }
         } catch (Exception e) {
            result = new PortableException(e);
         }
         return result;
      }

      public async Task<object> Invoke(Guid serviceGuid, string methodName, MethodArgumentsDto methodArgumentsDto) {
         object result = null;
         try {
            object[] methodArguments;
            if (methodArgumentsConverter.TryConvertFromDataTransferObject(methodArgumentsDto, out methodArguments) &&
                localServiceContainer.TryInvoke(serviceGuid, methodName, methodArguments, out result)) {
            } else {
               bool invocationSuccessful = false;
               foreach (var remoteInvokable in remoteInvokables) {
                  var invocation = await remoteInvokable.TryRemoteInvoke(serviceGuid, methodName, methodArgumentsDto);
                  if (invocation.Success) {
                     result = invocation.ReturnValue;
                     invocationSuccessful = true;
                     break;
                  }
               }

               if (!invocationSuccessful) {
                  throw new ServiceUnavailableException(serviceGuid, methodName);
               }
            }
         } catch (Exception e) {
            result = new PortableException(e);
         }
         return result;
      }

      public void AddRemoteInvokable(IRemoteInvokable remoteInvokable) {
         remoteInvokables.TryAdd(remoteInvokable);
      }

      public void RemoveRemoteInvokable(IRemoteInvokable remoteInvokable) {
         remoteInvokables.TryRemove(remoteInvokable);
      }

      public void Dispose() {
      }
   }
}
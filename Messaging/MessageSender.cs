using Dargon.PortableObjects.Streams;
using ItzWarty.Collections;
using System;
using System.Threading.Tasks;

namespace Dargon.Services.Messaging {
   public interface MessageSender : IDisposable {
      Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments);
      Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, MethodArgumentsDto methodArgumentsDto);
      Task SendInvocationResultAsync(uint invocationId, object result);
      Task SendServiceBroadcastAsync(IReadOnlySet<Guid> serviceGuids);
      Task SendServiceUpdateAsync(IReadOnlySet<Guid> addedServices, IReadOnlySet<Guid> removedServices);
   }

   public class MessageSenderImpl : MessageSender {
      private readonly PofStreamWriter pofStreamWriter;
      private readonly MethodArgumentsConverter methodArgumentsConverter;

      public MessageSenderImpl(PofStreamWriter pofStreamWriter, MethodArgumentsConverter methodArgumentsConverter) {
         this.pofStreamWriter = pofStreamWriter;
         this.methodArgumentsConverter = methodArgumentsConverter;
      }

      public Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments) {
         var methodArgumentsDto = methodArgumentsConverter.ConvertToDataTransferObject(methodArguments);
         return SendServiceInvocationAsync(invocationId, serviceGuid, methodName, methodArgumentsDto);
      }

      public Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, MethodArgumentsDto methodArgumentsDto) {
         var message = new X2XServiceInvocation(invocationId, serviceGuid, methodName, methodArgumentsDto);
         return pofStreamWriter.WriteAsync(message);
      }

      public Task SendInvocationResultAsync(uint invocationId, object result) {
         var message = new X2XInvocationResult(invocationId, result);
         return pofStreamWriter.WriteAsync(message);
      }

      public Task SendServiceBroadcastAsync(IReadOnlySet<Guid> serviceGuids) {
         return pofStreamWriter.WriteAsync(new G2HServiceBroadcast(serviceGuids));
      }

      public Task SendServiceUpdateAsync(IReadOnlySet<Guid> addedServices, IReadOnlySet<Guid> removedServices) {
         return pofStreamWriter.WriteAsync(new G2HServiceUpdate(addedServices, removedServices));
      }

      public void Dispose() {
         pofStreamWriter.Dispose();
      }
   }
}

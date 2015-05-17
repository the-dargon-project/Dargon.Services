using Dargon.PortableObjects.Streams;
using ItzWarty.Collections;
using System;
using System.Threading.Tasks;

namespace Dargon.Services.Messaging {
   public interface MessageSender : IDisposable {
      Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments);
      Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, PortableObjectBox portableObjectBox);
      Task SendInvocationResultAsync(uint invocationId, object result);
      Task SendServiceBroadcastAsync(IReadOnlySet<Guid> serviceGuids);
      Task SendServiceUpdateAsync(IReadOnlySet<Guid> addedServices, IReadOnlySet<Guid> removedServices);
   }

   public class MessageSenderImpl : MessageSender {
      private readonly PofStreamWriter pofStreamWriter;
      private readonly PortableObjectBoxConverter portableObjectBoxConverter;

      public MessageSenderImpl(PofStreamWriter pofStreamWriter, PortableObjectBoxConverter portableObjectBoxConverter) {
         this.pofStreamWriter = pofStreamWriter;
         this.portableObjectBoxConverter = portableObjectBoxConverter;
      }

      public Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments) {
         var methodArgumentsDto = portableObjectBoxConverter.ConvertToDataTransferObject(methodArguments);
         return SendServiceInvocationAsync(invocationId, serviceGuid, methodName, methodArgumentsDto);
      }

      public Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, PortableObjectBox portableObjectBox) {
         var message = new X2XServiceInvocation(invocationId, serviceGuid, methodName, portableObjectBox);
         return pofStreamWriter.WriteAsync(message);
      }

      public Task SendInvocationResultAsync(uint invocationId, object result) {
         var resultBox = result as PortableObjectBox;
         if (resultBox == null) {
            resultBox = portableObjectBoxConverter.ConvertToDataTransferObject(new[] { result });
         }
         var message = new X2XInvocationResult(invocationId, resultBox);
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

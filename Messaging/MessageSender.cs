using Dargon.PortableObjects.Streams;
using ItzWarty.Collections;
using ItzWarty.IO;
using System;
using System.Threading.Tasks;

namespace Dargon.Services.Messaging {
   public interface MessageSender : IDisposable {
      Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments);
      Task SendInvocationResultAsync(uint invocationId, object result);
      Task SendServiceBroadcastAsync(IReadOnlySet<Guid> serviceGuids);
      Task SendServiceUpdateAsync(IReadOnlySet<Guid> addedServices, IReadOnlySet<Guid> removedServices);
   }

   public class MessageSenderImpl : MessageSender {
      private readonly PofStreamWriter pofStreamWriter;

      public MessageSenderImpl(PofStreamWriter pofStreamWriter) {
         this.pofStreamWriter = pofStreamWriter;
      }

      public Task SendServiceInvocationAsync(uint invocationId, Guid serviceGuid, string methodName, object[] methodArguments) {
         var message = new X2XServiceInvocation(invocationId, serviceGuid, methodName, methodArguments);
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

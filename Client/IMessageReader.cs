using System;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using ItzWarty.IO;
using ItzWarty.Threading;

namespace Dargon.Services.Client {
   public interface IMessageReader : IDisposable {
      void Initialize();
   }

   public class MessageReader : IMessageReader {
      private readonly IThreadingProxy threadingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly IInvocationResponseProducerContext context;
      private readonly IBinaryReader reader;
      private readonly IThread thread;
      private readonly ICancellationTokenSource readerCancellationTokenSource;
      private readonly ICancellationTokenSource linkedCancellationTokenSource;

      public MessageReader(IThreadingProxy threadingProxy, IPofSerializer pofSerializer, IInvocationResponseProducerContext context, IBinaryReader reader) {
         this.threadingProxy = threadingProxy;
         this.pofSerializer = pofSerializer;
         this.context = context;
         this.reader = reader;

         this.thread = threadingProxy.CreateThread(ThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
         this.readerCancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.linkedCancellationTokenSource = threadingProxy.CreateLinkedCancellationTokenSource(context.CancellationToken, readerCancellationTokenSource.Token);
      }

      public void Initialize() {
         thread.Start();
      }

      private void ThreadEntryPoint() {
         var cancellationToken = linkedCancellationTokenSource.Token;
         while (!cancellationToken.IsCancellationRequested) {
            var message = pofSerializer.Deserialize(reader.__Reader);
            var messageType = message.GetType();
            if (messageType == typeof(H2CInvocationResult)) {
               HandleH2CInvocationResult((H2CInvocationResult)message);
            }
         }
      }

      private void HandleH2CInvocationResult(H2CInvocationResult x) {
         context.HandleInvocationResult(x.InvocationId, x.Payload);
      }

      public void Dispose() {
         readerCancellationTokenSource.Cancel();
         linkedCancellationTokenSource.Cancel();
         reader.Dispose();
         thread.Join();
      }
   }
}
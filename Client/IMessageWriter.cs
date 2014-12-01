using System;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using ItzWarty.IO;
using ItzWarty.Threading;

namespace Dargon.Services.Client {
   public interface IMessageWriter : IDisposable {
      void Initialize();
   }

   public class MessageWriter : IMessageWriter {
      private readonly IThreadingProxy threadingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly IInvocationRequestConsumerContext context;
      private readonly IBinaryWriter writer;
      private readonly IThread thread;

      public MessageWriter(IThreadingProxy threadingProxy, IPofSerializer pofSerializer, IInvocationRequestConsumerContext context, IBinaryWriter writer) {
         this.threadingProxy = threadingProxy;
         this.pofSerializer = pofSerializer;
         this.context = context;
         this.writer = writer;

         this.thread = threadingProxy.CreateThread(ThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
      }

      public void Initialize() {
         thread.Start();
      }

      private void ThreadEntryPoint() {
         var cancellationToken = context.CancellationToken;
         while (!cancellationToken.IsCancellationRequested) {
            IInvocationState invocationState;
            if (context.TryTakeUnsentInvocation(out invocationState)) {
               var dto = new C2HServiceInvocation(
                  invocationState.InvocationId, 
                  invocationState.ServiceGuid, 
                  invocationState.MethodName, 
                  invocationState.MethodArguments
               );
               pofSerializer.Serialize(writer.__Writer, dto);
            }
         }
      }

      public void Dispose() {
         throw new NotImplementedException();
      }
   }
}
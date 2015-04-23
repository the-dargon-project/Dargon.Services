using System;
using System.Diagnostics;
using System.Net.Sockets;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Phases.Guest {
   public class GuestPhase : IPhase {
      private readonly IPhaseFactory phaseFactory;
      private readonly IServiceNodeContext context;
      private readonly IConnectedSocket socket;
      private readonly PofStream pofStream;
      private readonly PofStreamWriter pofStreamWriter;
      private readonly PofDispatcher pofDispatcher;

      public GuestPhase(
         PofStreamsFactory pofStreamsFactory, 
         IPhaseFactory phaseFactory, 
         IPofSerializer pofSerializer, 
         IServiceNodeContext context, 
         IConnectedSocket socket
      ) : this(pofStreamsFactory,
               phaseFactory,
               context,
               socket,
               pofStreamsFactory.CreatePofStream(socket.Stream)
      ) { }

      public GuestPhase(
         PofStreamsFactory pofStreamsFactory,
         IPhaseFactory phaseFactory, 
         IServiceNodeContext context, 
         IConnectedSocket socket, 
         PofStream pofStream
      ) : this(
         phaseFactory,
         context,
         socket,
         pofStream,
         pofStream.Writer,
         pofStreamsFactory.CreateDispatcher(pofStream)
      ) { }

      private GuestPhase(IPhaseFactory phaseFactory, IServiceNodeContext context, IConnectedSocket socket, PofStream pofStream, PofStreamWriter pofStreamWriter, PofDispatcher pofDispatcher) {
         this.phaseFactory = phaseFactory;
         this.context = context;
         this.socket = socket;
         this.pofStream = pofStream;
         this.pofStreamWriter = pofStreamWriter;
         this.pofDispatcher = pofDispatcher;
      }

      public void Initialize() {
         Debug.WriteLine("Guest init");
         pofDispatcher.RegisterHandler<X2XServiceInvocation>(HandleX2XServiceInvocation);
         pofDispatcher.RegisterShutdownHandler(HandleDispatcherShutdown);
         pofDispatcher.Start();
      }

      public void HandleEnter() {
         var servicesGuids = new HashSet<Guid>(context.EnumerateServiceGuids());
         pofStreamWriter.WriteAsync(new G2HServiceBroadcast(servicesGuids));
      }

      private void HandleX2XServiceInvocation(X2XServiceInvocation x) {
         object result;
         try {
            if (!context.TryInvoke(x.ServiceGuid, x.MethodName, x.MethodArguments, out result)) {
               result = new PortableException(new ServiceUnavailableException(x.ServiceGuid, x.MethodName));
            }
         } catch (Exception e) {
            result = new PortableException(e);
         }
         pofStreamWriter.WriteAsync(new X2XInvocationResult(x.InvocationId, result));
      }

      private void HandleDispatcherShutdown() {
         context.Transition(phaseFactory.CreateIndeterminatePhase(context));
      }

      public void HandleServiceRegistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid>();
         addedServices.Add(invokableServiceContext.Guid);
         pofStreamWriter.WriteAsync(new G2HServiceUpdate(addedServices, removedServices));
//         pofSerializer.Serialize(socket.GetWriter(), serviceUpdate);
      }

      public void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext) {
         var addedServices = new HashSet<Guid>();
         var removedServices = new HashSet<Guid>();
         removedServices.Add(invokableServiceContext.Guid);
         pofStreamWriter.WriteAsync(new G2HServiceUpdate(addedServices, removedServices));
      }

      public void Dispose() {
      }
   }
}
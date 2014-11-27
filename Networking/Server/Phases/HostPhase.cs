using Dargon.PortableObjects;
using Dargon.Services.Networking.PortableObjects;
using Dargon.Services.Networking.Server.Sessions;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;
using System;
using System.IO;
using System.Net.Sockets;

namespace Dargon.Services.Networking.Server.Phases {
   public class HostPhase : IPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly IConnectorContext connectorContext;
      private readonly IListenerSocket listenerSocket;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly IConcurrentSet<IHostSession> sessions;
      private readonly IHostContext hostContext;

      private bool disposed = false;

      public HostPhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPofSerializer pofSerializer, IHostSessionFactory hostSessionFactory, IConnectorContext connectorContext, IListenerSocket listenerSocket) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofSerializer = pofSerializer;
         this.hostSessionFactory = hostSessionFactory;
         this.connectorContext = connectorContext;
         this.listenerSocket = listenerSocket;
         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.sessions = new ConcurrentSet<IHostSession>();
      }

      public void Initialize() {
         var listenerThread = threadingProxy.CreateThread(ListenerThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
         listenerThread.Start();
      }

      internal void ListenerThreadEntryPoint() {
         while (!cancellationTokenSource.IsCancellationRequested) {
            var socket = listenerSocket.Accept();
            var thread = threadingProxy.CreateThread(() => SessionThreadEntryPoint(socket), new ThreadCreationOptions { IsBackground = true });
            thread.Start();
         }
      }

      internal void SessionThreadEntryPoint(IConnectedSocket socket) {
         try {
            var handshake = pofSerializer.Deserialize<X2SHandshake>(socket.GetReader());
            if (handshake.Role == Role.Client) {
               var clientSession = hostSessionFactory.CreateClientSession(socket.GetReader(), socket.GetWriter());
               this.sessions.Add(clientSession);
            } else if (handshake.Role == Role.Guest) {
               var guestSession = hostSessionFactory.CreateGuestSession(socket.GetReader(), socket.GetWriter());
               this.sessions.Add(guestSession);
            } else {
               // do nothing
            }
         } catch (SocketException e) {
            logger.Warn(e);
         } catch (Exception e) {
            logger.Error(e);
         }
      }

      public void RunIteration() {
         // this.connectorContext.CurrentPhase
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            cancellationTokenSource.Dispose();
            listenerSocket.Dispose();
         }
      }
   }
}
using System;
using System.Net.Sockets;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server.Sessions;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Services.Server.Phases {
   public class HostPhase : IPhase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly IPofSerializer pofSerializer;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly IConnectorContext connectorContext;
      private readonly IListenerSocket listenerSocket;
      private readonly ICancellationTokenSource cancellationTokenSource;
      private readonly IConcurrentSet<IClientSession> clientSessions;
      private readonly IConcurrentSet<IGuestSession> guestSessions;
      private readonly IHostContext hostContext;

      private bool disposed = false;

      public HostPhase(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPofSerializer pofSerializer, IHostSessionFactory hostSessionFactory, IConnectorContext connectorContext, IListenerSocket listenerSocket, IHostContext hostContext) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofSerializer = pofSerializer;
         this.hostSessionFactory = hostSessionFactory;
         this.connectorContext = connectorContext;
         this.listenerSocket = listenerSocket;
         this.hostContext = hostContext;

         this.cancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.clientSessions = collectionFactory.CreateConcurrentSet<IClientSession>();
         this.guestSessions = collectionFactory.CreateConcurrentSet<IGuestSession>();
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
         IHostSession session = null;
         try {
            var handshake = pofSerializer.Deserialize<X2SHandshake>(socket.GetReader().__Reader);
            if (handshake.Role == Role.Client) {
               var clientSession = hostSessionFactory.CreateClientSession(socket.GetReader().__Reader, socket.GetWriter().__Writer);
               session = clientSession;
               clientSessions.Add(clientSession);
               clientSession.Run();
            } else if (handshake.Role == Role.Guest) {
               var guestSession = hostSessionFactory.CreateGuestSession(socket.GetReader().__Reader, socket.GetWriter().__Writer);
               session = guestSession;
               guestSessions.Add(guestSession);
               guestSession.Run();
            } else {
               // do nothing
            }
         } catch (SocketException e) {
            logger.Warn(e);
         } catch (Exception e) {
            logger.Error(e);
         } finally {
            if (session != null) {
               if (session.Role == Role.Client) {
                  clientSessions.Remove((IClientSession)session);
               } else if (session.Role == Role.Guest) {
                  guestSessions.Remove((IGuestSession)session);
               }
            }
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
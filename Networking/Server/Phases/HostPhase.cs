using Dargon.PortableObjects;
using Dargon.Services.Networking.PortableObjects;
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
      private readonly IContext context;
      private readonly IListenerSocket listenerSocket;

      private readonly IConcurrentDictionary<IConnectedSocket, string> blahByClient = new ConcurrentDictionary<IConnectedSocket, string>();

      public HostPhase(IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, IPofSerializer pofSerializer, IContext context, IListenerSocket listenerSocket) {
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofSerializer = pofSerializer;
         this.context = context;
         this.listenerSocket = listenerSocket;
      }

      public void Initialize() {
         threadingProxy.CreateThread(ListenerThreadEntryPoint, new ThreadCreationOptions { IsBackground = true });
      }

      private void ListenerThreadEntryPoint() {
         while (true) {
            var socket = networkingProxy.Accept(listenerSocket);
            threadingProxy.CreateThread(() => SessionThreadEntryPoint(socket), new ThreadCreationOptions { IsBackground = true });
         }
      }

      private void SessionThreadEntryPoint(IConnectedSocket socket) {
         using (var ns = networkingProxy.CreateNetworkStream(socket, true)) 
         using (var reader = new BinaryReader(ns)) 
         using (var writer = new BinaryWriter(ns)) {
            try {
               while (true) {
                  var serviceBroadcast = pofSerializer.Deserialize<G2HServiceBroadcast>(reader);
               }
            } catch (SocketException e) {
               logger.Warn(e);
            } catch (Exception e) {
               logger.Error(e);
            }
         }
      }

      public void HandleUpdate() {

      }
   }
}
using Dargon.PortableObjects.Streams;
using Dargon.Services.Clustering.Guest;
using Dargon.Services.Clustering.Host;
using Dargon.Services.Clustering.Indeterminate;
using Dargon.Services.Messaging;
using Dargon.Services.Server;
using Dargon.Services.Utilities;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Services.Clustering {
   public interface ClusteringPhaseFactory {
      ClusteringPhase CreateIndeterminatePhase(LocalServiceContainer localServiceContainer);
      ClusteringPhase CreateHostPhase(LocalServiceContainer localServiceContainer, IListenerSocket listenerSocket);
      ClusteringPhase CreateGuestPhase(LocalServiceContainer localServiceContainer, IConnectedSocket clientSocket);
   }
   public class ClusteringPhaseFactoryImpl : ClusteringPhaseFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly INetworkingProxy networkingProxy;
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly IHostSessionFactory hostSessionFactory;
      private readonly IClusteringConfiguration clusteringConfiguration;
      private readonly ClusteringPhaseManager clusteringPhaseManager;

      public ClusteringPhaseFactoryImpl(ICollectionFactory collectionFactory, IThreadingProxy threadingProxy, INetworkingProxy networkingProxy, PofStreamsFactory pofStreamsFactory, IHostSessionFactory hostSessionFactory, IClusteringConfiguration clusteringConfiguration, ClusteringPhaseManager clusteringPhaseManager) {
         this.collectionFactory = collectionFactory;
         this.threadingProxy = threadingProxy;
         this.networkingProxy = networkingProxy;
         this.pofStreamsFactory = pofStreamsFactory;
         this.hostSessionFactory = hostSessionFactory;
         this.clusteringConfiguration = clusteringConfiguration;
         this.clusteringPhaseManager = clusteringPhaseManager;
      }

      public ClusteringPhase CreateIndeterminatePhase(LocalServiceContainer localServiceContainer) {
         var phase = new IndeterminateClusteringPhase(threadingProxy, networkingProxy, this, clusteringConfiguration, localServiceContainer, clusteringPhaseManager);
         return phase;
      }

      public ClusteringPhase CreateHostPhase(LocalServiceContainer localServiceContainer, IListenerSocket listenerSocket) {
         var hostContext = new HostContext(localServiceContainer);
         var phase = new HostPhase(collectionFactory, threadingProxy, hostSessionFactory, hostContext, listenerSocket);
         return phase;
      }

      public ClusteringPhase CreateGuestPhase(LocalServiceContainer localServiceContainer, IConnectedSocket clientSocket) {
         var pofStream = pofStreamsFactory.CreatePofStream(clientSocket.Stream);
         var pofDispatcher = pofStreamsFactory.CreateDispatcher(pofStream);
         var messageSender = new MessageSenderImpl(pofStream.Writer);
         var phase = new GuestPhase(
            this, 
            localServiceContainer, 
            clusteringPhaseManager, 
            messageSender,
            pofDispatcher,
            collectionFactory.CreateUniqueIdentificationSet(true), 
            collectionFactory.CreateConcurrentDictionary<uint, AsyncValueBox>()
         );
         phase.Initialize();
         return phase;
      }
   }
}

using Dargon.Services.Phases;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services {
   public interface IServiceClientFactory {
      IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration);
   }

   public class ServiceClientFactory : IServiceClientFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;
      private readonly IPhaseFactory phaseFactory;

      public ServiceClientFactory(ICollectionFactory collectionFactory, InvokableServiceContextFactory invokableServiceContextFactory, IPhaseFactory phaseFactory) {
         this.collectionFactory = collectionFactory;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
         this.phaseFactory = phaseFactory;
      }

      public IServiceClient CreateOrJoin(IClusteringConfiguration clusteringConfiguration) {
         LocalServiceContainer localServiceContainer = new LocalServiceContainerImpl(collectionFactory, clusteringConfiguration);
         IPhase initialPhase = phaseFactory.CreateIndeterminatePhase(localServiceContainer);
         localServiceContainer.Transition(initialPhase);
         return new ServiceClient(collectionFactory, localServiceContainer, invokableServiceContextFactory);
      }
   }
}

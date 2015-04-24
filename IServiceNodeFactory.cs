using Dargon.Services.Phases;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services {
   public interface IServiceNodeFactory {
      IServiceNode CreateOrJoin(INodeConfiguration nodeConfiguration);
   }

   public class ServiceNodeFactory : IServiceNodeFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly InvokableServiceContextFactory invokableServiceContextFactory;
      private readonly IPhaseFactory phaseFactory;

      public ServiceNodeFactory(ICollectionFactory collectionFactory, InvokableServiceContextFactory invokableServiceContextFactory, IPhaseFactory phaseFactory) {
         this.collectionFactory = collectionFactory;
         this.invokableServiceContextFactory = invokableServiceContextFactory;
         this.phaseFactory = phaseFactory;
      }

      public IServiceNode CreateOrJoin(INodeConfiguration nodeConfiguration) {
         LocalServiceContainer localServiceContainer = new LocalServiceContainerImpl(collectionFactory, nodeConfiguration);
         IPhase initialPhase = phaseFactory.CreateIndeterminatePhase(localServiceContainer);
         localServiceContainer.Transition(initialPhase);
         return new ServiceNode(collectionFactory, localServiceContainer, invokableServiceContextFactory);
      }
   }
}

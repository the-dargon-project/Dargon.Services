using Dargon.Services.Phases;
using Dargon.Services.Server;
using ItzWarty.Collections;

namespace Dargon.Services {
   public interface IServiceNodeFactory {
      IServiceNode CreateOrJoin(INodeConfiguration nodeConfiguration);
   }

   public class ServiceNodeFactory : IServiceNodeFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IPhaseFactory phaseFactory;

      public ServiceNodeFactory(ICollectionFactory collectionFactory, IServiceContextFactory serviceContextFactory, IPhaseFactory phaseFactory) {
         this.collectionFactory = collectionFactory;
         this.serviceContextFactory = serviceContextFactory;
         this.phaseFactory = phaseFactory;
      }

      public IServiceNode CreateOrJoin(INodeConfiguration nodeConfiguration) {
         IServiceNodeContext serviceNodeContext = new ServiceNodeContext(collectionFactory, nodeConfiguration);
         IPhase initialPhase = phaseFactory.CreateIndeterminatePhase(serviceNodeContext);
         serviceNodeContext.Transition(initialPhase);
         return new ServiceNode(collectionFactory, serviceNodeContext, serviceContextFactory);
      }
   }
}

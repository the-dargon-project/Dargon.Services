using ItzWarty.Collections;

namespace Dargon.Services.Server {
   public interface IServiceNodeFactory {
      IServiceNode CreateOrJoin(IServiceConfiguration serviceConfiguration);
   }

   public class ServiceNodeFactory : IServiceNodeFactory {
      private readonly ICollectionFactory collectionFactory;
      private readonly IServiceContextFactory serviceContextFactory;
      private readonly IConnectorFactory connectorFactory;

      public ServiceNodeFactory(IConnectorFactory connectorFactory, IServiceContextFactory serviceContextFactory, ICollectionFactory collectionFactory) {
         this.connectorFactory = connectorFactory;
         this.serviceContextFactory = serviceContextFactory;
         this.collectionFactory = collectionFactory;
      }

      public IServiceNode CreateOrJoin(IServiceConfiguration serviceConfiguration) {
         var connector = connectorFactory.CreateServiceConnector(serviceConfiguration);
         return new ServiceNode(collectionFactory, connector, serviceContextFactory);
      }
   }
}

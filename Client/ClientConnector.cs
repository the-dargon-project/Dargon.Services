using System;
using ItzWarty.Collections;

namespace Dargon.Services.Client {
   public class ClientConnector : IClientConnector{
      private IUniqueIdentificationSet availableInvocationIdentifiers;

      public ClientConnector(ICollectionFactory collectionFactory) {
         this.availableInvocationIdentifiers = collectionFactory.CreateUniqueIdentificationSet(true);
      }

      public object Invoke(Guid serviceGuid, string methodName, object[] methodArguments) {

      }
   }
}
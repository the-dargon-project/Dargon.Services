using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Dargon.Services.Clustering.Remote {
   public class RemoteServiceClientContainerImpl : RemoteServiceClientContainer {
      private readonly object synchronization = new object();
      private readonly Dictionary<IPEndPoint, ServiceClient> remoteServiceClientsByIpEndpoint = new Dictionary<IPEndPoint, ServiceClient>();
      private readonly ServiceClientFactory serviceClientFactory;
      private ServiceClient[] serviceClients = new ServiceClient[0];

      public RemoteServiceClientContainerImpl(ServiceClientFactory serviceClientFactory) {
         this.serviceClientFactory = serviceClientFactory;
      }

      public ServiceClient[] ServiceClients => serviceClients;

      public void AddEndPoint(IPEndPoint endpoint) {
         lock (synchronization) {
            if (!remoteServiceClientsByIpEndpoint.ContainsKey(endpoint)) {
               var serviceClient = serviceClientFactory.Remote(endpoint);
               remoteServiceClientsByIpEndpoint.Add(endpoint, serviceClient);
               serviceClients = remoteServiceClientsByIpEndpoint.Values.ToArray();
            }
         }
      }

      public void RemoveEndPoint(IPEndPoint endpoint) {
         lock (synchronization) {
            if (!remoteServiceClientsByIpEndpoint.Remove(endpoint)) {
               throw new InvalidOperationException("Failed to remove endpoint: " + endpoint);
            } else {
               serviceClients = remoteServiceClientsByIpEndpoint.Values.ToArray();
            }
         }
      }
   }
}
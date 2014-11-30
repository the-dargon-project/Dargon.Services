using System;
using Dargon.PortableObjects;
using ItzWarty.Collections;

namespace Dargon.Services.PortableObjects {
   public class G2HServiceBroadcast : IPortableObject, IEquatable<G2HServiceBroadcast> {           
      private IReadOnlySet<Guid> serviceGuids;

      public G2HServiceBroadcast() { }

      public G2HServiceBroadcast(IReadOnlySet<Guid> serviceGuids) {
         this.serviceGuids = serviceGuids;
      }

      public IReadOnlySet<Guid> ServiceGuids { get { return serviceGuids; } } 

      public void Serialize(IPofWriter writer) {
         writer.WriteCollection(0, serviceGuids);
      }

      public void Deserialize(IPofReader reader) {
         serviceGuids = reader.ReadCollection<Guid, HashSet<Guid>>(0);
      }

      public override bool Equals(object obj) {
         return obj is G2HServiceBroadcast && Equals((G2HServiceBroadcast)obj);
      }

      public bool Equals(G2HServiceBroadcast other) {
         return serviceGuids.SetEquals(other.serviceGuids);
      }
   }
}

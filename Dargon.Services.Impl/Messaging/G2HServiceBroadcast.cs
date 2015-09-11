using System;
using Dargon.PortableObjects;
using ItzWarty.Collections;

namespace Dargon.Services.Messaging {
   internal class G2HServiceBroadcast : IPortableObject {           
      private IReadOnlySet<Guid> serviceGuids;

      public G2HServiceBroadcast() { }

      public G2HServiceBroadcast(IReadOnlySet<Guid> serviceGuids) {
         this.serviceGuids = serviceGuids;
      }

      public IReadOnlySet<Guid> ServiceGuids => serviceGuids;

      public void Serialize(IPofWriter writer) {
         writer.WriteCollection(0, serviceGuids);
      }

      public void Deserialize(IPofReader reader) {
         serviceGuids = reader.ReadCollection<Guid, HashSet<Guid>>(0);
      }
   }
}

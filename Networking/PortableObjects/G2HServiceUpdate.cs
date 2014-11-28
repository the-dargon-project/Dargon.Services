using System;
using Dargon.PortableObjects;
using ItzWarty.Collections;

namespace Dargon.Services.Networking.Server.Phases {
   public class G2HServiceUpdate : IPortableObject {
      private IReadOnlySet<Guid> addedServiceGuids;
      private IReadOnlySet<Guid> removedServiceGuids;

      public G2HServiceUpdate() { }

      public G2HServiceUpdate(
         IReadOnlySet<Guid> addedServiceGuids, 
         IReadOnlySet<Guid> removedServiceGuids
      ) {
         this.addedServiceGuids = addedServiceGuids;
         this.removedServiceGuids = removedServiceGuids;
      }

      public IReadOnlySet<Guid> AddedServiceGuids { get { return addedServiceGuids; } }
      public IReadOnlySet<Guid> RemovedServiceGuids { get { return removedServiceGuids; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteCollection(0, addedServiceGuids);
         writer.WriteCollection(1, removedServiceGuids);
      }

      public void Deserialize(IPofReader reader) {
         addedServiceGuids = reader.ReadCollection<Guid, HashSet<Guid>>(0);
         removedServiceGuids = reader.ReadCollection<Guid, HashSet<Guid>>(1);
      }
   }
}
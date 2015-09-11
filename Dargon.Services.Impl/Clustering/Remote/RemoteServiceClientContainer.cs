using System.Net;

namespace Dargon.Services.Clustering.Remote {
   public interface RemoteServiceClientContainer : RemoteServiceClientSource {
      void AddEndPoint(IPEndPoint endpoint);
      void RemoveEndPoint(IPEndPoint endpoint);
   }
}

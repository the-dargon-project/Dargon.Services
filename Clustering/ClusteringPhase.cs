﻿using System;
using System.Threading.Tasks;
using Dargon.Services.Server;

namespace Dargon.Services.Clustering {
   public interface ClusteringPhase : IDisposable {
      void HandleEnter();
      void HandleServiceRegistered(InvokableServiceContext invokableServiceContext);
      void HandleServiceUnregistered(InvokableServiceContext invokableServiceContext);

      Task<object> InvokeServiceCall(Guid serviceGuid, string methodName, object[] methodArguments);
   }
}
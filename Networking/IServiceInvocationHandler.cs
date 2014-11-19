namespace Dargon.Services.Networking {
   internal interface IServiceInvocationHandler {
      object HandleServiceInvocation(string service, string action, object[] arguments);
   }
}
using System;
using Dargon.PortableObjects;

namespace Dargon.Services.Networking.PortableObjects {
   public class PortableException : Exception, IPortableObject {
      private string type;
      private string message;
      private string stackTrace;
      private PortableException innerException;

      public PortableException() { }

      public PortableException(Exception e) {
         this.type = e.GetType().FullName;
         this.message = e.Message;
         this.stackTrace = e.StackTrace;
         if (e.InnerException == null) {
            this.innerException = null;
         } else {
            this.innerException = new PortableException(e.InnerException);
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, type);
         writer.WriteString(1, message);
         writer.WriteString(2, stackTrace);
         writer.WriteObject(3, innerException);
      }

      public void Deserialize(IPofReader reader) {
         type = reader.ReadString(0);
         message = reader.ReadString(1);
         stackTrace = reader.ReadString(2);
         innerException = reader.ReadObject<PortableException>(3);
      }

      public override string Message { get { return message; } }
      public override string StackTrace { get { return stackTrace; } }
      public new PortableException InnerException { get { return innerException; } }
   }
}
using System;
using Dargon.PortableObjects;

namespace Dargon.Services.Messaging {
   public class PortableException : Exception, IPortableObject {
      private string type;
      private string message;
      private string stackTrace;
      private PortableException innerException;

      public PortableException() { }

      public PortableException(Exception e) : this(e.GetType().FullName, e.Message, e.StackTrace, e.InnerException) { }

      public PortableException(string exceptionType, string exceptionMessage, string exceptionStackTrace, Exception innerException) {
         this.type = exceptionType;
         this.message = exceptionMessage;
         this.stackTrace = exceptionStackTrace;
         this.innerException = innerException == null ? null : new PortableException(innerException);
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, type);
         writer.WriteString(1, message);
         writer.WriteObject(2, stackTrace);
         writer.WriteObject(3, innerException);
      }

      public void Deserialize(IPofReader reader) {
         type = reader.ReadString(0);
         message = reader.ReadString(1);
         stackTrace = (string)reader.ReadObject(2);
         innerException = reader.ReadObject<PortableException>(3);
      }

      public override string Message => message;
      public override string StackTrace => stackTrace;
      public new PortableException InnerException => innerException;
   }
}
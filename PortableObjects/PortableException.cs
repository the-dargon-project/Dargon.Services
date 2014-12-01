using System;
using System.Collections;
using Dargon.PortableObjects;

namespace Dargon.Services.PortableObjects {
   public class PortableException : Exception, IPortableObject, IEquatable<PortableException> {
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
         if (innerException == null) {
            this.innerException = null;
         } else {
            this.innerException = new PortableException(innerException);
         }
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

      public override string Message { get { return message; } }
      public override string StackTrace { get { return stackTrace; } }
      public new PortableException InnerException { get { return innerException; } }

      public override bool Equals(object other) {
         return other != null && Equals(other as PortableException);
      }

      public bool Equals(PortableException other) {
         return other != null && 
                Equals(type, other.type) && 
                Equals(message, other.message) && 
                Equals(stackTrace, other.stackTrace) && 
                Equals(innerException, other.innerException);
      }
   }
}
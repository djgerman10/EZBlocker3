﻿using System;
using System.Runtime.Serialization;

namespace EZBlocker3 {
    [Serializable]
    internal class IllegalStateException : Exception {

        public IllegalStateException() : this("The current state of the object is illegal.") { }
        public IllegalStateException(string message) : base(message) { }
        public IllegalStateException(string message, Exception innerException) : base(message, innerException) { }
        protected IllegalStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}
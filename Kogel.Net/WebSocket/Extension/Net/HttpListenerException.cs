using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Kogel.Net.WebSocket.Extension.Net
{
  /// <summary>
  /// The exception that is thrown when an error occurs processing
  /// an HTTP request.
  /// </summary>
  [Serializable]
  public class HttpListenerException : Win32Exception
  {
    #region Protected Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpListenerException"/>
    /// class from the specified instances of the <see cref="SerializationInfo"/>
    /// and <see cref="StreamingContext"/> classes.
    /// </summary>
    /// <param name="serializationInfo">
    /// A <see cref="SerializationInfo"/> that contains the serialized
    /// object data.
    /// </param>
    /// <param name="streamingContext">
    /// A <see cref="StreamingContext"/> that specifies the source for
    /// the deserialization.
    /// </param>
    protected HttpListenerException (
      SerializationInfo serializationInfo, StreamingContext streamingContext
    )
      : base (serializationInfo, streamingContext)
    {
    }

    #endregion

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpListenerException"/>
    /// class.
    /// </summary>
    public HttpListenerException ()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpListenerException"/>
    /// class with the specified error code.
    /// </summary>
    /// <param name="errorCode">
    /// An <see cref="int"/> that specifies the error code.
    /// </param>
    public HttpListenerException (int errorCode)
      : base (errorCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpListenerException"/>
    /// class with the specified error code and message.
    /// </summary>
    /// <param name="errorCode">
    /// An <see cref="int"/> that specifies the error code.
    /// </param>
    /// <param name="message">
    /// A <see cref="string"/> that specifies the message.
    /// </param>
    public HttpListenerException (int errorCode, string message)
      : base (errorCode, message)
    {
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the error code that identifies the error that occurred.
    /// </summary>
    /// <value>
    ///   <para>
    ///   An <see cref="int"/> that represents the error code.
    ///   </para>
    ///   <para>
    ///   It is any of the Win32 error codes.
    ///   </para>
    /// </value>
    public override int ErrorCode {
      get {
        return NativeErrorCode;
      }
    }

    #endregion
  }
}

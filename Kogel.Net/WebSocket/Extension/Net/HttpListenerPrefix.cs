using System;

namespace Kogel.Net.WebSocket.Extension.Net
{
  internal sealed class HttpListenerPrefix
  {
    #region Private Fields

    private string       _host;
    private HttpListener _listener;
    private string       _original;
    private string       _path;
    private string       _port;
    private string       _prefix;
    private bool         _secure;

    #endregion

    #region Internal Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpListenerPrefix"/> class
    /// with the specified URI prefix and HTTP listener.
    /// </summary>
    /// <remarks>
    /// This constructor must be called after calling the CheckPrefix method.
    /// </remarks>
    /// <param name="uriPrefix">
    /// A <see cref="string"/> that specifies the URI prefix.
    /// </param>
    /// <param name="listener">
    /// A <see cref="HttpListener"/> that specifies the HTTP listener.
    /// </param>
    internal HttpListenerPrefix (string uriPrefix, HttpListener listener)
    {
      _original = uriPrefix;
      _listener = listener;

      parse (uriPrefix);
    }

    #endregion

    #region Public Properties

    public string Host {
      get {
        return _host;
      }
    }

    public bool IsSecure {
      get {
        return _secure;
      }
    }

    public HttpListener Listener {
      get {
        return _listener;
      }
    }

    public string Original {
      get {
        return _original;
      }
    }

    public string Path {
      get {
        return _path;
      }
    }

    public string Port {
      get {
        return _port;
      }
    }

    #endregion

    #region Private Methods

    private void parse (string uriPrefix)
    {
      if (uriPrefix.StartsWith ("https"))
        _secure = true;

      var len = uriPrefix.Length;
      var host = uriPrefix.IndexOf (':') + 3;
      var root = uriPrefix.IndexOf ('/', host + 1, len - host - 1);

      var colon = uriPrefix.LastIndexOf (':', root - 1, root - host - 1);

      if (uriPrefix[root - 1] != ']' && colon > host) {
        _host = uriPrefix.Substring (host, colon - host);
        _port = uriPrefix.Substring (colon + 1, root - colon - 1);
      }
      else {
        _host = uriPrefix.Substring (host, root - host);
        _port = _secure ? "443" : "80";
      }

      _path = uriPrefix.Substring (root);

      _prefix = String.Format (
                  "{0}://{1}:{2}{3}",
                  _secure ? "https" : "http",
                  _host,
                  _port,
                  _path
                );
    }

    #endregion

    #region Public Methods

    public static void CheckPrefix (string uriPrefix)
    {
      if (uriPrefix == null)
        throw new ArgumentNullException ("uriPrefix");

      var len = uriPrefix.Length;

      if (len == 0) {
        var msg = "An empty string.";

        throw new ArgumentException (msg, "uriPrefix");
      }

      var schm = uriPrefix.StartsWith ("http://")
                 || uriPrefix.StartsWith ("https://");

      if (!schm) {
        var msg = "The scheme is not 'http' or 'https'.";

        throw new ArgumentException (msg, "uriPrefix");
      }

      var end = len - 1;

      if (uriPrefix[end] != '/') {
        var msg = "It ends without '/'.";

        throw new ArgumentException (msg, "uriPrefix");
      }

      var host = uriPrefix.IndexOf (':') + 3;

      if (host >= end) {
        var msg = "No host is specified.";

        throw new ArgumentException (msg, "uriPrefix");
      }

      if (uriPrefix[host] == ':') {
        var msg = "No host is specified.";

        throw new ArgumentException (msg, "uriPrefix");
      }

      var root = uriPrefix.IndexOf ('/', host, len - host);

      if (root == host) {
        var msg = "No host is specified.";

        throw new ArgumentException (msg, "uriPrefix");
      }

      if (uriPrefix[root - 1] == ':') {
        var msg = "No port is specified.";

        throw new ArgumentException (msg, "uriPrefix");
      }

      if (root == end - 1) {
        var msg = "No path is specified.";

        throw new ArgumentException (msg, "uriPrefix");
      }
    }

    /// <summary>
    /// Determines whether the current instance is equal to the specified
    /// <see cref="object"/> instance.
    /// </summary>
    /// <remarks>
    /// This method will be required to detect duplicates in any collection.
    /// </remarks>
    /// <param name="obj">
    ///   <para>
    ///   An <see cref="object"/> instance to compare to the current instance.
    ///   </para>
    ///   <para>
    ///   An reference to a <see cref="HttpListenerPrefix"/> instance.
    ///   </para>
    /// </param>
    /// <returns>
    /// <c>true</c> if the current instance and <paramref name="obj"/> have
    /// the same URI prefix; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals (object obj)
    {
      var pref = obj as HttpListenerPrefix;

      return pref != null && _prefix.Equals (pref._prefix);
    }

    /// <summary>
    /// Gets the hash code for the current instance.
    /// </summary>
    /// <remarks>
    /// This method will be required to detect duplicates in any collection.
    /// </remarks>
    /// <returns>
    /// An <see cref="int"/> that represents the hash code.
    /// </returns>
    public override int GetHashCode ()
    {
      return _prefix.GetHashCode ();
    }

    public override string ToString ()
    {
      return _prefix;
    }

    #endregion
  }
}

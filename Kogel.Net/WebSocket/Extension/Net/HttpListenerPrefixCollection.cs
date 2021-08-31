using System;
using System.Collections;
using System.Collections.Generic;

namespace Kogel.Net.WebSocket.Extension.Net
{
  /// <summary>
  /// Provides a collection used to store the URI prefixes for a instance of
  /// the <see cref="HttpListener"/> class.
  /// </summary>
  /// <remarks>
  /// The <see cref="HttpListener"/> instance responds to the request which has
  /// a requested URI that the prefixes most closely match.
  /// </remarks>
  public class HttpListenerPrefixCollection : ICollection<string>
  {
    #region Private Fields

    private HttpListener _listener;
    private List<string> _prefixes;

    #endregion

    #region Internal Constructors

    internal HttpListenerPrefixCollection (HttpListener listener)
    {
      _listener = listener;
      _prefixes = new List<string> ();
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the number of prefixes in the collection.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> that represents the number of prefixes.
    /// </value>
    public int Count {
      get {
        return _prefixes.Count;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the access to the collection is
    /// read-only.
    /// </summary>
    /// <value>
    /// Always returns <c>false</c>.
    /// </value>
    public bool IsReadOnly {
      get {
        return false;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the access to the collection is
    /// synchronized.
    /// </summary>
    /// <value>
    /// Always returns <c>false</c>.
    /// </value>
    public bool IsSynchronized {
      get {
        return false;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds the specified URI prefix to the collection.
    /// </summary>
    /// <param name="uriPrefix">
    ///   <para>
    ///   A <see cref="string"/> that specifies the URI prefix to add.
    ///   </para>
    ///   <para>
    ///   It must be a well-formed URI prefix with http or https scheme,
    ///   and must end with a '/'.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="uriPrefix"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="uriPrefix"/> is invalid.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="HttpListener"/> instance associated with this
    /// collection is closed.
    /// </exception>
    public void Add (string uriPrefix)
    {
      _listener.CheckDisposed ();

      HttpListenerPrefix.CheckPrefix (uriPrefix);

      if (_prefixes.Contains (uriPrefix))
        return;

      if (_listener.IsListening)
        EndPointManager.AddPrefix (uriPrefix, _listener);

      _prefixes.Add (uriPrefix);
    }

    /// <summary>
    /// Removes all URI prefixes from the collection.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="HttpListener"/> instance associated with this
    /// collection is closed.
    /// </exception>
    public void Clear ()
    {
      _listener.CheckDisposed ();

      if (_listener.IsListening)
        EndPointManager.RemoveListener (_listener);

      _prefixes.Clear ();
    }

    /// <summary>
    /// Returns a value indicating whether the collection contains the
    /// specified URI prefix.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the collection contains the URI prefix; otherwise,
    /// <c>false</c>.
    /// </returns>
    /// <param name="uriPrefix">
    /// A <see cref="string"/> that specifies the URI prefix to test.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="uriPrefix"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="HttpListener"/> instance associated with this
    /// collection is closed.
    /// </exception>
    public bool Contains (string uriPrefix)
    {
      _listener.CheckDisposed ();

      if (uriPrefix == null)
        throw new ArgumentNullException ("uriPrefix");

      return _prefixes.Contains (uriPrefix);
    }

    /// <summary>
    /// Copies the contents of the collection to the specified array of string.
    /// </summary>
    /// <param name="array">
    /// An array of <see cref="string"/> that specifies the destination of
    /// the URI prefix strings copied from the collection.
    /// </param>
    /// <param name="offset">
    /// An <see cref="int"/> that specifies the zero-based index in
    /// the array at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="offset"/> is less than zero.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The space from <paramref name="offset"/> to the end of
    /// <paramref name="array"/> is not enough to copy to.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="HttpListener"/> instance associated with this
    /// collection is closed.
    /// </exception>
    public void CopyTo (string[] array, int offset)
    {
      _listener.CheckDisposed ();

      _prefixes.CopyTo (array, offset);
    }

    /// <summary>
    /// Gets the enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerator{string}"/>
    /// instance that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<string> GetEnumerator ()
    {
      return _prefixes.GetEnumerator ();
    }

    /// <summary>
    /// Removes the specified URI prefix from the collection.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the URI prefix is successfully removed; otherwise,
    /// <c>false</c>.
    /// </returns>
    /// <param name="uriPrefix">
    /// A <see cref="string"/> that specifies the URI prefix to remove.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="uriPrefix"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="HttpListener"/> instance associated with this
    /// collection is closed.
    /// </exception>
    public bool Remove (string uriPrefix)
    {
      _listener.CheckDisposed ();

      if (uriPrefix == null)
        throw new ArgumentNullException ("uriPrefix");

      if (!_prefixes.Contains (uriPrefix))
        return false;

      if (_listener.IsListening)
        EndPointManager.RemovePrefix (uriPrefix, _listener);

      return _prefixes.Remove (uriPrefix);
    }

    #endregion

    #region Explicit Interface Implementations

    /// <summary>
    /// Gets the enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> instance that can be used to iterate
    /// through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator ()
    {
      return _prefixes.GetEnumerator ();
    }

    #endregion
  }
}

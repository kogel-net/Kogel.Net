using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Kogel.Net.WebSocket.Extension.Net
{
    internal sealed class EndPointManager
    {
        private static readonly Dictionary<IPEndPoint, EndPointListener> _endpoints;
        static EndPointManager()
        {
            _endpoints = new Dictionary<IPEndPoint, EndPointListener>();
        }

        private EndPointManager()
        {
        }

        private static void addPrefix(string uriPrefix, HttpListener listener)
        {
            var pref = new HttpListenerPrefix(uriPrefix, listener);

            var addr = convertToIPAddress(pref.Host);

            if (addr == null)
            {
                var msg = "The URI prefix includes an invalid host.";

                throw new HttpListenerException(87, msg);
            }

            if (!addr.IsLocal())
            {
                var msg = "The URI prefix includes an invalid host.";

                throw new HttpListenerException(87, msg);
            }

            int port;

            if (!Int32.TryParse(pref.Port, out port))
            {
                var msg = "The URI prefix includes an invalid port.";

                throw new HttpListenerException(87, msg);
            }

            if (!port.IsPortNumber())
            {
                var msg = "The URI prefix includes an invalid port.";

                throw new HttpListenerException(87, msg);
            }

            var path = pref.Path;

            if (path.IndexOf('%') != -1)
            {
                var msg = "The URI prefix includes an invalid path.";

                throw new HttpListenerException(87, msg);
            }

            if (path.IndexOf("//", StringComparison.Ordinal) != -1)
            {
                var msg = "The URI prefix includes an invalid path.";

                throw new HttpListenerException(87, msg);
            }

            var endpoint = new IPEndPoint(addr, port);

            EndPointListener lsnr;

            if (_endpoints.TryGetValue(endpoint, out lsnr))
            {
                if (lsnr.IsSecure ^ pref.IsSecure)
                {
                    var msg = "The URI prefix includes an invalid scheme.";

                    throw new HttpListenerException(87, msg);
                }
            }
            else
            {
                lsnr = new EndPointListener(
                         endpoint,
                         pref.IsSecure,
                         listener.CertificateFolderPath,
                         listener.SslConfiguration,
                         listener.ReuseAddress
                       );

                _endpoints.Add(endpoint, lsnr);
            }

            lsnr.AddPrefix(pref);
        }

        private static IPAddress convertToIPAddress(string hostname)
        {
            if (hostname == "*")
                return IPAddress.Any;

            if (hostname == "+")
                return IPAddress.Any;

            return hostname.ToIPAddress();
        }

        private static void removePrefix(string uriPrefix, HttpListener listener)
        {
            var pref = new HttpListenerPrefix(uriPrefix, listener);

            var addr = convertToIPAddress(pref.Host);

            if (addr == null)
                return;

            if (!addr.IsLocal())
                return;

            int port;

            if (!Int32.TryParse(pref.Port, out port))
                return;

            if (!port.IsPortNumber())
                return;

            var path = pref.Path;

            if (path.IndexOf('%') != -1)
                return;

            if (path.IndexOf("//", StringComparison.Ordinal) != -1)
                return;

            var endpoint = new IPEndPoint(addr, port);

            EndPointListener lsnr;

            if (!_endpoints.TryGetValue(endpoint, out lsnr))
                return;

            if (lsnr.IsSecure ^ pref.IsSecure)
                return;

            lsnr.RemovePrefix(pref);
        }

        internal static bool RemoveEndPoint(IPEndPoint endpoint)
        {
            lock (((ICollection)_endpoints).SyncRoot)
                return _endpoints.Remove(endpoint);
        }
        public static void AddListener(HttpListener listener)
        {
            var added = new List<string>();

            lock (((ICollection)_endpoints).SyncRoot)
            {
                try
                {
                    foreach (var pref in listener.Prefixes)
                    {
                        addPrefix(pref, listener);
                        added.Add(pref);
                    }
                }
                catch
                {
                    foreach (var pref in added)
                        removePrefix(pref, listener);

                    throw;
                }
            }
        }

        public static void AddPrefix(string uriPrefix, HttpListener listener)
        {
            lock (((ICollection)_endpoints).SyncRoot)
                addPrefix(uriPrefix, listener);
        }

        public static void RemoveListener(HttpListener listener)
        {
            lock (((ICollection)_endpoints).SyncRoot)
            {
                foreach (var pref in listener.Prefixes)
                    removePrefix(pref, listener);
            }
        }

        public static void RemovePrefix(string uriPrefix, HttpListener listener)
        {
            lock (((ICollection)_endpoints).SyncRoot)
                removePrefix(uriPrefix, listener);
        }
    }
}

//
// Server.cs
//
// Author:
//   Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2008 S&S Black Ltd.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Xml;

using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

using SsdpServer = Mono.Ssdp.Server;

namespace Mono.Upnp
{
    public class Server : IDisposable
    {
        static readonly Random random = new Random ();
        static WeakReference static_serializer = new WeakReference (null);
        
        readonly SsdpServer ssdp_server;
        readonly DataServer description_server;
        readonly Root root;
        
        public Server (Root root)
            : this (root, null, null)
        {
        }
        
        public Server (Root root, IPAddress localAddress)
            : this (root, localAddress, null)
        {
        }
        
        public Server (Root root, Uri url)
            : this (root, null, url)
        {
        }
        
        Server (Root root, IPAddress localAddress, Uri url)
        {
            if (root == null) throw new ArgumentNullException ("root");
            
            this.root = root;
            
            if (url != null) {
                foreach (var address in Dns.GetHostEntry (url.Host).AddressList) {
                    if (address.AddressFamily == AddressFamily.InterNetwork) {
                        localAddress = address;
                        break;
                    }
                }
                if (localAddress == null) {
                    throw new ArgumentException ("The URL is not local to this machine.", "url");
                }
            } else {
                if (localAddress == null) {
                    localAddress = Helper.GetLocalAddress ();
                }
                url = new Uri (string.Format ("http://{0}:{1}/upnp/", localAddress, random.Next (1024, 5000)));
            }
            
            ssdp_server = new SsdpServer (localAddress);
            var serializer = Helper.Get<XmlSerializer> (static_serializer);
            root.Initialize (serializer, url);
            description_server = new DataServer (serializer.GetBytes (root), url);
            Announce (url.ToString ());
        }
        
        public IPAddress LocalAddress {
            get { return ssdp_server.LocalAddress; }
        }
        
        public bool Started { get; private set; }

        public void Start ()
        {
            CheckDisposed ();
            if (Started) throw new InvalidOperationException ("The server is already started.");

            root.Start ();
            description_server.Start ();
            ssdp_server.Start ();
            Started = true;
        }

        public void Stop ()
        {
            CheckDisposed ();
            
            if (!Started) {
                return;
            }
            
            ssdp_server.Stop ();
            root.Stop ();
            description_server.Stop ();
            Started = false;
        }

        void Announce (string url)
        {
            ssdp_server.Announce ("upnp:rootdevice", root.RootDevice.Udn + "::upnp:rootdevice", url, false);
            AnnounceDevice (root.RootDevice, url);
        }

        void AnnounceDevice (Device device, string url)
        {
            ssdp_server.Announce (device.Udn, device.Udn, url, false);
            ssdp_server.Announce (device.Type.ToString (), string.Format ("{0}::{1}", device.Udn, device.Type), url, false);

            foreach (var child_device in device.Devices) {
                AnnounceDevice (child_device, url);
            }

            foreach (var service in device.Services) {
                AnnounceService (device, service, url);
            }
        }

        void AnnounceService (Device device, Service service, string url)
        {
            ssdp_server.Announce (service.Type.ToString (), string.Format ("{0}::{1}", device.Udn,service.Type), url, false);
        }
        
        void CheckDisposed ()
        {
            if (root == null) throw new ObjectDisposedException (ToString ());
        }

        public void Dispose ()
        {
            if (root != null) {
                Dispose (true);
                GC.SuppressFinalize (this);
            }
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposing) {
                Stop ();
                //root_device.Dispose ();
                if (description_server != null) {
                    description_server.Dispose ();
                    ssdp_server.Dispose ();
                }
            }
        }
    }
}

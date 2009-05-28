//
// Icon.cs
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
using System.Net;

using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp
{
    [XmlType ("icon", Protocol.DeviceUrn)]
    public class Icon : Description
    {
        string filename;
        byte[] data;
        
        protected Icon ()
        {
        }

        protected internal Icon (Deserializer deserializer)
            : base (deserializer)
        {
        }
        
        public Icon (int width, int height, int depth, string mimetype, byte[] data)
             : this (width, height, depth, mimetype)
        {
            if (data == null) throw new ArgumentNullException ("data");
            
            this.data = data;
        }

        public Icon (int width, int height, int depth, string format, string filename)
            : this (width, height, depth, format)
        {
            if (filename == null) throw new ArgumentNullException ("filename");
            //if (!File.Exists (filename)) throw new ArgumentException ("The specified filename does not exist on the file system.", "path");
            
            this.filename = filename;
        }
        
        protected Icon (int width, int height, int depth, string mimetype)
        {
            if (mimetype == null) throw new ArgumentNullException ("mimetype");
            
            Width = width;
            Height = height;
            Depth = depth;
            MimeType = mimetype;
        }

        [XmlElement ("url", Protocol.DeviceUrn)]
        protected virtual string UrlFragment {
            get { return CollapseUrl (Url); }
            set { Url = ExpandUrl (value); }
        }
        
        public virtual Uri Url { get; protected set; }
        
        [XmlElement ("mimetype", Protocol.DeviceUrn)]
        public virtual string MimeType { get; protected set; }
        
        [XmlElement ("width", Protocol.DeviceUrn)]
        public virtual int Width { get; protected set; }
        
        [XmlElement ("height", Protocol.DeviceUrn)]
        public virtual int Height { get; protected set; }
        
        [XmlElement ("depth", Protocol.DeviceUrn)]
        public virtual int Depth { get; protected set; }
        
        protected internal virtual void Initialize (Root root, Uri iconUrl)
        {
            Initialize (root);
            Url = iconUrl;
        }

        public virtual byte[] GetData ()
        {
            if (data == null) {
                try {
                    var request = (HttpWebRequest)WebRequest.Create (Url);
                    using (var response = Helper.GetResponse (request)) {
                        data = new byte[response.ContentLength];
                        using (var stream = response.GetResponseStream ()) {
                            stream.Read (data, 0, (int)response.ContentLength);
                        }
                    }
                } catch (WebException e) {
                    if (e.Status == WebExceptionStatus.Timeout) {
                        Deserializer.CheckDisposed ();
                    }
                    throw e;
                }
            }
            var copy = new byte [data.Length];
            Array.Copy (data, copy, data.Length);
            return copy;
        }

        protected override void DeserializeElement (XmlDeserializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoDeserializeElement (this);
        }
        
        protected override void SerializeSelfAndMembers (XmlSerializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoSerializeObjectAndMembers (this);
        }
        
        protected override void SerializeMembersOnly (XmlSerializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoSerializeMembersOnly (this);
        }
        
        public override string ToString ()
        {
            return string.Format ("Icon {{ {0}, {1}x{2}x{3}, {4} }}", MimeType, Width, Height, Depth, Url);
        }
    }
}

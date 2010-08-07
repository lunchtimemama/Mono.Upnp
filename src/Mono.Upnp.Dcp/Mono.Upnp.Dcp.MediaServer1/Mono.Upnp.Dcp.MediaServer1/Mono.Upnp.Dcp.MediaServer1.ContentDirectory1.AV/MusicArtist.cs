// 
// MusicArtist.cs
//  
// Author:
//       Scott Peterson <lunchtimemama@gmail.com>
// 
// Copyright (c) 2009 Scott Peterson
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.AV
{
    public class MusicArtist : Person
    {
        protected MusicArtist ()
        {
            Genres = new List<string> ();
        }
        
        public MusicArtist (string id, string parentId, MusicArtistOptions options)
            : base (id, parentId, options)
        {
            ArtistDiscographyUri = options.ArtistDiscographyUri;
            Genres = Helper.MakeReadOnlyCopy (options.Genres);
        }

        protected void CopyToOptions (MusicArtistOptions options)
        {
            base.CopyToOptions (options);

            options.ArtistDiscographyUri = ArtistDiscographyUri;
            options.Genres = new List<string> (Genres);
        }

        public new MusicArtistOptions GetOptions ()
        {
            var options = new MusicArtistOptions ();
            CopyToOptions (options);
            return options;
        }
        
        [XmlArrayItem ("genre", Schemas.UpnpSchema)]
        public virtual IList<string> Genres { get; private set; }
        
        [XmlElement ("artistDiscographyURI", Schemas.UpnpSchema, OmitIfNull = true)]
        public virtual Uri ArtistDiscographyUri { get; protected set; }

        protected override void Deserialize (XmlDeserializationContext context)
        {
            base.Deserialize (context);

            Genres = new ReadOnlyCollection<string> (Genres);
        }
    
        protected override void DeserializeElement (XmlDeserializationContext context)
        {
            context.AutoDeserializeElement (this);
        }

        protected override void SerializeMembersOnly (XmlSerializationContext context)
        {
            context.AutoSerializeMembersOnly (this);
        }
    }
}

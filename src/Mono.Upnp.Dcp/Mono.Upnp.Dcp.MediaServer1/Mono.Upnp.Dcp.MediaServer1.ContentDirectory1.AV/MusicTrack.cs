// 
// MusicTrack.cs
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
using System.Xml;

namespace Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Av
{
    public class MusicTrack : AudioItem
    {
        readonly List<PersonWithRole> artist_list = new List<PersonWithRole> ();
        readonly ReadOnlyCollection<PersonWithRole> artists;
        readonly List<string> contributor_list = new List<string> ();
        readonly ReadOnlyCollection<string> contributors;
         readonly List<string> album_list = new List<string>();
        readonly ReadOnlyCollection<string> albums;
        readonly List<string> playlist_list = new List<string> ();
        readonly ReadOnlyCollection<string> playlists;
        
        protected MusicTrack ()
        {
            artists = artist_list.AsReadOnly ();
            contributors = contributor_list.AsReadOnly ();
            albums = album_list.AsReadOnly ();
            playlists = playlist_list.AsReadOnly ();
        }
        
        public ReadOnlyCollection<PersonWithRole> Artists { get { return artists; } }
        public ReadOnlyCollection<string> Albums { get { return albums; } }
        public int? OriginalTrackNumber { get; private set; }
        public ReadOnlyCollection<string> Playlists { get { return playlists; } }
        public string StorageMedium { get; private set; }
        public ReadOnlyCollection<string> Contributors { get { return contributors; } }
        public string Date { get; private set; }
        public Uri LyricsUri { get; private set; }
        
        protected override void DeserializePropertyElement (XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException ("reader");
            
            if (reader.NamespaceURI == Schemas.UpnpSchema) {
                switch (reader.LocalName) {
                case "artist":
                    artist_list.Add (PersonWithRole.Deserialize (reader));
                    break;
                case "album":
                    album_list.Add (reader.ReadString ());
                    break;
                case "playlist":
                    playlist_list.Add (reader.ReadString ());
                    break;
                case "originalTrackNumber":
                    OriginalTrackNumber = reader.ReadElementContentAsInt ();
                    break;
                default:
                    base.DeserializePropertyElement (reader);
                    break;
                }
             } else if (reader.NamespaceURI == Schemas.DublinCoreSchema) {
                switch (reader.LocalName) {
                case "contributor":
                    contributor_list.Add (reader.ReadString ());
                    break;
                case "date":
                    Date = reader.ReadString ();
                    break;
                default:
                    base.DeserializePropertyElement (reader);
                    break;
                }
            } else {
                base.DeserializePropertyElement (reader);
            }
        }
    }
}
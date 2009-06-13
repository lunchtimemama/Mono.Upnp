// 
// DeserializationInfo.cs
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

namespace Mono.Upnp.Xml.Internal
{
    delegate object Deserializer (XmlDeserializationContext context);
    delegate void ObjectDeserializer (object obj, XmlDeserializationContext context);
    
    class DeserializationInfo
    {
        readonly DeserializationCompiler compiler;
        
        Deserializer deserializer;
        ObjectDeserializer auto_deserializer;
        ObjectDeserializer attribute_auto_deserializer;
        ObjectDeserializer element_auto_deserializer;
        
        public DeserializationInfo (XmlDeserializer xmlDeserializer, Type type)
        {
            compiler = new DeserializationCompiler (xmlDeserializer, this, type);
        }
        
        public Deserializer Deserializer {
            get {
                if (this.deserializer == null) {
                    Deserializer deserializer = null;
                    this.deserializer = context => deserializer (context);
                    deserializer = compiler.CreateDeserializer ();
                    this.deserializer = deserializer;
                }
                return this.deserializer;
            }
        }
        
        public ObjectDeserializer AutoDeserializer {
            get {
                if (auto_deserializer == null) {
                    auto_deserializer = compiler.CreateAutoDeserializer ();
                }
                return auto_deserializer;
            }
        }
        
        public ObjectDeserializer AttributeAutoDeserializer {
            get {
                if (attribute_auto_deserializer == null) {
                    attribute_auto_deserializer = compiler.CreateAttributeAutoDeserializer ();
                }
                return attribute_auto_deserializer;
            }
        }
        
        public ObjectDeserializer ElementAutoDeserializer {
            get {
                if (element_auto_deserializer == null) {
                    element_auto_deserializer = compiler.CreateElementAutoDeserializer ();
                }
                return element_auto_deserializer;
            }
        }
    }
}
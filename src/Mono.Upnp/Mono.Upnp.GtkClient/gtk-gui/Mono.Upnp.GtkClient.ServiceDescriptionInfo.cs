// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Mono.Upnp.GtkClient {
    
    
    public partial class ServiceDescriptionInfo {
        
        private Gtk.Table table2;
        
        private Gtk.Label controlUrl;
        
        private Gtk.Label eventUrl;
        
        private Gtk.Label label10;
        
        private Gtk.Label label11;
        
        private Gtk.Label label7;
        
        private Gtk.Label label8;
        
        private Gtk.Label label9;
        
        private Gtk.Label scpdUrl;
        
        private Gtk.Label serviceId;
        
        private Gtk.Label serviceType;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget Mono.Upnp.GtkClient.ServiceDescriptionInfo
            Stetic.BinContainer.Attach(this);
            this.Name = "Mono.Upnp.GtkClient.ServiceDescriptionInfo";
            // Container child Mono.Upnp.GtkClient.ServiceDescriptionInfo.Gtk.Container+ContainerChild
            this.table2 = new Gtk.Table(((uint)(5)), ((uint)(2)), false);
            this.table2.Name = "table2";
            this.table2.RowSpacing = ((uint)(10));
            this.table2.ColumnSpacing = ((uint)(10));
            this.table2.BorderWidth = ((uint)(10));
            // Container child table2.Gtk.Table+TableChild
            this.controlUrl = new Gtk.Label();
            this.controlUrl.Name = "controlUrl";
            this.controlUrl.Xalign = 0F;
            this.controlUrl.LabelProp = "controlUrl";
            this.controlUrl.Selectable = true;
            this.table2.Add(this.controlUrl);
            Gtk.Table.TableChild w1 = ((Gtk.Table.TableChild)(this.table2[this.controlUrl]));
            w1.TopAttach = ((uint)(3));
            w1.BottomAttach = ((uint)(4));
            w1.LeftAttach = ((uint)(1));
            w1.RightAttach = ((uint)(2));
            w1.XOptions = ((Gtk.AttachOptions)(4));
            w1.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.eventUrl = new Gtk.Label();
            this.eventUrl.Name = "eventUrl";
            this.eventUrl.Xalign = 0F;
            this.eventUrl.LabelProp = "eventUrl";
            this.eventUrl.Selectable = true;
            this.table2.Add(this.eventUrl);
            Gtk.Table.TableChild w2 = ((Gtk.Table.TableChild)(this.table2[this.eventUrl]));
            w2.TopAttach = ((uint)(4));
            w2.BottomAttach = ((uint)(5));
            w2.LeftAttach = ((uint)(1));
            w2.RightAttach = ((uint)(2));
            w2.XOptions = ((Gtk.AttachOptions)(4));
            w2.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label10 = new Gtk.Label();
            this.label10.Name = "label10";
            this.label10.Xalign = 1F;
            this.label10.LabelProp = Mono.Unix.Catalog.GetString("<b>Control URL</b>");
            this.label10.UseMarkup = true;
            this.table2.Add(this.label10);
            Gtk.Table.TableChild w3 = ((Gtk.Table.TableChild)(this.table2[this.label10]));
            w3.TopAttach = ((uint)(3));
            w3.BottomAttach = ((uint)(4));
            w3.XOptions = ((Gtk.AttachOptions)(4));
            w3.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label11 = new Gtk.Label();
            this.label11.Name = "label11";
            this.label11.Xalign = 1F;
            this.label11.LabelProp = Mono.Unix.Catalog.GetString("<b>Event URL</b>");
            this.label11.UseMarkup = true;
            this.table2.Add(this.label11);
            Gtk.Table.TableChild w4 = ((Gtk.Table.TableChild)(this.table2[this.label11]));
            w4.TopAttach = ((uint)(4));
            w4.BottomAttach = ((uint)(5));
            w4.XOptions = ((Gtk.AttachOptions)(4));
            w4.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label7 = new Gtk.Label();
            this.label7.Name = "label7";
            this.label7.Xalign = 1F;
            this.label7.LabelProp = Mono.Unix.Catalog.GetString("<b>ServiceType</b>");
            this.label7.UseMarkup = true;
            this.table2.Add(this.label7);
            Gtk.Table.TableChild w5 = ((Gtk.Table.TableChild)(this.table2[this.label7]));
            w5.XOptions = ((Gtk.AttachOptions)(4));
            w5.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label8 = new Gtk.Label();
            this.label8.Name = "label8";
            this.label8.Xalign = 1F;
            this.label8.LabelProp = Mono.Unix.Catalog.GetString("<b>Service ID</b>");
            this.label8.UseMarkup = true;
            this.table2.Add(this.label8);
            Gtk.Table.TableChild w6 = ((Gtk.Table.TableChild)(this.table2[this.label8]));
            w6.TopAttach = ((uint)(1));
            w6.BottomAttach = ((uint)(2));
            w6.XOptions = ((Gtk.AttachOptions)(4));
            w6.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label9 = new Gtk.Label();
            this.label9.Name = "label9";
            this.label9.Xalign = 1F;
            this.label9.LabelProp = Mono.Unix.Catalog.GetString("<b>SCPD URL</b>");
            this.label9.UseMarkup = true;
            this.table2.Add(this.label9);
            Gtk.Table.TableChild w7 = ((Gtk.Table.TableChild)(this.table2[this.label9]));
            w7.TopAttach = ((uint)(2));
            w7.BottomAttach = ((uint)(3));
            w7.XOptions = ((Gtk.AttachOptions)(4));
            w7.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.scpdUrl = new Gtk.Label();
            this.scpdUrl.Name = "scpdUrl";
            this.scpdUrl.Xalign = 0F;
            this.scpdUrl.LabelProp = "scpdUrl";
            this.scpdUrl.Selectable = true;
            this.table2.Add(this.scpdUrl);
            Gtk.Table.TableChild w8 = ((Gtk.Table.TableChild)(this.table2[this.scpdUrl]));
            w8.TopAttach = ((uint)(2));
            w8.BottomAttach = ((uint)(3));
            w8.LeftAttach = ((uint)(1));
            w8.RightAttach = ((uint)(2));
            w8.XOptions = ((Gtk.AttachOptions)(4));
            w8.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.serviceId = new Gtk.Label();
            this.serviceId.Name = "serviceId";
            this.serviceId.Xalign = 0F;
            this.serviceId.LabelProp = "serviceId";
            this.serviceId.Selectable = true;
            this.table2.Add(this.serviceId);
            Gtk.Table.TableChild w9 = ((Gtk.Table.TableChild)(this.table2[this.serviceId]));
            w9.TopAttach = ((uint)(1));
            w9.BottomAttach = ((uint)(2));
            w9.LeftAttach = ((uint)(1));
            w9.RightAttach = ((uint)(2));
            w9.XOptions = ((Gtk.AttachOptions)(4));
            w9.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.serviceType = new Gtk.Label();
            this.serviceType.Name = "serviceType";
            this.serviceType.Xalign = 0F;
            this.serviceType.LabelProp = "serviceType";
            this.serviceType.Selectable = true;
            this.table2.Add(this.serviceType);
            Gtk.Table.TableChild w10 = ((Gtk.Table.TableChild)(this.table2[this.serviceType]));
            w10.LeftAttach = ((uint)(1));
            w10.RightAttach = ((uint)(2));
            w10.XOptions = ((Gtk.AttachOptions)(4));
            w10.YOptions = ((Gtk.AttachOptions)(4));
            this.Add(this.table2);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Hide();
        }
    }
}

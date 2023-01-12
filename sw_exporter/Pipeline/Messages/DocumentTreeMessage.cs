using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
using sw_exporter.Pipeline.Messages.Base;

namespace sw_exporter.Pipeline.Messages {
    public class DocumentTreeMessage : Message {
        public ModelDoc2 Doc { get; set; }
        public List<DocumentTreeMessage> Children { get; set; }
    }
}
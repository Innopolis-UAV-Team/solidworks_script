using System.Collections.Generic;
using sw_exporter.Pipeline.Messages.Base;

namespace sw_exporter.Pipeline.Messages {
    public class CleanFlattenedDataMessage : Message {
        public string DocumentName { get; set; }
        public float Mass { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
    
    public class ItemCountFlattenedDataMessage : CleanFlattenedDataMessage {
        public int Count { get; set; }
    }
}
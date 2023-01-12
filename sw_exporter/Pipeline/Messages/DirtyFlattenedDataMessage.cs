using System.Collections.Generic;
using sw_exporter.Pipeline.Messages.Base;

namespace sw_exporter.Pipeline.Messages {
    public class DirtyFlattenedDataMessage : Message {
        public string DocumentName { get; set; }
        public float Mass { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
        public Dictionary<string, string> ConfigSpecificProperties { get; set; }
    }
}
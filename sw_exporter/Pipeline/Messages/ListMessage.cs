using System.Collections.Generic;
using sw_exporter.Pipeline.Messages.Base;

namespace sw_exporter.Pipeline.Messages {
    public class ListMessage<T> : Message {
        public List<T> Elements { get; set; }
    }
}
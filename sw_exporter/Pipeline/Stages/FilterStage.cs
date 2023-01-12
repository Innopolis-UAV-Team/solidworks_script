using System.Collections.Generic;
using System.Linq;
using sw_exporter.Pipeline.Messages;
using sw_exporter.Pipeline.Messages.Base;
using sw_exporter.Pipeline.Stages.Base;

namespace sw_exporter.Pipeline.Stages {
    public class FilterStage : Stage {
        //ToDo remove this stub
        public override Message Process(Message data) {
            return new ListMessage<CleanFlattenedDataMessage> {
                Elements=(data as ListMessage<DirtyFlattenedDataMessage>)?.Elements.Select(elem => new CleanFlattenedDataMessage() {
                    DocumentName = elem.DocumentName,
                    Mass = elem.Mass,
                    Properties = elem.CustomProperties
            }).ToList()};
        }
    }
}
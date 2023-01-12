using System.Collections.Generic;
using System.Linq;
using sw_exporter.Pipeline.Messages;
using sw_exporter.Pipeline.Messages.Base;
using sw_exporter.Pipeline.Stages.Base;

namespace sw_exporter.Pipeline.Stages {
    public class FlattenStage : Stage {
        public override Message Process(Message data) {
            var result = new List<DirtyFlattenedDataMessage>();
            FlattenRecurse(data as DirtyDataMessage, result);
            return new ListMessage<DirtyFlattenedDataMessage>(){Elements=result};
        }

        private void FlattenRecurse(DirtyDataMessage subTree, List<DirtyFlattenedDataMessage> flattenedMessages) {
            flattenedMessages.Add(new DirtyFlattenedDataMessage {
                ConfigSpecificProperties = subTree.ConfigSpecificProperties,
                CustomProperties = subTree.CustomProperties,
                DocumentName = subTree.DocumentName,
                Mass = subTree.Mass
            });
            foreach (var subTreeChild in subTree.Children) {
                FlattenRecurse(subTreeChild, flattenedMessages);
            }
        }
    }
}
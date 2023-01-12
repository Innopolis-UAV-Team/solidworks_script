using System.Collections.Generic;
using System.Linq;
using sw_exporter.Pipeline.Messages;
using sw_exporter.Pipeline.Messages.Base;
using sw_exporter.Pipeline.Stages.Base;

namespace sw_exporter.Pipeline.Stages {
    public class LoaderStage: Stage {
        public override Message Process(Message data) {
            DirtyDataMessage container = new DirtyDataMessage() {
                DocumentName = "",
                Mass = 0,
                ConfigSpecificProperties = new Dictionary<string, string>(),
                CustomProperties = new Dictionary<string, string>(),
                Children = new List<DirtyDataMessage>()
            };
            RecurseProcess(data as DocumentTreeMessage, container);
            return container;
        }

        private void RecurseProcess(DocumentTreeMessage subTree, DirtyDataMessage container) {
            var customPropertyManager = subTree.Doc.Extension.CustomPropertyManager[""];

            object propNames = null;
            object propTypes = null;
            object propValues = null;
            object resolved = null;
            object propLink = null;
            customPropertyManager.GetAll3(ref propNames, ref propTypes, ref propValues, ref resolved, ref propLink);
            var customPropertyDict = new Dictionary<string, string>();
            if (propNames != null) {
                customPropertyDict = ((string[])propNames)
                    .Zip((string[])propValues, (k, v) => new { k, v })
                    .ToDictionary(x => x.k, x => x.v);
            }

            subTree.Doc.ConfigurationManager.GetConfigurationParams(subTree.Doc.ConfigurationManager.ActiveConfiguration.Name,
                out var configParamKeys, out var configParamValues);
            var configPropertyDict = new Dictionary<string, string>();

            if (configParamKeys != null) {
                configPropertyDict = ((string[])configParamKeys)
                    .Zip((string[])configParamValues, (k, v) => new { k, v })
                    .GroupBy(item => item.k)
                    .ToDictionary(x => x.Key, x => x.First().v);
            }
            
            var name = subTree.Doc.GetPathName();
            container.DocumentName = name;
            container.CustomProperties = customPropertyDict;
            container.ConfigSpecificProperties = configPropertyDict;
            container.Children = new List<DirtyDataMessage>();

            foreach (var subTreeChild in subTree.Children) {
                var child = new DirtyDataMessage() {
                    DocumentName = "",
                    Children = new List<DirtyDataMessage>(),
                    CustomProperties = new Dictionary<string, string>(),
                    ConfigSpecificProperties = new Dictionary<string, string>()
                };
                container.Children.Add(child);
                RecurseProcess(subTreeChild, child);
            }
        }
        
    }
}
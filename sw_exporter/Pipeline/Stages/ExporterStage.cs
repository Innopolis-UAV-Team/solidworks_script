using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using sw_exporter.Pipeline.Messages;
using sw_exporter.Pipeline.Messages.Base;
using sw_exporter.Pipeline.Stages.Base;

namespace sw_exporter.Pipeline.Stages {
    public class ExporterStage : Stage {
        public string Path;
        public ExporterStage(string path) : base(){
            Path = path;
        }
        public override Message Process(Message tree) {
            var options = new JsonSerializerOptions {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            var result = JsonSerializer.Serialize((tree as ListMessage<CleanFlattenedDataMessage>)?.Elements, options);
            Console.WriteLine(result);
            using (var w = new StreamWriter(Path)) {
                w.Write(result);
            }
            return null;
        }
    }
}
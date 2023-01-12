using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CommandLine;


namespace sw_exporter {
    
    internal static class Entrypoint {
        private static void Main(string[] args) {
            CliConfig opt = null;
            Parser.Default.ParseArguments<CliConfig>(args).WithParsed(o => opt = o);
            var options = new JsonSerializerOptions {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            
            string output = JsonSerializer.Serialize(opt, options);
            Console.WriteLine($"Invoking the script with options: {output}");

            using var swApp = new SwInterop.SwInterface(opt);
            swApp.LoadDocument(opt.Asm);
            var bom = swApp.GetBomData();
            using var w = new StreamWriter(opt.OutFile);
            w.Write(JsonSerializer.Serialize(bom, options));
        }
    }
}
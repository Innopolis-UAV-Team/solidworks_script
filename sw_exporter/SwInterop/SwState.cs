using System;
using System.IO;
using System.Text.Json;

namespace sw_exporter.SwInterface {
    public class SwState {
        public SwStateJson JsonData { get; }
        public string Path { get; set; }
        
        public SwState(string path = "./state.json") {
            Path = path;
            try {
                using var r = new StreamReader(Path);
                var jsonStr = r.ReadToEnd();
                JsonData = JsonSerializer.Deserialize<SwStateJson>(jsonStr);
            }
            catch (FileNotFoundException) {
                JsonData = new SwStateJson {
                    State = "stopped",
                    Moniker = "",
                };
                try {
                    Save();
                } catch (DirectoryNotFoundException) {
                    Console.WriteLine($"Exiting due to non-existent path {Path}");
                    Environment.Exit(-1);
                }
            }
        }

        public void Save() {
            using var w = new StreamWriter(Path);
            w.Write(JsonSerializer.Serialize(JsonData));
        }
    }

    public class SwStateJson {
        //running or stopped
        public string State { get; set; }
        //moniker name
        public string Moniker { get; set; }
    }
}
using CommandLine;

namespace sw_exporter {
    public class CliConfig {
        [Option('f', "file", Required = true, 
            HelpText = "Path to Solidworks Assembly to open")]
        public string Asm { get; set; }
        
        [Option('e', "executable", Required = false, 
            HelpText = "Set the path to Solidworks executable", 
            Default = @"C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\SLDWORKS.exe")]
        public string Executable { get; set; }
        
        [Option('s', "state", Required = false, 
            HelpText = "Set the path to state path (used to determine whether SW is running)",
            Default = @"./state.json")]
        public string StatePath { get; set; }
        
        [Option('b', "background", Required = false, HelpText = "Set to true to hide SW window and start in background",
            Default = false)]
        public bool IsBackground { get; set; }
        
        [Option('t', "timeout", Required = false, HelpText = "Set timeout (sec) for SW connection attempt",
            Default = 40)]
        public int Timeout { get; set; }
        
        [Option('c', "create", Required = false, HelpText = "Set to true to always recreate a SW instance",
            Default = false)]
        public bool AlwaysCreate { get; set; }
        
        [Option('o', "out", Required = true, HelpText = "Output filename",
            Default = false)]
        public string OutFile { get; set; }
    }
}
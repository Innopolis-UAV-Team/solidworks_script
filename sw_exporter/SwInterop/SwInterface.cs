using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using sw_exporter.Pipeline.Messages;
using sw_exporter.SwInterface;
using Environment = System.Environment;

namespace sw_exporter.SwInterop {
    public class SwInterface : IDisposable {
        [DllImport("ole32.dll")] 
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public List<ModelDoc2> OpenDocs;
        public ISldWorks App;
        public CliConfig Config;
        public ModelDoc2 ActiveDoc;
        
        public SwInterface(CliConfig config) {
            Config = config;
            OpenDocs = new List<ModelDoc2>();
            var state = new sw_exporter.SwInterface.SwState(Config.StatePath);
            try {
                foreach (var tmp in GetRunningProcesses()) {
                    Console.WriteLine(tmp);
                }
                App = FetchSwAppBackground(Config.Executable, state, Config.IsBackground, Config.Timeout, Config.AlwaysCreate);
            } catch (Exception ex) {
                Console.WriteLine($"Unable to connect to SW process: {ex.Message} {ex.StackTrace}");
                Environment.Exit(-1);
            }
        }

        private ISldWorks FetchSwAppBackground(string appPath, sw_exporter.SwInterface.SwState state, bool isBackground, int timeoutSec, bool alwaysCreateInstance) {
            var timeout = TimeSpan.FromSeconds(timeoutSec);
            var startTime = DateTime.Now;
            
            //First check if we can reuse previously launched SW instance
            bool needsCreation;
            if (!alwaysCreateInstance) {
                needsCreation = true;
                foreach (var runningMonikerName in GetRunningProcesses()) {
                    needsCreation &= (runningMonikerName != state.JsonData.Moniker);
                }
            }
            else {
                needsCreation = true;
            }

            string moniker;
            var isLoaded = false;
            var onIdleFunc = new DSldWorksEvents_OnIdleNotifyEventHandler(() => {
                isLoaded = true;
                return 0;
            });

            if (needsCreation) {
                Console.WriteLine("Creating a new instance of Solidworks");
                var prcInfo = new ProcessStartInfo() {
                    FileName = appPath,
                    Arguments = isBackground? "/r /b" : "", //no splash screen
                    CreateNoWindow = isBackground,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                var prc = Process.Start(prcInfo);
                moniker = "SolidWorks_PID_" + prc.Id;

                state.JsonData.Moniker = moniker;
                state.JsonData.State = "running";
                
                if (!alwaysCreateInstance) {
                    // No need to save state because the app will be disposed of
                    state.Save();
                }
            }
            else {
                Console.WriteLine("Reusing previously created instance of Solidworks");
                moniker = state.JsonData.Moniker;
            }

            try {
                while (!isLoaded) {
                    if (DateTime.Now - startTime > timeout) {
                        throw new TimeoutException();
                    }
                    if (App == null) {
                        App = GetSwAppFromMonikerName(moniker);
                        if (App != null) {
                            ((SldWorks)App).OnIdleNotify += onIdleFunc;
                        }
                    }
                    Thread.Sleep(100);
                }

                if (App != null && isBackground) {
                    ShowWindow(new IntPtr(App.IFrameObject().GetHWnd()), 0);
                }
            }
            finally {
                if (App != null) {
                    ((SldWorks)App).OnIdleNotify -= onIdleFunc;
                }
            }

            return App;
        }

        private static ISldWorks GetSwAppFromMonikerName(string monikerName) {
            using (var rotManager = new RotExplorer()) {
                object app = null;
                var moniker = new IMoniker[1];
                while (rotManager.Monikers.Next(1, moniker, IntPtr.Zero) == 0) {
                    var curMoniker = moniker.First();
                    string name = null;

                    if (curMoniker != null) {
                        try {
                            curMoniker.GetDisplayName(rotManager.Context, null, out name);
                        } catch (UnauthorizedAccessException) {}
                    }

                    if (!string.Equals(monikerName,
                            name, StringComparison.CurrentCultureIgnoreCase)) continue;
                    rotManager.Rot.GetObject(curMoniker, out app);
                }
                return app as ISldWorks;
            }
        }
        
        private List<string> GetRunningProcesses() {
            var processes = new List<string>();
            using (var rotManager = new RotExplorer()) {
                var moniker = new IMoniker[1];
                while (rotManager.Monikers.Next(1, moniker, IntPtr.Zero) == 0) {
                    var curMoniker = moniker.First();
                    if (curMoniker == null) continue;
                    try {
                        curMoniker.GetDisplayName(rotManager.Context, null, out var name);
                        processes.Add(name);
                    } catch (UnauthorizedAccessException) { }
                }
            }
            return processes;
        }

        public ModelDoc2 LoadDocument(string path) {
            var err = 0;
            var warn = 0;
            App.CommandInProgress = true;
            var document = App.OpenDoc6(path, (int)swDocumentTypes_e.swDocASSEMBLY, 
                (int)swOpenDocOptions_e.swOpenDocOptions_LoadLightweight, "", ref err, ref warn);

            if (err != 0) {
                throw new System.InvalidOperationException("Failed to open a document");
            }
            
            Console.WriteLine("Errors " + err + " Warnings " + warn + " File " + document);
            Console.WriteLine("App loaded");
            OpenDocs.Add(document);
            ActiveDoc = document;
            return document;
        }

        public void UnloadDocument(ModelDoc2 document) {
            OpenDocs.Remove(document);
            App.QuitDoc(document.GetTitle());
        }

        public DocumentTreeMessage TraverseDesignTree(ModelDoc2 document) {
            var container = new DocumentTreeMessage {
                Doc=null,
                Children = new List<DocumentTreeMessage>()
            };
            TraverseDesignTree(document, container);
            return container;
        }

        public List<T> ApplyFlat<T>(Func<DocumentTreeMessage, T> fun, DocumentTreeMessage designTree) {
            var result = new List<T>();
            ApplyFlatRecurse(fun, result, designTree);
            return result;
        }

        private void ApplyFlatRecurse<T>(Func<DocumentTreeMessage, T> fun, List<T> container, DocumentTreeMessage doc) {
            container.Add(fun(doc));
            foreach (var docChild in doc.Children) {
                ApplyFlatRecurse(fun, container, docChild);
            }
        }

        private void TraverseDesignTree(ModelDoc2 document, DocumentTreeMessage container) {
            container.Doc = document;
            TraverseFeatureTree(document.FirstFeature() as Feature, container);
        }
        
        private void TraverseDesignTree(IComponent2 document, DocumentTreeMessage container) {
            container.Doc = document.GetModelDoc2() as ModelDoc2;
            TraverseFeatureTree(document.FirstFeature(), container);
        }
        //Todo move doc loading to separate function
        private void TraverseFeatureTree(Feature feat, DocumentTreeMessage container) {
            while (feat != null) {
                if (feat.GetSpecificFeature2() is Component2 comp && !comp.IsSuppressed()) {
                    Console.WriteLine(feat.Name);
                    var child = new DocumentTreeMessage {
                        Doc = null,
                        Children = new List<DocumentTreeMessage>()
                    };
                    container.Children.Add(child);
                    TraverseDesignTree(comp, child);
                }
                feat = feat.GetNextFeature() as Feature;
            }
        }

        public List<List<Dictionary<string, string>>> GetBomData(ModelDoc2 doc = null) {
            if (doc == null && ActiveDoc == null)
                throw new InvalidOperationException("Open a doc or provide one as an argument to this call");
            if (doc == null) doc = ActiveDoc;
            
            var t = GetFeature(doc, "TableFolder");
            if (t == null) return new List<List<Dictionary<string, string>>>();
            var bom = GetFeature(t, "BomFeat").GetSpecificFeature2() as BomFeature;
            var bomTables = bom?.GetTableAnnotations() as object[];

            var listBom = (from TableAnnotation x  in bomTables select 
                (from i in Enumerable.Range(0, x.RowCount) select 
                    (from j in Enumerable.Range(0, x.ColumnCount) select x.Text[i, j]).ToList()
                    ).ToList()
                ).ToList();
            var re = new Regex("^<.*>");
            return (from x in listBom select 
                (from list in x.Skip(1) select x
                    .First()
                    .Zip(list, (k, v) => new {k, v})
                    .ToDictionary(p => re.Replace(p.k, ""), p => p.v)).ToList()
                ).ToList();
        }

        public void Dispose() {
            App.CommandInProgress = false;
            for (var i = 0; i < OpenDocs.Count; i++) {
                UnloadDocument(OpenDocs[i]);
            }
            GC.SuppressFinalize(this);
        }
        ~SwInterface() {
            Dispose();
        }
        
        public static Feature GetFeature(ModelDoc2 doc, string name) {
            var feature = doc.FirstFeature() as Feature;
            while (feature != null) {
                feature = feature.GetNextFeature() as Feature;
                if (feature?.GetTypeName2() == name) {
                    return feature;
                }
            }
            return default;
        }

        public static Feature GetFeature(Feature feature, string name) {
            var nextFeature = feature.GetFirstSubFeature() as Feature;
            while (nextFeature != null) {
                if (nextFeature?.GetTypeName2() == name) {
                    return nextFeature;
                }
                nextFeature = feature.GetNextSubFeature() as Feature;
            }
            return default;
        }
    }
}
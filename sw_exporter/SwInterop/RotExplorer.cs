using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace sw_exporter.SwInterface {
    public class RotExplorer: IDisposable {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public IBindCtx Context;
        public IRunningObjectTable Rot;
        public IEnumMoniker Monikers;
        public RotExplorer() {
            CreateBindCtx(0, out Context);

            Context.GetRunningObjectTable(out Rot);
            Rot.EnumRunning(out Monikers);
        }

        public void Dispose() {
            if (Monikers != null) {
                Marshal.ReleaseComObject(Monikers);
            }

            if (Rot != null) {
                Marshal.ReleaseComObject(Rot);
            }

            if (Context != null) {
                Marshal.ReleaseComObject(Context);
            }
        }

        ~RotExplorer() {
            Dispose();
        }
    }
}
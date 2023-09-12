using Bb.Analysis;
using System.Diagnostics;

namespace Bb.OpenApiServices
{
    public class DiagnosticGeneratorBase
    {

        protected virtual void Initialize(ContextGenerator ctx)
        {
            _ctx = ctx;
            _diag = ctx.Diagnostics;
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        protected void Stop()
        {

            var st = new StackTrace();
            var f = st.GetFrame(1);
            Debug.WriteLine($"{f.ToString().Trim()} try to stop");

            if (Debugger.IsAttached)
                Debugger.Break();

        }


        protected ContextGenerator _ctx;
        protected Diagnostics _diag;

    }


}
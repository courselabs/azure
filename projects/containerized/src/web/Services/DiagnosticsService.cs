using System.Net;
using System.Runtime.InteropServices;
using ToDoList.Model;

namespace ToDoList.Services
{
    public class DiagnosticsService
    {
        private static readonly Diagnostic _Diagnostic;

        static DiagnosticsService()
        {
            _Diagnostic = new Diagnostic
            {
                OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                OSDescription = RuntimeInformation.OSDescription,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                HostName = Dns.GetHostName()
            };
        }

        public Diagnostic GetDiagnostics()
        {
            return _Diagnostic;
        }
    }
}

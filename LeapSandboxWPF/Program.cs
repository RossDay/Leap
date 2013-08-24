using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Vyrolan.VMCS
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            var path = CreateNativeDLLTempPath();
            LoadNativeDLL(path, "Leap.dll", VMCS.Properties.Resources.Leap);
            LoadNativeDLL(path, "LeapCSharp.dll", VMCS.Properties.Resources.LeapCSharp);

            App.Main(); // Run WPF startup code.
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        private static string CreateNativeDLLTempPath()
        {
            var an = Assembly.GetExecutingAssembly().GetName();
            var tempFolder = String.Format("{0}.{1}.{2}", an.Name, an.ProcessorArchitecture, an.Version);
            var dirName = Path.Combine(Path.GetTempPath(), tempFolder);

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            return dirName;
        }

        private static void LoadNativeDLL(string tempPath, string name, byte[] resourceBytes)
        {
            var dllPath = Path.Combine(tempPath, name);
            if (!File.Exists(dllPath) || !File.ReadAllBytes(dllPath).SequenceEqual(resourceBytes))
                File.WriteAllBytes(dllPath, resourceBytes);

            LoadLibrary(dllPath);
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs e)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            // Get the Name of the AssemblyFile
            var assemblyName = new AssemblyName(e.Name);
            var dllName = assemblyName.Name + ".dll";

            // Load from Embedded Resources - This function is not called if the Assembly is already
            // in the same folder as the app.
            var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(dllName));
            if (resources.Any())
            {
                // 99% of cases will only have one matching item, but if you don't,
                // you will have to change the logic to handle those cases.
                var resourceName = resources.First();
                using (var stream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    var block = new byte[stream.Length];

                    // Safely try to load the assembly.
                    try
                    {
                        stream.Read(block, 0, block.Length);
                        return Assembly.Load(block);
                    }
                    catch (IOException)
                    {
                        return null;
                    }
                    catch (BadImageFormatException)
                    {
                        return null;
                    }
                }
            }

            // in the case the resource doesn't exist, return null.
            return null;
        }
    }
}

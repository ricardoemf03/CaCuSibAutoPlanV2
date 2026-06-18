using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CaCuSibAutoPlan.Services
{
    public class DllPathResolver
    {
        public string Resolve(string dllFileName)
        {
            if (string.IsNullOrWhiteSpace(dllFileName)) throw new ArgumentException("Nombre de DLL vacío.", "dllFileName");
            if (Path.IsPathRooted(dllFileName) && File.Exists(dllFileName)) return dllFileName;
            foreach (var folder in GetCandidateFolders())
            {
                string candidate = Path.Combine(folder, dllFileName);
                if (File.Exists(candidate)) return candidate;
            }
            throw new FileNotFoundException("No se encontró el DLL del script: " + dllFileName + Environment.NewLine + "Colócalo en la misma carpeta que CaCuSibAutoPlan.esapi.dll o revisa el AssemblyName.");
        }
        private IEnumerable<string> GetCandidateFolders()
        {
            var folders = new List<string>();
            TryAdd(folders, AppDomain.CurrentDomain.BaseDirectory);
            var executing = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrWhiteSpace(executing)) TryAdd(folders, Path.GetDirectoryName(executing));
            var entry = Assembly.GetEntryAssembly();
            if (entry != null && !string.IsNullOrWhiteSpace(entry.Location)) TryAdd(folders, Path.GetDirectoryName(entry.Location));
            var current = folders.FirstOrDefault(Directory.Exists);
            if (!string.IsNullOrWhiteSpace(current))
            {
                DirectoryInfo d = new DirectoryInfo(current);
                for (int i = 0; i < 4 && d != null; i++)
                {
                    TryAdd(folders, d.FullName);
                    TryAdd(folders, Path.Combine(d.FullName, "plugins"));
                    d = d.Parent;
                }
            }
            return folders.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase);
        }
        private static void TryAdd(List<string> folders, string folder) { if (!string.IsNullOrWhiteSpace(folder)) folders.Add(folder); }
    }
}

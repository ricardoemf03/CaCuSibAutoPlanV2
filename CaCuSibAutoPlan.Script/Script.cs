using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using CaCuSibAutoPlan.ViewModels;
using CaCuSibAutoPlan.Views;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: AssemblyVersion("1.0.0.3")]
[assembly: AssemblyFileVersion("1.0.0.3")]
[assembly: AssemblyInformationalVersion("1.0.0.3")]
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script() { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context)
        {
            if (context == null) { MessageBox.Show("El contexto de ESAPI es nulo."); return; }
            if (context.Patient == null) { MessageBox.Show("No hay paciente cargado. Abre un paciente antes de ejecutar AutoPlan."); return; }
            if (context.StructureSet == null) { MessageBox.Show("No hay StructureSet activo. Selecciona un StructureSet antes de ejecutar AutoPlan."); return; }
            var viewModel = new MainViewModel(context);
            var mainWindow = new MainWindow { DataContext = viewModel };
            mainWindow.ShowDialog();
        }
    }
}

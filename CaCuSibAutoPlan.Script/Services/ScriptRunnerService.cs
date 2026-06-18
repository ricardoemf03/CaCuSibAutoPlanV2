using System;
using System.Reflection;
using CaCuSibAutoPlan.Models;
//using CaCuSibAutoPlan.Models;
using CaCuSibAutoPlan.Services;
using VMS.TPS.Common.Model.API;

namespace CaCuSibAutoPlan.Services
{
    public class ScriptRunnerService
    {
        private readonly ScriptContext _context;
        private readonly SettingsService _settingsService;
        private readonly DllPathResolver _dllPathResolver;
        public ScriptRunnerService(ScriptContext context, SettingsService settingsService)
        {
            _context = context ?? throw new ArgumentNullException("context");
            _settingsService = settingsService ?? throw new ArgumentNullException("settingsService");
            _dllPathResolver = new DllPathResolver();
        }
        public void Run(ScriptModule module, AutoPlanSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            _settingsService.Save(settings);
            string dllName = GetDllName(module, settings);
            string dllPath = _dllPathResolver.Resolve(dllName);
            Assembly assembly = Assembly.LoadFrom(dllPath);
            Type scriptType = assembly.GetType("VMS.TPS.Script", true);
            object scriptInstance = Activator.CreateInstance(scriptType);
            MethodInfo executeMethod = scriptType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(ScriptContext) }, null);
            if (executeMethod == null) throw new MissingMethodException("El script " + dllName + " no tiene Execute(ScriptContext context).");
            try { executeMethod.Invoke(scriptInstance, new object[] { _context }); }
            catch (TargetInvocationException ex) { if (ex.InnerException != null) throw ex.InnerException; throw; }
        }
        private static string GetDllName(ScriptModule module, AutoPlanSettings settings)
        {
            switch (module)
            {
                case ScriptModule.OptimizationStructures: return settings.OptimizationScriptDllName;
                case ScriptModule.PlanParameters: return settings.PlanParametersScriptDllName;
                case ScriptModule.DvhEstimation: return settings.DvhEstimationScriptDllName;
                case ScriptModule.FinalDoseCalculation: return settings.CalculationScriptDllName;
                default: throw new ArgumentOutOfRangeException("module");
            }
        }
    }
}

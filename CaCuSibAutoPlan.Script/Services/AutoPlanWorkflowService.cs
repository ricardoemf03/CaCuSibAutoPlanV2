using System;
using CaCuSibAutoPlan.Models;
//using CaCuSibAutoPlan.Shared.Models;
using CaCuSibAutoPlan.Services;
using VMS.TPS.Common.Model.API;

namespace CaCuSibAutoPlan.Services
{
    public class AutoPlanWorkflowService
    {
        public delegate void WorkflowProgressHandler(double percent, string message, bool isIndeterminate);
        private readonly ScriptRunnerService _runner;

        public AutoPlanWorkflowService(ScriptContext context, SettingsService settingsService)
        {
            if (context == null) throw new ArgumentNullException("context");
            _runner = new ScriptRunnerService(context, settingsService);
        }

        public void RunOptimizationStructures(AutoPlanSettings settings) { _runner.Run(ScriptModule.OptimizationStructures, settings); }
        public void RunPlanParameters(AutoPlanSettings settings) { _runner.Run(ScriptModule.PlanParameters, settings); }
        public void RunDvhEstimation(AutoPlanSettings settings) { _runner.Run(ScriptModule.DvhEstimation, settings); }
        public void RunFinalDoseCalculation(AutoPlanSettings settings) { _runner.Run(ScriptModule.FinalDoseCalculation, settings); }

        public void RunFullAutoPlan(AutoPlanSettings settings, WorkflowProgressHandler progress)
        {
            Report(progress, 0, "Preparando AutoPlan...", false);
            Report(progress, 5, "1/4 Creando estructuras de optimización...", false);
            RunOptimizationStructures(settings);
            Report(progress, 30, "2/4 Creando/configurando plan y campos...", false);
            RunPlanParameters(settings);
            Report(progress, 50, "3/4 Ejecutando RapidPlan / estimación DVH...", true);
            RunDvhEstimation(settings);
            Report(progress, 70, "4/4 Optimizando VMAT con dosis intermedia y calculando dosis final...", true);
            RunFinalDoseCalculation(settings);
            Report(progress, 100, "AutoPlan completo finalizado.", false);
        }

        public void RunFullAutoPlan(AutoPlanSettings settings)
        {
            RunFullAutoPlan(settings, null);
        }

        private static void Report(WorkflowProgressHandler progress, double percent, string message, bool isIndeterminate)
        {
            if (progress != null) progress(percent, message, isIndeterminate);
        }
    }
}

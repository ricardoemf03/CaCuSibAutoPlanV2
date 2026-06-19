using System;
using CaCuSibAutoPlan.Models;
using CaCuSibAutoPlan.Modules.OptimizationStructures;
using CaCuSibAutoPlan.Modules.PlanParameters;
using CaCuSibAutoPlan.Modules.DvhEstimation;
using CaCuSibAutoPlan.Modules.FinalDoseCalculation;
using VMS.TPS.Common.Model.API;

namespace CaCuSibAutoPlan.Services
{
    public class AutoPlanWorkflowService
    {
        public delegate void WorkflowProgressHandler(
            double percent,
            string message,
            bool isIndeterminate);

        private readonly ScriptContext _context;
        private readonly SettingsService _settingsService;

        private readonly OptimizationStructuresModule _optimizationStructuresModule;
        private readonly PlanParametersModule _planParametersModule;
        private readonly DvhEstimationModule _dvhEstimationModule;
        private readonly FinalDoseCalculationModule _finalDoseCalculationModule;

        public AutoPlanWorkflowService(
            ScriptContext context,
            SettingsService settingsService)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (settingsService == null)
                throw new ArgumentNullException("settingsService");

            _context = context;
            _settingsService = settingsService;

            _optimizationStructuresModule = new OptimizationStructuresModule();
            _planParametersModule = new PlanParametersModule();
            _dvhEstimationModule = new DvhEstimationModule();
            _finalDoseCalculationModule = new FinalDoseCalculationModule();
        }

        public void RunOptimizationStructures(AutoPlanSettings settings)
        {
            SaveSettings(settings);
            _optimizationStructuresModule.Run(_context, settings);
        }

        public void RunPlanParameters(AutoPlanSettings settings)
        {
            SaveSettings(settings);
            _planParametersModule.Run(_context, settings);
        }

        public void RunDvhEstimation(AutoPlanSettings settings)
        {
            SaveSettings(settings);
            _dvhEstimationModule.Run(_context, settings);
        }

        public void RunFinalDoseCalculation(AutoPlanSettings settings)
        {
            SaveSettings(settings);
            _finalDoseCalculationModule.Run(_context, settings);
        }

        public void RunFullAutoPlan(
            AutoPlanSettings settings,
            WorkflowProgressHandler progress)
        {
            SaveSettings(settings);

            Report(progress, 0, "Preparando AutoPlan...", false);

            Report(progress, 5, "1/4 Creando estructuras de optimización...", false);
            _optimizationStructuresModule.Run(_context, settings);

            Report(progress, 30, "2/4 Creando/configurando plan y campos...", false);
            _planParametersModule.Run(_context, settings);

            Report(progress, 50, "3/4 Ejecutando RapidPlan / estimación DVH...", true);
            _dvhEstimationModule.Run(_context, settings);

            Report(progress, 70, "4/4 Optimizando VMAT y calculando dosis final...", true);
            _finalDoseCalculationModule.Run(_context, settings);

            Report(progress, 100, "AutoPlan completo finalizado.", false);
        }

        public void RunFullAutoPlan(AutoPlanSettings settings)
        {
            RunFullAutoPlan(settings, null);
        }

        private void SaveSettings(AutoPlanSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settingsService.Save(settings);
        }

        private static void Report(
            WorkflowProgressHandler progress,
            double percent,
            string message,
            bool isIndeterminate)
        {
            if (progress != null)
                progress(percent, message, isIndeterminate);
        }
    }
}
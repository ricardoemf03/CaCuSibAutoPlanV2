using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using CaCuSibAutoPlan.Models;

namespace CaCuSibAutoPlan.Modules.FinalDoseCalculation
{
    public sealed class FinalDoseCalculationModule
    {
        // Cambia este valor manualmente:
        // true  = usar GPU durante la optimización VMAT.
        // false = no usar GPU durante la optimización VMAT.
        private const bool UseGpuForVmatOptimization = true;
        public void Run(ScriptContext context, AutoPlanSettings settings)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (settings == null)
                throw new ArgumentNullException("settings");

            if (context.Patient == null)
                throw new ApplicationException("No hay paciente cargado.");

            AutoPlanSettings ui = settings;

            ExternalPlanSetup plan = FindExternalPlanById(
                context.Patient,
                ui.CourseId,
                ui.PlanId);

            context.Patient.BeginModifications();

            try
            {
                ConfigureGpuForVmatOptimization(plan, UseGpuForVmatOptimization);
                OptimizerResult optimizerResult = RunVmatOptimization(plan, ui);

                if (!optimizerResult.Success)
                    throw new ApplicationException("La optimización VMAT falló.\nRazón: " + SafeDetails(optimizerResult));

                CalculationResult doseResult = plan.CalculateDose();

                if (!doseResult.Success)
                    throw new ApplicationException("El cálculo de dosis final falló.\nRazón: " + SafeDetails(doseResult));

                MessageBox.Show(
                    BuildSuccessMessage(plan, optimizerResult, doseResult),
                    "Optimización y cálculo final",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en optimización/cálculo final:\n\n" + ex,
                    "FinalDoseCalculation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                throw;
            }
        }

        private static void ConfigureGpuForVmatOptimization(
            ExternalPlanSetup plan,
            bool useGpu)
        {
            if (plan == null)
                throw new ArgumentNullException("plan");

            string optimizationModel = plan.GetCalculationModel(
                CalculationType.PhotonVMATOptimization);

            if (string.IsNullOrWhiteSpace(optimizationModel))
            {
                throw new ApplicationException(
                    "El plan no tiene configurado un modelo de optimización VMAT.");
            }

            string gpuOptionValue = useGpu ? "Yes" : "No";

            bool gpuOptionWasSet = plan.SetCalculationOption(
                optimizationModel,
                "UseGPU",
                gpuOptionValue);

            if (!gpuOptionWasSet)
            {
                throw new ApplicationException(
                    "El modelo de optimización VMAT '" + optimizationModel +
                    "' no expone la opción UseGPU. Verifica las opciones " +
                    "disponibles para ese modelo en la configuración DCF.");
            }
        }

        private static OptimizerResult RunVmatOptimization(
            ExternalPlanSetup plan,
            AutoPlanSettings ui)
        {
            if (plan == null)
                throw new ArgumentNullException("plan");

            string mlcId = string.IsNullOrWhiteSpace(ui.VmatMlcId)
                ? string.Empty
                : ui.VmatMlcId.Trim();

            if (ui.UseIntermediateDoseDuringOptimization)
            {
                var options = new OptimizationOptionsVMAT(
                    OptimizationIntermediateDoseOption.UseIntermediateDose,
                    mlcId);

                return plan.OptimizeVMAT(options);
            }

            if (!string.IsNullOrWhiteSpace(mlcId))
                return plan.OptimizeVMAT(mlcId);

            return plan.OptimizeVMAT();
        }

        private static ExternalPlanSetup FindExternalPlanById(
            Patient patient,
            string courseId,
            string planId)
        {
            if (patient == null)
                throw new ArgumentNullException("patient");

            if (string.IsNullOrWhiteSpace(planId))
                throw new ApplicationException("Debes definir PlanId en la interfaz.");

            IEnumerable<Course> courses = patient.Courses;

            if (!string.IsNullOrWhiteSpace(courseId))
            {
                Course course = courses.FirstOrDefault(c =>
                    string.Equals(c.Id, courseId, StringComparison.OrdinalIgnoreCase));

                if (course == null)
                    throw new ApplicationException("No encontré el Course.Id = " + courseId);

                ExternalPlanSetup plan = course.ExternalPlanSetups.FirstOrDefault(p =>
                    string.Equals(p.Id, planId, StringComparison.OrdinalIgnoreCase));

                if (plan == null)
                {
                    throw new ApplicationException(
                        "No encontré el Plan.Id = " + planId +
                        " dentro del Course.Id = " + courseId);
                }

                return plan;
            }

            List<ExternalPlanSetup> matches = courses
                .SelectMany(c => c.ExternalPlanSetups)
                .Where(p => string.Equals(p.Id, planId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
                throw new ApplicationException("No encontré ningún ExternalPlanSetup con Id = " + planId);

            if (matches.Count > 1)
            {
                throw new ApplicationException(
                    "Encontré más de un plan con Id = " + planId +
                    ". Define también CourseId para evitar ambigüedad.");
            }

            return matches[0];
        }

        private static string SafeDetails(object result)
        {
            return result == null ? "(resultado nulo)" : result.ToString();
        }

        private static string BuildSuccessMessage(
            ExternalPlanSetup plan,
            OptimizerResult optimizerResult,
            CalculationResult doseResult)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Optimización VMAT completada exitosamente.");
            sb.AppendLine("Dosis final calculada exitosamente.");
            sb.AppendLine();
            sb.AppendLine("Plan: " + plan.Id);
            sb.AppendLine();
            sb.AppendLine("OptimizerResult:");
            sb.AppendLine(SafeDetails(optimizerResult));
            sb.AppendLine();
            sb.AppendLine("CalculationResult:");
            sb.AppendLine(SafeDetails(doseResult));

            return sb.ToString();
        }
    }
}
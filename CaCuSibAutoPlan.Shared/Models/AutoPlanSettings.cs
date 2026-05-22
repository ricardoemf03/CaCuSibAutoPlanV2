using System;

namespace CaCuSibAutoPlan.Shared.Models
{
    [Serializable]
    public class AutoPlanSettings
    {
        public string PlanId { get; set; }
        public string Ptv1Id { get; set; }
        public string Ptv2Id { get; set; }
        public string Ptv3Id { get; set; }
        public string Ptv1DoseGyText { get; set; }
        public string Ptv2DoseGyText { get; set; }
        public string Ptv3DoseGyText { get; set; }
        public string MachineId { get; set; }
        public string CourseId { get; set; }
        public string PrescriptionId { get; set; }
        public string PrescriptionName { get; set; }
        public string PrescriptionTargetId { get; set; }
        public bool HasInguinalNodes { get; set; }
        public bool CropPtvOutsideBody { get; set; }
        public bool UseIntermediateDoseDuringOptimization { get; set; }
        public string VmatMlcId { get; set; }
        public string OptimizationScriptDllName { get; set; }
        public string PlanParametersScriptDllName { get; set; }
        public string DvhEstimationScriptDllName { get; set; }
        public string CalculationScriptDllName { get; set; }
        public DateTime SavedAt { get; set; }

        public AutoPlanSettings()
        {
            PlanId = "plan1";
            MachineId = "HAL1102";
            Ptv1DoseGyText = "45";
            Ptv2DoseGyText = "55";
            Ptv3DoseGyText = "57.5";
            HasInguinalNodes = false;
            CropPtvOutsideBody = false;
            UseIntermediateDoseDuringOptimization = true;
            VmatMlcId = string.Empty;
            OptimizationScriptDllName = "opti3.esapi.dll";
            PlanParametersScriptDllName = "planParams3.esapi.dll";
            DvhEstimationScriptDllName = "DVHestimations3.esapi.dll";
            CalculationScriptDllName = "FinalDoseCalculation3.esapi.dll";
            SavedAt = DateTime.Now;
        }
    }
}

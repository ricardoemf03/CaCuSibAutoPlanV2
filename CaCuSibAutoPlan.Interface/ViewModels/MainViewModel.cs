using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CaCuSibAutoPlan.Interface.Commands;
using CaCuSibAutoPlan.Interface.Models;
using CaCuSibAutoPlan.Interface.Services;
using CaCuSibAutoPlan.Shared.Models;
using CaCuSibAutoPlan.Shared.Services;
using VMS.TPS.Common.Model.API;

namespace CaCuSibAutoPlan.Interface.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ScriptContext _context;
        private readonly EsapiDataService _dataService;
        private readonly SettingsService _settingsService;
        private readonly AutoPlanWorkflowService _workflowService;

        private string _patientName;
        private string _patientId;
        private string _structureSetId;
        private string _statusMessage;
        private string _planId;
        private string _ptv1DoseGyText;
        private string _ptv2DoseGyText;
        private string _ptv3DoseGyText;
        private string _selectedMachineId;
        private string _selectedCourseId;
        private string _workflowProgressText;

        private bool _hasInguinalNodes;
        private bool _cropPtvOutsideBody;
        private bool _isBusy;
        private bool _workflowProgressIsIndeterminate;
        private double _workflowProgressValue;

        private PtvOption _selectedPtv1;
        private PtvOption _selectedPtv2;
        private PtvOption _selectedPtv3;
        private PrescriptionOption _selectedPrescription;
        private PrescriptionTargetOption _selectedPrescriptionTarget;

        public MainViewModel(ScriptContext context)
        {
            _context = context ?? throw new ArgumentNullException("context");
            _dataService = new EsapiDataService(_context);
            _settingsService = new SettingsService();
            _workflowService = new AutoPlanWorkflowService(_context, _settingsService);

            Ptvs = new ObservableCollection<PtvOption>();
            PtvsWithNone = new ObservableCollection<PtvOption>();
            Courses = new ObservableCollection<string>();
            Prescriptions = new ObservableCollection<PrescriptionOption>();
            PrescriptionTargets = new ObservableCollection<PrescriptionTargetOption>();

            RunOptimizationStructuresCommand = new RelayCommand(RunOptimizationStructures, CanRunBasic);
            RunPlanParametersCommand = new RelayCommand(RunPlanParameters, CanRunFull);
            RunDvhEstimationCommand = new RelayCommand(RunDvhEstimation, CanRunFull);
            RunAutoPlanCommand = new RelayCommand(RunAutoPlan, CanRunFull);
            SaveSettingsCommand = new RelayCommand(SaveSettings, CanRunBasic);

            LoadInitialData();
        }

        public ObservableCollection<PtvOption> Ptvs { get; private set; }
        public ObservableCollection<PtvOption> PtvsWithNone { get; private set; }
        public ObservableCollection<string> Courses { get; private set; }
        public ObservableCollection<PrescriptionOption> Prescriptions { get; private set; }
        public ObservableCollection<PrescriptionTargetOption> PrescriptionTargets { get; private set; }

        public string PatientName
        {
            get { return _patientName; }
            set { _patientName = value; OnPropertyChanged(); }
        }

        public string PatientId
        {
            get { return _patientId; }
            set { _patientId = value; OnPropertyChanged(); }
        }

        public string StructureSetId
        {
            get { return _structureSetId; }
            set { _structureSetId = value; OnPropertyChanged(); }
        }

        public string PlanId
        {
            get { return _planId; }
            set { _planId = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public string Ptv1DoseGyText
        {
            get { return _ptv1DoseGyText; }
            set { _ptv1DoseGyText = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public string Ptv2DoseGyText
        {
            get { return _ptv2DoseGyText; }
            set { _ptv2DoseGyText = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public string Ptv3DoseGyText
        {
            get { return _ptv3DoseGyText; }
            set { _ptv3DoseGyText = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public string SelectedMachineId
        {
            get { return _selectedMachineId; }
            set { _selectedMachineId = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public bool HasInguinalNodes
        {
            get { return _hasInguinalNodes; }
            set { _hasInguinalNodes = value; OnPropertyChanged(); }
        }

        public bool CropPtvOutsideBody
        {
            get { return _cropPtvOutsideBody; }
            set { _cropPtvOutsideBody = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public double WorkflowProgressValue
        {
            get { return _workflowProgressValue; }
            set { _workflowProgressValue = value; OnPropertyChanged(); }
        }

        public string WorkflowProgressText
        {
            get { return _workflowProgressText; }
            set { _workflowProgressText = value; OnPropertyChanged(); }
        }

        public bool WorkflowProgressIsIndeterminate
        {
            get { return _workflowProgressIsIndeterminate; }
            set { _workflowProgressIsIndeterminate = value; OnPropertyChanged(); }
        }

        public PtvOption SelectedPtv1
        {
            get { return _selectedPtv1; }
            set { _selectedPtv1 = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public PtvOption SelectedPtv2
        {
            get { return _selectedPtv2; }
            set { _selectedPtv2 = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public PtvOption SelectedPtv3
        {
            get { return _selectedPtv3; }
            set { _selectedPtv3 = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public string SelectedCourseId
        {
            get { return _selectedCourseId; }
            set
            {
                _selectedCourseId = value;
                OnPropertyChanged();
                LoadPrescriptions();
                RaiseCommandStates();
            }
        }

        public PrescriptionOption SelectedPrescription
        {
            get { return _selectedPrescription; }
            set
            {
                _selectedPrescription = value;
                OnPropertyChanged();
                LoadPrescriptionTargets();
                RaiseCommandStates();
            }
        }

        public PrescriptionTargetOption SelectedPrescriptionTarget
        {
            get { return _selectedPrescriptionTarget; }
            set { _selectedPrescriptionTarget = value; OnPropertyChanged(); RaiseCommandStates(); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand RunOptimizationStructuresCommand { get; private set; }
        public ICommand RunPlanParametersCommand { get; private set; }
        public ICommand RunDvhEstimationCommand { get; private set; }
        public ICommand RunAutoPlanCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }

        private void LoadInitialData()
        {
            PatientName = _dataService.GetPatientName();
            PatientId = _dataService.GetPatientId();
            StructureSetId = _dataService.GetStructureSetId();

            AutoPlanSettings saved = _settingsService.Load();

            PlanId = saved.PlanId;
            Ptv1DoseGyText = saved.Ptv1DoseGyText;
            Ptv2DoseGyText = saved.Ptv2DoseGyText;
            Ptv3DoseGyText = saved.Ptv3DoseGyText;
            SelectedMachineId = string.IsNullOrWhiteSpace(saved.MachineId) ? "HAL1102" : saved.MachineId;
            HasInguinalNodes = saved.HasInguinalNodes;
            CropPtvOutsideBody = saved.CropPtvOutsideBody;

            WorkflowProgressValue = 0;
            WorkflowProgressText = "Listo para ejecutar AutoPlan.";
            WorkflowProgressIsIndeterminate = false;

            LoadPtvs(saved);
            LoadCourses(saved);

            StatusMessage = "Listo. Configuración guardada en: " + _settingsService.SettingsPath;
        }

        private void LoadPtvs(AutoPlanSettings saved)
        {
            Ptvs.Clear();
            PtvsWithNone.Clear();

            foreach (PtvOption ptv in _dataService.GetPtvOptions(false))
                Ptvs.Add(ptv);

            foreach (PtvOption ptv in _dataService.GetPtvOptions(true))
                PtvsWithNone.Add(ptv);

            SelectedPtv1 = FindPtv(Ptvs, saved.Ptv1Id) ?? Ptvs.FirstOrDefault();
            SelectedPtv2 = FindPtv(Ptvs, saved.Ptv2Id) ?? Ptvs.Skip(1).FirstOrDefault() ?? Ptvs.FirstOrDefault();
            SelectedPtv3 = FindPtv(PtvsWithNone, saved.Ptv3Id) ?? PtvsWithNone.FirstOrDefault();
        }

        private void LoadCourses(AutoPlanSettings saved)
        {
            Courses.Clear();
            foreach (string courseId in _dataService.GetCourseIds())
                Courses.Add(courseId);

            SelectedCourseId = Courses.FirstOrDefault(c => string.Equals(c, saved.CourseId, StringComparison.OrdinalIgnoreCase))
                               ?? Courses.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(saved.PrescriptionId) || !string.IsNullOrWhiteSpace(saved.PrescriptionName))
            {
                SelectedPrescription = Prescriptions.FirstOrDefault(p =>
                    string.Equals(p.Id, saved.PrescriptionId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name, saved.PrescriptionName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(saved.PrescriptionTargetId))
            {
                SelectedPrescriptionTarget = PrescriptionTargets.FirstOrDefault(t =>
                    string.Equals(t.TargetId, saved.PrescriptionTargetId, StringComparison.OrdinalIgnoreCase));
            }
        }

        private static PtvOption FindPtv(ObservableCollection<PtvOption> source, string id)
        {
            if (source == null)
                return null;

            if (string.IsNullOrWhiteSpace(id))
                return source.FirstOrDefault(p => p.IsNoneOption);

            return source.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        private void LoadPrescriptions()
        {
            Prescriptions.Clear();
            PrescriptionTargets.Clear();
            _selectedPrescription = null;
            _selectedPrescriptionTarget = null;
            OnPropertyChanged("SelectedPrescription");
            OnPropertyChanged("SelectedPrescriptionTarget");

            if (string.IsNullOrWhiteSpace(SelectedCourseId))
                return;

            foreach (PrescriptionOption prescription in _dataService.GetPrescriptionsFromCourse(SelectedCourseId))
                Prescriptions.Add(prescription);

            SelectedPrescription = Prescriptions.FirstOrDefault();
        }

        private void LoadPrescriptionTargets()
        {
            PrescriptionTargets.Clear();
            _selectedPrescriptionTarget = null;
            OnPropertyChanged("SelectedPrescriptionTarget");

            if (SelectedPrescription == null)
                return;

            foreach (PrescriptionTargetOption target in _dataService.GetPrescriptionTargets(SelectedCourseId, SelectedPrescription.Id))
                PrescriptionTargets.Add(target);

            SelectedPrescriptionTarget = PrescriptionTargets.FirstOrDefault();
        }

        private AutoPlanSettings BuildSettings()
        {
            AutoPlanSettings current = _settingsService.Load();

            current.PlanId = PlanId;
            current.Ptv1Id = SelectedPtv1 != null ? SelectedPtv1.Id : null;
            current.Ptv2Id = SelectedPtv2 != null ? SelectedPtv2.Id : null;
            current.Ptv3Id = SelectedPtv3 != null && !SelectedPtv3.IsNoneOption ? SelectedPtv3.Id : null;

            current.Ptv1DoseGyText = Ptv1DoseGyText;
            current.Ptv2DoseGyText = Ptv2DoseGyText;
            current.Ptv3DoseGyText = Ptv3DoseGyText;

            current.MachineId = SelectedMachineId;

            current.CourseId = SelectedCourseId;
            current.PrescriptionId = SelectedPrescription != null ? SelectedPrescription.Id : null;
            current.PrescriptionName = SelectedPrescription != null ? SelectedPrescription.Name : null;
            current.PrescriptionTargetId = SelectedPrescriptionTarget != null ? SelectedPrescriptionTarget.TargetId : null;

            current.HasInguinalNodes = HasInguinalNodes;
            current.CropPtvOutsideBody = CropPtvOutsideBody;
            current.UseIntermediateDoseDuringOptimization = true;

            if (string.IsNullOrWhiteSpace(current.CalculationScriptDllName))
                current.CalculationScriptDllName = "FinalDoseCalculation3.esapi.dll";

            return current;
        }

        private bool CanRunBasic()
        {
            return !IsBusy
                && !string.IsNullOrWhiteSpace(PlanId)
                && SelectedPtv1 != null
                && SelectedPtv2 != null
                && !string.IsNullOrWhiteSpace(SelectedMachineId);
        }

        private bool CanRunFull()
        {
            return CanRunBasic()
                && !string.IsNullOrWhiteSpace(SelectedCourseId)
                && SelectedPrescription != null
                && SelectedPrescriptionTarget != null;
        }

        private void SaveSettings()
        {
            try
            {
                _settingsService.Save(BuildSettings());
                StatusMessage = "Configuración guardada.";
            }
            catch (Exception ex)
            {
                ShowError("Guardar configuración", ex);
            }
        }

        private void RunOptimizationStructures()
        {
            RunSafely("Optimization Structures", delegate
            {
                _workflowService.RunOptimizationStructures(BuildSettings());
            });
        }

        private void RunPlanParameters()
        {
            RunSafely("Plan Parameters", delegate
            {
                _workflowService.RunPlanParameters(BuildSettings());
            });
        }

        private void RunDvhEstimation()
        {
            RunSafely("DVH Estimation", delegate
            {
                _workflowService.RunDvhEstimation(BuildSettings());
            });
        }

        private void RunAutoPlan()
        {
            RunSafelyWithEmbeddedProgress("AutoPlan completo", delegate
            {
                _workflowService.RunFullAutoPlan(BuildSettings(), ReportProgress);
            });
        }

        private void RunSafely(string actionName, Action action)
        {
            try
            {
                IsBusy = true;
                ReportProgress(0, "Ejecutando: " + actionName + "...", false);
                _settingsService.Save(BuildSettings());
                action();
                ReportProgress(100, actionName + " finalizado.", false);
            }
            catch (Exception ex)
            {
                ShowError(actionName, ex);
            }
            finally
            {
                IsBusy = false;
                WorkflowProgressIsIndeterminate = false;
                RaiseCommandStates();
            }
        }

        private void RunSafelyWithEmbeddedProgress(string actionName, Action action)
        {
            try
            {
                IsBusy = true;
                ReportProgress(0, "Ejecutando: " + actionName + "...", false);

                _settingsService.Save(BuildSettings());

                // IMPORTANTE:
                // Las llamadas ESAPI permanecen en el hilo original de Eclipse.
                // No se usa Task.Run ni un thread externo para plan.OptimizeVMAT() / plan.CalculateDose(),
                // porque ESAPI no es seguro fuera del hilo en el que Eclipse creó el ScriptContext.
                //
                // La barra se mantiene dentro de la interfaz principal. Durante las llamadas largas
                // de ESAPI, Eclipse/WPF puede quedar ocupado porque OptimizeVMAT y CalculateDose son
                // llamadas síncronas y no exponen progreso interno por callback.
                action();

                ReportProgress(100, actionName + " finalizado.", false);
            }
            catch (Exception ex)
            {
                ShowError(actionName, ex);
            }
            finally
            {
                IsBusy = false;
                WorkflowProgressIsIndeterminate = false;
                RaiseCommandStates();
            }
        }

        private void ReportProgress(double percent, string message, bool isIndeterminate)
        {
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            WorkflowProgressValue = percent;
            WorkflowProgressText = message;
            WorkflowProgressIsIndeterminate = isIndeterminate;
            StatusMessage = message;

            PumpUi();
        }

        private static void PumpUi()
        {
            Dispatcher dispatcher = System.Windows.Application.Current != null
                ? System.Windows.Application.Current.Dispatcher
                : Dispatcher.CurrentDispatcher;

            if (dispatcher == null || !dispatcher.CheckAccess())
                return;

            DispatcherFrame frame = new DispatcherFrame();
            dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(delegate { frame.Continue = false; }));
            Dispatcher.PushFrame(frame);
        }

        private void ShowError(string actionName, Exception ex)
        {
            StatusMessage = "Error en " + actionName + ": " + ex.Message;
            //MessageBox.Show(ex.ToString(), "Error en " + actionName, MessageBoxButton.OK, MessageBoxImage.Error);
            MessageBox.Show(ex.Message, "Error en " + actionName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RaiseCommandStates()
        {
            Raise(RunOptimizationStructuresCommand);
            Raise(RunPlanParametersCommand);
            Raise(RunDvhEstimationCommand);
            Raise(RunAutoPlanCommand);
            Raise(SaveSettingsCommand);
        }

        private static void Raise(ICommand command)
        {
            RelayCommand relay = command as RelayCommand;
            if (relay != null)
                relay.RaiseCanExecuteChanged();
        }
    }
}

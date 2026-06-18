using System;
using System.Collections.Generic;
using System.Linq;
using CaCuSibAutoPlan.Models;
using VMS.TPS.Common.Model.API;

namespace CaCuSibAutoPlan.Services
{
    public class EsapiDataService
    {
        private readonly ScriptContext _context;
        public EsapiDataService(ScriptContext context) { _context = context ?? throw new ArgumentNullException("context"); }
        public string GetPatientName() { return _context.Patient != null ? _context.Patient.Name : "(sin paciente)"; }
        public string GetPatientId() { return _context.Patient != null ? _context.Patient.Id : "(sin paciente)"; }
        public string GetStructureSetId() { return _context.StructureSet != null ? _context.StructureSet.Id : "(sin StructureSet)"; }
        public List<PtvOption> GetPtvOptions(bool includeNoneOption)
        {
            if (_context.StructureSet == null) throw new InvalidOperationException("No hay StructureSet activo en el contexto.");
            var result = _context.StructureSet.Structures.Where(s => s != null && string.Equals(s.DicomType, "PTV", StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Id).Select(s => new PtvOption { Id = s.Id, DisplayName = s.Id, IsNoneOption = false }).ToList();
            if (includeNoneOption) result.Insert(0, new PtvOption { Id = null, DisplayName = "No existe PTV3", IsNoneOption = true });
            return result;
        }
        public List<string> GetCourseIds()
        {
            if (_context.Patient == null) throw new InvalidOperationException("No hay paciente cargado.");
            return _context.Patient.Courses.Where(c => c != null).Select(c => c.Id).OrderBy(id => id).ToList();
        }
        public List<PrescriptionOption> GetPrescriptionsFromCourse(string courseId)
        {
            if (_context.Patient == null) throw new InvalidOperationException("No hay paciente cargado.");
            if (string.IsNullOrWhiteSpace(courseId)) return new List<PrescriptionOption>();
            var course = _context.Patient.Courses.FirstOrDefault(c => string.Equals(c.Id, courseId, StringComparison.OrdinalIgnoreCase));
            if (course == null) return new List<PrescriptionOption>();
            return course.TreatmentPhases.Where(tp => tp != null).SelectMany(tp => tp.Prescriptions ?? Enumerable.Empty<RTPrescription>()).Where(p => p != null).GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).Select(p => new PrescriptionOption { Id = p.Id, Name = p.Name }).OrderBy(p => p.DisplayName).ToList();
        }
        public List<PrescriptionTargetOption> GetPrescriptionTargets(string courseId, string prescriptionIdOrName)
        {
            if (_context.Patient == null) throw new InvalidOperationException("No hay paciente cargado.");
            if (string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(prescriptionIdOrName)) return new List<PrescriptionTargetOption>();
            var course = _context.Patient.Courses.FirstOrDefault(c => string.Equals(c.Id, courseId, StringComparison.OrdinalIgnoreCase));
            if (course == null) return new List<PrescriptionTargetOption>();
            var prescription = course.TreatmentPhases.Where(tp => tp != null).SelectMany(tp => tp.Prescriptions ?? Enumerable.Empty<RTPrescription>()).FirstOrDefault(p => p != null && (string.Equals(p.Id, prescriptionIdOrName, StringComparison.OrdinalIgnoreCase) || string.Equals(p.Name, prescriptionIdOrName, StringComparison.OrdinalIgnoreCase)));
            if (prescription == null) return new List<PrescriptionTargetOption>();
            return prescription.Targets.Where(t => t != null).Select(t => new PrescriptionTargetOption { TargetId = t.TargetId }).OrderBy(t => t.TargetId).ToList();
        }
    }
}

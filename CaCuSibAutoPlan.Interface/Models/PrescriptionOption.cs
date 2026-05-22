namespace CaCuSibAutoPlan.Interface.Models
{
    public class PrescriptionOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get { return !string.IsNullOrWhiteSpace(Name) ? Name : Id; } }
        public override string ToString() { return DisplayName; }
    }
}

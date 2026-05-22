namespace CaCuSibAutoPlan.Interface.Models
{
    public class PtvOption
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsNoneOption { get; set; }
        public override string ToString() { return DisplayName; }
    }
}

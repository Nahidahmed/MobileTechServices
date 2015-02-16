using System.Xml.Serialization;

namespace MTechServices.Models.Entity
{
    public class UrgencyInfo : SystemCodeInfo {
        [XmlElement(IsNullable = true)]
        public string Notes { get; set; }

        [XmlElement]
        public int AppliesWO { get; set; }

        [XmlElement]
        public int AppliesEO { get; set; }
    }
}

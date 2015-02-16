using System.Xml.Serialization;

namespace MTechServices.Models.Entity
{
    public class OpenWorkStatusInfo : SystemCodeInfo {
        [XmlElement(IsNullable = true)]
        public string Notes { get; set; }
        [XmlElement]
        public short RequestsActionCode { get; set; }

    }
}
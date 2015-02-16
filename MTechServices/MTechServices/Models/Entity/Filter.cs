using System.Xml.Serialization;

namespace MTechServices.Models.Entity
{
	public class Filter
	{
		[XmlElement(IsNullable = true)]
		public long MTFPrimaryId { get; set; }
		[XmlElement]
		public string FilterName { get; set; }
		[XmlElement]
		public byte Active { get; set; }
        [XmlElement(IsNullable = true)]
        public long SECPrimaryId { get; set; }
	}


	public class FilterInfo:Filter
	{
		[XmlElement]
		public string FACPrimaryIds { get; set; }
		[XmlElement]
		public string ACCPrimaryIds { get; set; }
		[XmlElement]
		public string WKRPrimaryIds { get; set; }
	}
}


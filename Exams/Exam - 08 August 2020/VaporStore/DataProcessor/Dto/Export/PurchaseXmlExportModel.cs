using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace VaporStore.DataProcessor.Dto.Export
{
    [XmlType("Purchase")]
    public class PurchaseXmlExportModel
    {
        [XmlElement("Card")]
        public string Card { get; set; }
        
        [XmlElement("Cvc")]
        public string CVC { get; set; }

        [XmlElement("Date")]
        public string Date { get; set; }

        [XmlElement]
        public GameXmlExportModel Game { get; set; }
    }
}
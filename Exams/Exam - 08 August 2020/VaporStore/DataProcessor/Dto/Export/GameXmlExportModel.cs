﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using VaporStore.Data.Models;

namespace VaporStore.DataProcessor.Dto.Export
{
    [XmlType("Game")]
    public class GameXmlExportModel
    {
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlElement("Genre")]
        public Genre Genre { get; set; }

        [XmlElement("Price")]
        public decimal Price { get; set; }
    }
}
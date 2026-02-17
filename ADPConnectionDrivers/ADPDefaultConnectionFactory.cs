using System;
using System.Collections.Generic;
using System.Text;

namespace Cati.ADP.Server {
    public class ADPDefaultConnectionFactory : ADPBaseConnectionFactory {
        public override IADPConnection GetConnection(string driverID) {
            IADPConnection result = null;
            switch (driverID) {
                case "IBProvider":
                    result = new ADPConnectionForIBProvider();
                    break;
                case "DelimitedFile":
                    result = new ADPConnectionForDelimitedFile();
                    break;
                case "XmlDataTable":
                    result = new ADPConnectionForXmlDataTable();
                    break;
                case "XmlDataSet":
                    result = new ADPConnectionForXmlDataSet();
                    break;
            }
            return result;
        }
    }
}

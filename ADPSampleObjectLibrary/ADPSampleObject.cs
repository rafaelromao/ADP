using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using Cati.ADP.Objects;
using Cati.ADP.Common;

namespace ADPSampleObjectLibrary {
    [MappingTable("PERS_PERSON")]
    public class ADPSampleObject : ADPObject {
        public ADPSampleObject(ADPSession session, bool registerInSession)
            : base(session, registerInSession) {
        }
        public ADPSampleObject(ADPSession session)
            : base(session) {
        }
        public string Name {
            get { return (String)GetPropertyValue("Name", ""); }
            set { SetPropertyValue("Name", value); }
        }
        public string DonorNumber {
            get { return (String)GetPropertyValue("DonorNumber", ""); }
            set { SetPropertyValue("DonorNumber", value); }
        }
        [MappingField("PERS_BIRTHDATE")]
        public DateTime DateOfBirth {
            get { return (DateTime)GetPropertyValue("DateOfBirth", new DateTime(1900, 01, 01)); }
            set { SetPropertyValue("DateOfBirth", value); }
        }
    }
}

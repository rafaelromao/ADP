using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using Cati.ADP.Objects;
using Cati.ADP.Common;

namespace ADPSampleObjectLibrary {
    public class ADPSampleObjectCollection : ADPCollection<ADPSampleObject> {
        public ADPSampleObjectCollection(ADPSession session)
            : base(session) {
        }
        public ADPSampleObjectCollection(ADPSession session, List<ADPSampleObject> list)
            : base(session, list) {
        }
        public ADPSampleObjectCollection(ADPSession session, ADPCollection<ADPSampleObject> list)
            : base(session, list) {
        }
    }
}

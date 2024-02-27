using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class Organizations
    {
        public string ORGANIZATION_ID { get; set; }
        public string NAME { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
        public string CODE { get; set; }
        public List<Risk> RiskItems { get; set; }
        public List<HeadActing_Organizations> HeadActing { get; set; }
    }

    public class HeadActing_Organizations
    {
        public string ORGANIZATION_ID { get; set; }
        public string HEAD_ID { get; set; }
        public string HeadEmail { get; set; }
        public string ACTING_HEAD_ID { get; set; }
        public bool ACTING_STATUS { get; set; }
        public string ActingEmail { get; set; }
    }

    public class OrganizationsChildeDB
    {
        public string Parent_ORGANIZATION_ID { get; set; }
        public string ORGANIZATION_ID { get; set; }
        public string Name { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
        public string CODE { get; set; }
        public string HEAD_ID { get; set; }
        public string HEAD_ORGANIZATION_ID { get; set; }
        public string HeadEmail { get; set; }
        public string ACTING_HEAD_ID { get; set; }
        public string ACTING_STATUS { get; set; }
        public string ActingEmail { get; set; }
    }
}

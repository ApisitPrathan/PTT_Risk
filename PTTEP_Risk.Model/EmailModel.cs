using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class EmailModel
    {
        public string Risk_Id { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Body1 { get; set; }
        public string Body2 { get; set; }
        public string Body3 { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
    }
    public class TempRawData
    {
        public string Status { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Risk_Id { get; set; }
        public string Level { get; set; }
        public string Comment { get; set; }
        public string Owner { get; set; }
        public Owner OwnerModel { get; set; }
    }
    public class Owner
    {
        public string ID { get; set; }
        public string Owner_Name { get; set; }
    }

    public class EmailModelInsert
    {
        public string Module { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Body1 { get; set; }
        public string Body2 { get; set; }
        public string Body3 { get; set; }
        public string To { get; set; }
        public string Description { get; set; }
    }

}

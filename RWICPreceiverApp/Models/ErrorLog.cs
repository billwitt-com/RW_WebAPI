//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RWICPreceiverApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ErrorLog
    {
        public int ID { get; set; }
        public string Message { get; set; }
        public string FromPage { get; set; }
        public string StackTrace { get; set; }
        public Nullable<int> Severity { get; set; }
        public string LoggedInUser { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Comment { get; set; }
    }
}

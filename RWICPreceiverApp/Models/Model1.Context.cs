﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class RiverWatchEntities : DbContext
    {
        public RiverWatchEntities()
            : base("name=RiverWatchEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
        public virtual DbSet<InboundICPFinal> InboundICPFinals { get; set; }
        public virtual DbSet<InboundICPOrigional> InboundICPOrigionals { get; set; }
        public virtual DbSet<MetalBarCode> MetalBarCodes { get; set; }
        public virtual DbSet<Sample> Samples { get; set; }
        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<NEWexpWater> NEWexpWaters { get; set; }
        public virtual DbSet<StationImage> StationImages { get; set; }
    }
}

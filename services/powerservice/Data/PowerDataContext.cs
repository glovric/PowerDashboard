using Microsoft.EntityFrameworkCore;
using PowerService.Models.DatabaseEntities;

namespace PowerService.Data
{
    public class PowerDataContext : DbContext
    {
        public PowerDataContext(DbContextOptions<PowerDataContext> options)
            : base(options) { }

        public DbSet<PowerDataQuarter> PowerDataQuarter { get; set; }
        public DbSet<PowerDataHour> PowerDataHour { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PowerDataQuarter>(entity =>
            {
                entity.ToTable("PowerDataQuarter");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.ATLoadValue).HasColumnName("at_load_value");
                entity.Property(e => e.BELoadValue).HasColumnName("be_load_value");
                entity.Property(e => e.DELoadValue).HasColumnName("de_load_value");
                entity.Property(e => e.HULoadValue).HasColumnName("hu_load_value");
                entity.Property(e => e.LULoadValue).HasColumnName("lu_load_value");
                entity.Property(e => e.NLLoadValue).HasColumnName("nl_load_value");
            });

            modelBuilder.Entity<PowerDataHour>(entity =>
            {
                entity.ToTable("PowerDataHour");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.ATLoadValue).HasColumnName("at_load_value");
                entity.Property(e => e.BELoadValue).HasColumnName("be_load_value");
                entity.Property(e => e.BGLoadValue).HasColumnName("bg_load_value");
                entity.Property(e => e.CHLoadValue).HasColumnName("ch_load_value");
                entity.Property(e => e.CYLoadValue).HasColumnName("cy_load_value");
                entity.Property(e => e.CZLoadValue).HasColumnName("cz_load_value");
                entity.Property(e => e.DELoadValue).HasColumnName("de_load_value");
                entity.Property(e => e.DKLoadValue).HasColumnName("dk_load_value");
                entity.Property(e => e.EELoadValue).HasColumnName("ee_load_value");
                entity.Property(e => e.ESLoadValue).HasColumnName("es_load_value");
                entity.Property(e => e.FILoadValue).HasColumnName("fi_load_value");
                entity.Property(e => e.FRLoadValue).HasColumnName("fr_load_value");
                entity.Property(e => e.GBLoadValue).HasColumnName("gb_load_value");
                entity.Property(e => e.GRLoadValue).HasColumnName("gr_load_value");
                entity.Property(e => e.HRLoadValue).HasColumnName("hr_load_value");
                entity.Property(e => e.HULoadValue).HasColumnName("hu_load_value");
                entity.Property(e => e.IELoadValue).HasColumnName("ie_load_value");
                entity.Property(e => e.ITLoadValue).HasColumnName("it_load_value");
                entity.Property(e => e.LTLoadValue).HasColumnName("lt_load_value");
                entity.Property(e => e.LULoadValue).HasColumnName("lu_load_value");
                entity.Property(e => e.LVLoadValue).HasColumnName("lv_load_value");
                entity.Property(e => e.MELoadValue).HasColumnName("me_load_value");
                entity.Property(e => e.NLLoadValue).HasColumnName("nl_load_value");
                entity.Property(e => e.NOLoadValue).HasColumnName("no_load_value");
                entity.Property(e => e.PLLoadValue).HasColumnName("pl_load_value");
                entity.Property(e => e.PTLoadValue).HasColumnName("pt_load_value");
                entity.Property(e => e.ROLoadValue).HasColumnName("ro_load_value");
                entity.Property(e => e.RSLoadValue).HasColumnName("rs_load_value");
                entity.Property(e => e.SELoadValue).HasColumnName("se_load_value");
                entity.Property(e => e.SILoadValue).HasColumnName("si_load_value");
                entity.Property(e => e.SKLoadValue).HasColumnName("sk_load_value");
                entity.Property(e => e.UALoadValue).HasColumnName("ua_load_value");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

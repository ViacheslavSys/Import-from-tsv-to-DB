using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace Import_from_tsv_to_DB;

public partial class BdContext : DbContext
{
    public BdContext()
    {
    }

    public BdContext(DbContextOptions<BdContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Departament> Departaments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Jobtitle> Jobtitles { get; set; }

     
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder();
        // установка пути к текущему каталогу
        builder.SetBasePath(Directory.GetCurrentDirectory());
        // получаем конфигурацию из файла appsettings.json
        builder.AddJsonFile("appsettings.json");
        // создаем конфигурацию
        var config = builder.Build();
        string filepath = config.GetConnectionString("DefaultConnection");
        optionsBuilder.UseNpgsql(filepath); 
    }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Departament>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("departaments_pkey");

            entity.ToTable("departaments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Managerid).HasColumnName("managerid");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Parentid).HasColumnName("parentid");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");

            entity.HasOne(d => d.Manager).WithMany(p => p.Departaments)
                .HasForeignKey(d => d.Managerid)
                .HasConstraintName("departaments_managerid_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.Parentid)
                .HasConstraintName("departaments_parentid_fkey");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Departament).HasColumnName("departament");
            entity.Property(e => e.Fullname)
                .HasMaxLength(50)
                .HasColumnName("fullname");
            entity.Property(e => e.Jobtitleid).HasColumnName("jobtitleid");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");

            entity.HasOne(d => d.DepartamentNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Departament)
                .HasConstraintName("employees_departament_fkey");

            entity.HasOne(d => d.Jobtitle).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Jobtitleid)
                .HasConstraintName("employees_jobtitleid_fkey");
        });

        modelBuilder.Entity<Jobtitle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobtitle_pkey");

            entity.ToTable("jobtitle");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

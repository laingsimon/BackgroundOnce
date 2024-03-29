﻿// <auto-generated />
using BackgroundOnce.UnitTestCommon.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    [DbContext(typeof(SqliteDbContext))]
    partial class SqliteDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.12");

            modelBuilder.Entity("BackgroundOnce.UnitTestCommon.Data.Address", b =>
                {
                    b.Property<string>("AddressCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("County")
                        .HasColumnType("TEXT");

                    b.Property<string>("House")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Street")
                        .HasColumnType("TEXT");

                    b.HasKey("AddressCode");

                    b.ToTable("Address");
                });

            modelBuilder.Entity("BackgroundOnce.UnitTestCommon.Data.Department", b =>
                {
                    b.Property<string>("DepartmentCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("AddressCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("DepartmentCode");

                    b.ToTable("Department");
                });

            modelBuilder.Entity("BackgroundOnce.UnitTestCommon.Data.Person", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Age")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DepartmentCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Gender")
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("Person");
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjectNovi.Api.Data;

#nullable disable

namespace ProjectNovi.Data.Migrations
{
    [DbContext(typeof(ProjectNoviContext))]
    partial class ProjectNoviContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("ProjectNovi.Api.Entities.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CountryName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ThreeLetterCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TwoLetterCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("ProjectNovi.Api.Entities.IP", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CountryId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.ToTable("Ips");
                });

            modelBuilder.Entity("ProjectNovi.Api.Entities.IP", b =>
                {
                    b.HasOne("ProjectNovi.Api.Entities.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TibiaCharFinderAPI.Entities;

#nullable disable

namespace TibiaCharFinder.Migrations
{
    [DbContext(typeof(EnemyCharFinderDbContext))]
    [Migration("20220719220119_changedDeleteBehaviorToClientSetNull")]
    partial class changedDeleteBehaviorToClientSetNull
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.Character", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.World", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Worlds");
                });

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.WorldCorrelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("LoginCharacterId")
                        .HasColumnType("int");

                    b.Property<int>("LogoutCharacterId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LoginCharacterId");

                    b.HasIndex("LogoutCharacterId");

                    b.ToTable("WorldCorrelations");
                });

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.WorldScan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("CharactersOnline")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ScanCreateDateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("WorldId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("WorldId");

                    b.ToTable("WorldScans");
                });

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.WorldCorrelation", b =>
                {
                    b.HasOne("TibiaCharFinderAPI.Entities.Character", "LoginCharacter")
                        .WithMany("LoginWorldCorrelations")
                        .HasForeignKey("LoginCharacterId")
                        .IsRequired();

                    b.HasOne("TibiaCharFinderAPI.Entities.Character", "LogoutCharacter")
                        .WithMany("LogoutWorldCorrelations")
                        .HasForeignKey("LogoutCharacterId")
                        .IsRequired();

                    b.Navigation("LoginCharacter");

                    b.Navigation("LogoutCharacter");
                });

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.WorldScan", b =>
                {
                    b.HasOne("TibiaCharFinderAPI.Entities.World", "World")
                        .WithMany("WorldScans")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.Character", b =>
                {
                    b.Navigation("LoginWorldCorrelations");

                    b.Navigation("LogoutWorldCorrelations");
                });

            modelBuilder.Entity("TibiaCharFinderAPI.Entities.World", b =>
                {
                    b.Navigation("WorldScans");
                });
#pragma warning restore 612, 618
        }
    }
}

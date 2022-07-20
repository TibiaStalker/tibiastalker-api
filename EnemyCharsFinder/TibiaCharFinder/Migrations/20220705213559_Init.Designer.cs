﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TibiaCharFinderAPI.Entities;

namespace TibiaCharFinder.Migrations
{
    [DbContext(typeof(TibiaCharacterFinderDbContext))]
    [Migration("20220705213559_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TibiaCharFinder.Entities.Character", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CorrelationId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.Correlation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CharacterId")
                        .HasColumnType("int");

                    b.Property<DateTime>("LogInOrLogOutDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CharacterId");

                    b.ToTable("Correlations");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.Scan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CharactersOnline")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ScanCreateDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Scans");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.ScanWorld", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CharactersOnline")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ScanCreateDateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("WorldId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("WorldId");

                    b.ToTable("ScanWorlds");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.World", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Worlds");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.Character", b =>
                {
                    b.HasOne("TibiaCharFinder.Entities.Correlation", null)
                        .WithMany("PossibleCorrelationCharacters")
                        .HasForeignKey("CorrelationId");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.Correlation", b =>
                {
                    b.HasOne("TibiaCharFinder.Entities.Character", "Character")
                        .WithMany()
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.ScanWorld", b =>
                {
                    b.HasOne("TibiaCharFinder.Entities.World", "World")
                        .WithMany()
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaCharFinder.Entities.Correlation", b =>
                {
                    b.Navigation("PossibleCorrelationCharacters");
                });
#pragma warning restore 612, 618
        }
    }
}

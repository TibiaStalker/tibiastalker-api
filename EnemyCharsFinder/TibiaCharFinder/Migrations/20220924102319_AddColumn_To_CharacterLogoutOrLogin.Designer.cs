﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TibiaCharacterFinderAPI.Entities;

#nullable disable

namespace TibiaCharacterFinderAPI.Migrations
{
    [DbContext(typeof(TibiaCharacterFinderDbContext))]
    [Migration("20220924102319_AddColumn_To_CharacterLogoutOrLogin")]
    partial class AddColumn_To_CharacterLogoutOrLogin
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.Character", b =>
                {
                    b.Property<int>("CharacterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CharacterId"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<short>("WorldId")
                        .HasColumnType("smallint");

                    b.HasKey("CharacterId");

                    b.HasIndex("WorldId");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.CharacterCorrelation", b =>
                {
                    b.Property<int>("CorrelationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CorrelationId"), 1L, 1);

                    b.Property<int>("LoginCharacterId")
                        .HasColumnType("int");

                    b.Property<int>("LogoutCharacterId")
                        .HasColumnType("int");

                    b.Property<short>("NumberOfMatches")
                        .HasColumnType("smallint");

                    b.HasKey("CorrelationId");

                    b.HasIndex("LoginCharacterId");

                    b.HasIndex("LogoutCharacterId");

                    b.ToTable("CharacterCorrelations");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.CharacterLogoutOrLogin", b =>
                {
                    b.Property<int>("CharacterLogoutOrLoginId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CharacterLogoutOrLoginId"), 1L, 1);

                    b.Property<int>("CharacterId")
                        .HasColumnType("int");

                    b.Property<bool>("IsOnline")
                        .HasColumnType("bit");

                    b.Property<int>("WorldScanId")
                        .HasColumnType("int");

                    b.HasKey("CharacterLogoutOrLoginId");

                    b.HasIndex("CharacterId");

                    b.HasIndex("WorldScanId");

                    b.ToTable("CharacterLogoutOrLogins");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.World", b =>
                {
                    b.Property<short>("WorldId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<short>("WorldId"), 1L, 1);

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("WorldId");

                    b.ToTable("Worlds");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.WorldScan", b =>
                {
                    b.Property<int>("WorldScanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("WorldScanId"), 1L, 1);

                    b.Property<string>("CharactersOnline")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ScanCreateDateTime")
                        .HasColumnType("datetime2");

                    b.Property<short>("WorldId")
                        .HasColumnType("smallint");

                    b.HasKey("WorldScanId");

                    b.HasIndex("WorldId");

                    b.ToTable("WorldScans");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.Character", b =>
                {
                    b.HasOne("TibiaCharacterFinderAPI.Entities.World", "World")
                        .WithMany("Characters")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.CharacterCorrelation", b =>
                {
                    b.HasOne("TibiaCharacterFinderAPI.Entities.Character", "LoginCharacter")
                        .WithMany("LoginWorldCorrelations")
                        .HasForeignKey("LoginCharacterId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("TibiaCharacterFinderAPI.Entities.Character", "LogoutCharacter")
                        .WithMany("LogoutWorldCorrelations")
                        .HasForeignKey("LogoutCharacterId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("LoginCharacter");

                    b.Navigation("LogoutCharacter");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.CharacterLogoutOrLogin", b =>
                {
                    b.HasOne("TibiaCharacterFinderAPI.Entities.Character", "Character")
                        .WithMany()
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TibiaCharacterFinderAPI.Entities.WorldScan", "WorldScan")
                        .WithMany()
                        .HasForeignKey("WorldScanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");

                    b.Navigation("WorldScan");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.WorldScan", b =>
                {
                    b.HasOne("TibiaCharacterFinderAPI.Entities.World", "World")
                        .WithMany("WorldScans")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.Character", b =>
                {
                    b.Navigation("LoginWorldCorrelations");

                    b.Navigation("LogoutWorldCorrelations");
                });

            modelBuilder.Entity("TibiaCharacterFinderAPI.Entities.World", b =>
                {
                    b.Navigation("Characters");

                    b.Navigation("WorldScans");
                });
#pragma warning restore 612, 618
        }
    }
}

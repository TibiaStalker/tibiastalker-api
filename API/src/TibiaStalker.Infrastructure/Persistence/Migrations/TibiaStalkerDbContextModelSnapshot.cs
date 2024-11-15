﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TibiaStalker.Infrastructure.Persistence;

#nullable disable

namespace TibiaStalker.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(TibiaStalkerDbContext))]
    partial class TibiaStalkerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TibiaStalker.Domain.Entities.Character", b =>
                {
                    b.Property<int>("CharacterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("character_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CharacterId"));

                    b.Property<int>("DeleteApproachNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("delete_approach_number");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<DateOnly?>("TradedDate")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("date")
                        .HasDefaultValue(new DateOnly(2001, 1, 1))
                        .HasColumnName("traded_date");

                    b.Property<DateOnly?>("VerifiedDate")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("date")
                        .HasDefaultValue(new DateOnly(2001, 1, 1))
                        .HasColumnName("verified_date");

                    b.Property<short>("WorldId")
                        .HasColumnType("smallint")
                        .HasColumnName("world_id");

                    b.HasKey("CharacterId")
                        .HasName("pk_characters");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_characters_name");

                    b.HasIndex("TradedDate")
                        .HasDatabaseName("ix_characters_traded_date");

                    b.HasIndex("VerifiedDate")
                        .HasDatabaseName("ix_characters_verified_date");

                    b.HasIndex("WorldId")
                        .HasDatabaseName("ix_characters_world_id");

                    b.ToTable("characters", "public");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.CharacterCorrelation", b =>
                {
                    b.Property<long>("CorrelationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("correlation_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("CorrelationId"));

                    b.Property<DateOnly>("CreateDate")
                        .HasColumnType("date")
                        .HasColumnName("create_date");

                    b.Property<DateOnly>("LastMatchDate")
                        .HasColumnType("date")
                        .HasColumnName("last_match_date");

                    b.Property<int>("LoginCharacterId")
                        .HasColumnType("integer")
                        .HasColumnName("login_character_id");

                    b.Property<int>("LogoutCharacterId")
                        .HasColumnType("integer")
                        .HasColumnName("logout_character_id");

                    b.Property<int>("NumberOfMatches")
                        .HasColumnType("integer")
                        .HasColumnName("number_of_matches");

                    b.HasKey("CorrelationId")
                        .HasName("pk_character_correlations");

                    b.HasIndex("LoginCharacterId")
                        .HasDatabaseName("ix_character_correlations_login_character_id");

                    b.HasIndex("LogoutCharacterId")
                        .HasDatabaseName("ix_character_correlations_logout_character_id");

                    b.HasIndex("NumberOfMatches")
                        .HasDatabaseName("ix_character_correlations_number_of_matches");

                    b.ToTable("character_correlations", "public");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.OnlineCharacter", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<DateTime>("OnlineDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("online_date_time");

                    b.Property<string>("WorldName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("world_name");

                    b.HasKey("Name", "OnlineDateTime")
                        .HasName("pk_online_characters");

                    b.HasIndex("WorldName")
                        .HasDatabaseName("ix_online_characters_world_name");

                    b.ToTable("online_characters", "public");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.TrackedCharacter", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<string>("ConnectionId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("connection_id");

                    b.Property<DateTime>("StartTrackDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_track_date_time");

                    b.Property<string>("WorldName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("world_name");

                    b.HasKey("Name", "ConnectionId")
                        .HasName("pk_tracked_characters");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_tracked_characters_name");

                    b.HasIndex("WorldName")
                        .HasDatabaseName("ix_tracked_characters_world_name");

                    b.ToTable("tracked_characters", "public");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.World", b =>
                {
                    b.Property<short>("WorldId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint")
                        .HasColumnName("world_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<short>("WorldId"));

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean")
                        .HasColumnName("is_available");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("name");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("url");

                    b.HasKey("WorldId")
                        .HasName("pk_worlds");

                    b.ToTable("worlds", "public");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.WorldScan", b =>
                {
                    b.Property<int>("WorldScanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("world_scan_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("WorldScanId"));

                    b.Property<string>("CharactersOnline")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("characters_online");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_deleted");

                    b.Property<DateTime>("ScanCreateDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scan_create_date_time");

                    b.Property<short>("WorldId")
                        .HasColumnType("smallint")
                        .HasColumnName("world_id");

                    b.HasKey("WorldScanId")
                        .HasName("pk_world_scans");

                    b.HasIndex("IsDeleted")
                        .HasDatabaseName("ix_world_scans_is_deleted");

                    b.HasIndex("ScanCreateDateTime")
                        .HasDatabaseName("ix_world_scans_scan_create_date_time");

                    b.HasIndex("WorldId")
                        .HasDatabaseName("ix_world_scans_world_id");

                    b.HasIndex("WorldId", "IsDeleted")
                        .HasDatabaseName("ix_world_scan_id_world_id_is_deleted");

                    b.HasIndex("WorldId", "IsDeleted", "ScanCreateDateTime")
                        .HasDatabaseName("ix_world_scan_world_id_is_deleted_scan_date_time");

                    b.ToTable("world_scans", "public");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.Character", b =>
                {
                    b.HasOne("TibiaStalker.Domain.Entities.World", "World")
                        .WithMany("Characters")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_characters_worlds_world_id");

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.CharacterCorrelation", b =>
                {
                    b.HasOne("TibiaStalker.Domain.Entities.Character", "LoginCharacter")
                        .WithMany("LoginCharacterCorrelations")
                        .HasForeignKey("LoginCharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_character_correlations_characters_login_character_id");

                    b.HasOne("TibiaStalker.Domain.Entities.Character", "LogoutCharacter")
                        .WithMany("LogoutCharacterCorrelations")
                        .HasForeignKey("LogoutCharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_character_correlations_characters_character_id");

                    b.Navigation("LoginCharacter");

                    b.Navigation("LogoutCharacter");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.WorldScan", b =>
                {
                    b.HasOne("TibiaStalker.Domain.Entities.World", "World")
                        .WithMany("WorldScans")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_world_scans_worlds_world_id");

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.Character", b =>
                {
                    b.Navigation("LoginCharacterCorrelations");

                    b.Navigation("LogoutCharacterCorrelations");
                });

            modelBuilder.Entity("TibiaStalker.Domain.Entities.World", b =>
                {
                    b.Navigation("Characters");

                    b.Navigation("WorldScans");
                });
#pragma warning restore 612, 618
        }
    }
}

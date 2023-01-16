﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TibiaEnemyOtherCharactersFinder.Infrastructure.Entities;

#nullable disable

namespace TibiaEnemyOtherCharactersFinder.Api.Migrations
{
    [DbContext(typeof(TibiaCharacterFinderDbContext))]
    [Migration("20230115180033_AddLogoutOrLoginDate_to_CharacterActions")]
    partial class AddLogoutOrLoginDate_to_CharacterActions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.Character", b =>
                {
                    b.Property<int>("CharacterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("character_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CharacterId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<short>("WorldId")
                        .HasColumnType("smallint")
                        .HasColumnName("world_id");

                    b.HasKey("CharacterId")
                        .HasName("pk_characters");

                    b.HasIndex("WorldId")
                        .HasDatabaseName("ix_characters_world_id");

                    b.ToTable("characters", "public");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.CharacterAction", b =>
                {
                    b.Property<int>("CharacterActionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("character_action_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CharacterActionId"));

                    b.Property<string>("CharacterName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("character_name");

                    b.Property<bool>("IsOnline")
                        .HasColumnType("boolean")
                        .HasColumnName("is_online");

                    b.Property<DateOnly>("LogoutOrLoginDate")
                        .HasColumnType("date")
                        .HasColumnName("logout_or_login_date");

                    b.Property<short>("WorldId")
                        .HasColumnType("smallint")
                        .HasColumnName("world_id");

                    b.Property<int>("WorldScanId")
                        .HasColumnType("integer")
                        .HasColumnName("world_scan_id");

                    b.HasKey("CharacterActionId")
                        .HasName("pk_character_actions");

                    b.HasIndex("WorldId")
                        .HasDatabaseName("ix_character_actions_world_id");

                    b.HasIndex("WorldScanId")
                        .HasDatabaseName("ix_character_actions_world_scan_id");

                    b.ToTable("character_actions", "public");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.CharacterCorrelation", b =>
                {
                    b.Property<int>("CorrelationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("correlation_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CorrelationId"));

                    b.Property<DateOnly>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("date")
                        .HasDefaultValue(new DateOnly(2022, 12, 6))
                        .HasColumnName("create_date");

                    b.Property<DateOnly>("LastMatchDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("date")
                        .HasDefaultValue(new DateOnly(2022, 12, 6))
                        .HasColumnName("last_match_date");

                    b.Property<int>("LoginCharacterId")
                        .HasColumnType("integer")
                        .HasColumnName("login_character_id");

                    b.Property<int>("LogoutCharacterId")
                        .HasColumnType("integer")
                        .HasColumnName("logout_character_id");

                    b.Property<short>("NumberOfMatches")
                        .HasColumnType("smallint")
                        .HasColumnName("number_of_matches");

                    b.HasKey("CorrelationId")
                        .HasName("pk_character_correlations");

                    b.HasIndex("LoginCharacterId")
                        .HasDatabaseName("ix_character_correlations_login_character_id");

                    b.HasIndex("LogoutCharacterId")
                        .HasDatabaseName("ix_character_correlations_logout_character_id");

                    b.ToTable("character_correlations", "public");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.World", b =>
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
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("url");

                    b.HasKey("WorldId")
                        .HasName("pk_worlds");

                    b.ToTable("worlds", "public");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.WorldScan", b =>
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

                    b.HasIndex("WorldId")
                        .HasDatabaseName("ix_world_scans_world_id");

                    b.ToTable("world_scans", "public");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.Character", b =>
                {
                    b.HasOne("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.World", "World")
                        .WithMany("Characters")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_characters_worlds_world_id");

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.CharacterAction", b =>
                {
                    b.HasOne("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.World", "World")
                        .WithMany("CharacterLogoutOrLogins")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_character_actions_worlds_world_id");

                    b.HasOne("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.WorldScan", "WorldScan")
                        .WithMany()
                        .HasForeignKey("WorldScanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_character_actions_world_scans_world_scan_id");

                    b.Navigation("World");

                    b.Navigation("WorldScan");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.CharacterCorrelation", b =>
                {
                    b.HasOne("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.Character", "LoginCharacter")
                        .WithMany("LoginWorldCorrelations")
                        .HasForeignKey("LoginCharacterId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_character_correlations_characters_login_character_id");

                    b.HasOne("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.Character", "LogoutCharacter")
                        .WithMany("LogoutWorldCorrelations")
                        .HasForeignKey("LogoutCharacterId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_character_correlations_characters_character_id");

                    b.Navigation("LoginCharacter");

                    b.Navigation("LogoutCharacter");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.WorldScan", b =>
                {
                    b.HasOne("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.World", "World")
                        .WithMany("WorldScans")
                        .HasForeignKey("WorldId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_world_scans_worlds_world_id");

                    b.Navigation("World");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.Character", b =>
                {
                    b.Navigation("LoginWorldCorrelations");

                    b.Navigation("LogoutWorldCorrelations");
                });

            modelBuilder.Entity("TibiaEnemyOtherCharactersFinder.Infrastructure.Entities.World", b =>
                {
                    b.Navigation("CharacterLogoutOrLogins");

                    b.Navigation("Characters");

                    b.Navigation("WorldScans");
                });
#pragma warning restore 612, 618
        }
    }
}

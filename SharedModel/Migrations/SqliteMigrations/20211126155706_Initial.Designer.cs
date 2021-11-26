﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedModel;

#nullable disable

namespace SharedModel.Migrations.SqliteMigrations
{
    [DbContext(typeof(SqliteCardContext))]
    [Migration("20211126155706_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.0");

            modelBuilder.Entity("SharedModel.Card", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Layout")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("SharedModel.Face", b =>
                {
                    b.Property<Guid>("CardId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Sequence")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Loyalty")
                        .HasColumnType("TEXT");

                    b.Property<string>("ManaCost")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OracleText")
                        .HasColumnType("TEXT");

                    b.Property<string>("Power")
                        .HasColumnType("TEXT");

                    b.Property<string>("Toughness")
                        .HasColumnType("TEXT");

                    b.Property<string>("TypeLine")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("CardId", "Sequence");

                    b.ToTable("Face");
                });

            modelBuilder.Entity("SharedModel.NameIndex", b =>
                {
                    b.Property<string>("SanitizedName")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CardId")
                        .HasColumnType("TEXT");

                    b.HasKey("SanitizedName");

                    b.HasIndex("CardId");

                    b.ToTable("Index");
                });

            modelBuilder.Entity("SharedModel.Face", b =>
                {
                    b.HasOne("SharedModel.Card", null)
                        .WithMany("Faces")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SharedModel.NameIndex", b =>
                {
                    b.HasOne("SharedModel.Card", "Card")
                        .WithMany()
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");
                });

            modelBuilder.Entity("SharedModel.Card", b =>
                {
                    b.Navigation("Faces");
                });
#pragma warning restore 612, 618
        }
    }
}

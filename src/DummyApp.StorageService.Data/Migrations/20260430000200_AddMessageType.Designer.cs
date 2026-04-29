using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DummyApp.StorageService.Data.Migrations
{
    [DbContext(typeof(StorageDbContext))]
    [Migration("20260430000200_AddMessageType")]
    partial class AddMessageType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("DummyApp.StorageService.Data.MessageType", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("longtext");

                b.HasKey("Id");

                b.ToTable("MessageTypes");
            });

            modelBuilder.Entity("DummyApp.StorageService.Data.Message", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<int>("MessageTypeId")
                    .HasColumnType("int");

                b.Property<string>("Text")
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnType("longtext");

                b.HasKey("Id");

                b.HasIndex("MessageTypeId");

                b.ToTable("Messages");
            });

            modelBuilder.Entity("DummyApp.StorageService.Data.Message", b =>
            {
                b.HasOne("DummyApp.StorageService.Data.MessageType", "MessageType")
                    .WithMany("Messages")
                    .HasForeignKey("MessageTypeId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });
        }
    }
}

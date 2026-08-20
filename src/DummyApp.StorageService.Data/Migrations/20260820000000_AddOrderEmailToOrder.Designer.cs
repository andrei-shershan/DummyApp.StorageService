using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DummyApp.StorageService.Data.Migrations
{
    [DbContext(typeof(StorageDbContext))]
    [Migration("20260820000000_AddOrderEmailToOrder")]
    public partial class AddOrderEmailToOrder
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            // This migration does not need a target model implementation.
        }
    }
}

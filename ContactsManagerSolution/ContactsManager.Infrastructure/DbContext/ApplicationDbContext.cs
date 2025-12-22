using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Entities
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Country> Countries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");


            string countriesjson =  System.IO.File.ReadAllText("countries.json");

            List<Country> countries= System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesjson);

            foreach(Country country in countries)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }


            string personjson = System.IO.File.ReadAllText("persons.json");

            List<Person> persons =  System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personjson);

            foreach(Person person in persons)
            {
                modelBuilder.Entity<Person>().HasData(person);
            }


            //fluent API

            modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABC12345");

            modelBuilder.Entity<Person>()
                .HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");
        }

        public List<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
        }

        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[] {
            new SqlParameter("@PersonID", person.PersonID),
            new SqlParameter("@PersonName", person.PersonName),
            new SqlParameter("@Email", person.Email),
            new SqlParameter("@DateOfBirth", person.DateOfBirth),
            new SqlParameter("@Gender", person.Gender),
            new SqlParameter("@CountryID", person.CountryID),
            new SqlParameter("@Address", person.Address),
            new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters),
            new SqlParameter("@TIN", person.TIN)
            };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters, @TIN", parameters);
        }


    }


}

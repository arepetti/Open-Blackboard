using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OpenBlackboard.Hosting;

namespace OpenBlackboard.Model.Storage
{
    /// <summary>
    /// Represents the entire set of collected values.
    /// </summary>
    public sealed class BackboardContext : DbContext
    {
        /// <summary>
        /// Creates a new <see cref="BackboardContext"/> object
        /// usinig the specified connection string or the configured one if omitted.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string used to connect to the database. If <see langword="null"/>
        /// the connection string in configuration file will be used (named with the same name
        /// of this class). If configuration file isn't present or it has not any connection string
        /// configured then default one is used (assume local MSSS and integrated security).
        /// </param>
        public BackboardContext(string connectionString = null)
        {
            _connectionString = connectionString
                ?? Settings.Default.GetConnectionString(nameof(BackboardContext))
                ?? DefaultConnectionString;
        }

        /// <summary>
        /// Gets/sets the list of protocols associated with submissions.
        /// </summary>
        /// <value>
        /// The list of <see cref="Protocol"/> optionally associated with submission.
        /// </value>
        public DbSet<Protocol> Protocols { get; set; }

        /// <summary>
        /// Gets/sets the list of known centers that can submit data.
        /// </summary>
        /// <value>
        /// The list of known <see cref="Center"/> that can submit data.
        /// </value>
        public DbSet<Center> Centers { get; set; }

        /// <summary>
        /// Gets/sets the list of all submissions.
        /// </summary>
        /// <value>
        /// The list of all <see cref="Submission"/>.
        /// </value>
        public DbSet<Submission> Submissions { get; set; }

        /// <summary>
        /// Gets/sets the list of all submitted values.
        /// </summary>
        /// <value>
        /// The list of all submitted <see cref="Value"/>/
        /// </value>
        public DbSet<Value> Values { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Center>()
                .HasMany<Submission>()
                .WithOne((string)null)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Submission>()
                .HasOne<Protocol>()
                .WithMany((string)null);

            modelBuilder.Entity<Submission>()
                .HasMany<Value>()
                .WithOne((string)null)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private const string DefaultConnectionString = "Server=.;Database=OpenBlackboard;Trusted_Connection=True;MultipleActiveResultSets=true;";
        private readonly string _connectionString;
    }
}

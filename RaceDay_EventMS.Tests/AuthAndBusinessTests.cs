using Microsoft.EntityFrameworkCore;
using RaceDay_EventMS.Data;
using RaceDay_EventMS.Models;
using Xunit;

namespace RaceDay_EventMS.Tests
{

    /// <summary>
    /// Integration and unit test suite for core authentication and business logic.
    /// Uses Entity Framework Core InMemory database fixtures.
    /// </summary>
    public class AuthAndBusinessTests
    {

        /// <summary>
        /// Tests that a user is created with the expected role and email persistence.
        /// </summary>
        [Fact]
        public async Task UserRegistration_StoresCorrectRoleAndEmail()
        {
            // Arrange - Use InMemory database for fast isolated unit testing
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: "RaceDayTestDb")
                .Options;

            using var context = new RaceDayDbContext(options);

            // Act
            var user = new User
            {
                Email = "testorganiser@raceday.com",
                PasswordHash = "hashed_fake_password",
                Role = "Organiser"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "testorganiser@raceday.com");
            Assert.NotNull(savedUser);
            Assert.Equal("Organiser", savedUser.Role);
        }

        [Fact]
        public async Task Enrolment_LinksParticipantToCategoryCorrectly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: "RaceDayTestDb_Enrolment")
                .Options;

            using var context = new RaceDayDbContext(options);

            var participant = new User { Email = "runner@raceday.com", PasswordHash = "hash", Role = "Participant" };
            var category = new Category
            {
                CategoryName = "10km Run",
                DistanceKm = 10m, // Assuming 10km as decimal
                EntryFee = 0m,    // Provide required properties with default values if needed
                MaxParticipants = 100 // Example value, adjust as appropriate
            };

            context.Users.Add(participant);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // Act
            var enrolment = new Enrolment
            {
                ParticipantId = participant.UserId,
                CategoryId = category.CategoryId,
                EnrolmentDate = DateTime.UtcNow
            };

            context.Enrolments.Add(enrolment);
            await context.SaveChangesAsync();

            // Assert
            var savedEnrolment = await context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .FirstOrDefaultAsync();

            Assert.NotNull(savedEnrolment);
            Assert.Equal("runner@raceday.com", savedEnrolment.Participant?.Email);
            Assert.Equal("10km Run", savedEnrolment.Category?.CategoryName);
        }
    }
}
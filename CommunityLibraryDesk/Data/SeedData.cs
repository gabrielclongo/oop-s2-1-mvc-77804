using Bogus;
using Library.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CommunityLibraryDesk.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            context.Database.EnsureCreated();

            // CREATE ADMIN ROLE
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
            }

            // STOP IF BOOKS ALREADY EXIST
            if (context.Books.Any())
            {
                return;
            }

            // BOOKS
            var books = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.Isbn, f => f.Random.Replace("###-########"))
                .RuleFor(b => b.Category, f => f.PickRandom(new[]
                {
                    "Programming",
                    "Fantasy",
                    "Science",
                    "History",
                    "Self Help"
                }))
                .RuleFor(b => b.IsAvailable, true)
                .Generate(20);

            context.Books.AddRange(books);
            context.SaveChanges();

            // MEMBERS
            var members = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Name.FullName())
                .RuleFor(m => m.Email, f => f.Internet.Email())
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber())
                .Generate(10);

            context.Members.AddRange(members);
            context.SaveChanges();

            var random = new Random();

            // LOANS
            var loans = new List<Loan>();

            for (int i = 0; i < 15; i++)
            {
                loans.Add(new Loan
                {
                    BookId = books[random.Next(books.Count)].Id,
                    MemberId = members[random.Next(members.Count)].Id,
                    LoanDate = DateTime.Now.AddDays(-random.Next(1, 10)),
                    DueDate = DateTime.Now.AddDays(random.Next(5, 15)),
                    ReturnedDate = random.Next(2) == 0 ? null : DateTime.Now
                });
            }

            context.Loans.AddRange(loans);
            context.SaveChanges();
        }
    }
}
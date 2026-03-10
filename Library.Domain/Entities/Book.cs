namespace Library.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Isbn { get; set; }

        public string Category { get; set; }

        public bool IsAvailable { get; set; }

        public List<Loan> Loans { get; set; }
    }
}
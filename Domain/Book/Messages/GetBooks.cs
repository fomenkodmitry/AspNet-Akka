namespace Domain.Book.Messages
{
    public class GetBooks
    {
        private GetBooks() { }
        public static GetBooks Instance { get; } = new();
    }
}
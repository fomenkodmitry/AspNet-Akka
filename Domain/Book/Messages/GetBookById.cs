using System;

namespace Domain.Book.Messages
{
    public class GetBookById
    {
        public GetBookById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class BookNotFound
    {
        private BookNotFound() { }
        public static BookNotFound Instance { get; } = new BookNotFound();
    }

}
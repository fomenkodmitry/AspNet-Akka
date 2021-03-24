using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Domain.Book;
using Domain.Book.Dto;
using Domain.Book.Messages;

namespace Actor
{
    public class BooksManagerActor : ReceiveActor
    {
        private readonly Dictionary<Guid, Book> _books = new Dictionary<Guid, Book>();

        public BooksManagerActor()
        {
            Receive<CreateBook>(command =>
            {
                var newBook = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = command.Title,
                    Author = command.Author,
                    Cost = command.Cost,
                    InventoryAmount = command.InventoryAmount,
                };

                _books.Add(newBook.Id, newBook);
            });

            Receive<GetBookById>(query =>
            {
                if (_books.TryGetValue(query.Id, out var book))
                    Sender.Tell(GetBookDto(book));
                else
                    Sender.Tell(BookNotFound.Instance);
            });

            Receive<GetBooks>(query => Sender.Tell(_books.Select(x => GetBookDto(x.Value)).ToList()));
       
            Receive<GetPath>(query => Sender.Tell(Self.Path.Address));
            
        }

        private static BookDto GetBookDto(Book book) => new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Cost = book.Cost,
            InventoryAmount = book.InventoryAmount
        };
    }
}
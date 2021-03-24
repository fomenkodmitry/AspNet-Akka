using Akka.Actor;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Domain.Book.Dto;
using Domain.Book.Messages;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IActorRef _booksManagerActor;

        public BooksController(BooksManagerActorProvider booksManagerActorProvider)
        {
            _booksManagerActor = booksManagerActorProvider();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var books = await _booksManagerActor.Ask<IEnumerable<BookDto>>(GetBooks.Instance);
            return Ok(books);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _booksManagerActor.Ask(new GetBookById(id));
            switch (result)
            {
                case BookDto book:
                    return Ok(book);
                default:
                    return BadRequest();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Path")]
        public async Task<IActionResult> Path()
        {
            var books = await _booksManagerActor.Ask<Address>(GetPath.Instance);
            return Ok(books);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] CreateBook command)
        {
            _booksManagerActor.Tell(command);
            return Accepted();
        }
    }
}

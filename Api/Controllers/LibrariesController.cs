﻿using Api.Libraries;
using Application.Application.Caching;
using Application.Library.Commands.AddBook;
using Application.Library.Commands.Create;
using Application.Library.Commands.Delete;
using Application.Library.Commands.RemoveBook;
using Application.Library.Commands.Update;
using Application.Library.Queries.GetAll;
using Application.Library.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CleanArchitectire.Controllers;

[ApiController]
[Route("api/[controller]")]

public class LibrariesController : Controller
{
    private readonly IDistributedCache _cache;
    private readonly IMediator _mediator;

    public LibrariesController(IMediator mediator, IDistributedCache cache)
    {
        _cache = cache;
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Library>>> GetAll()
    {
        var library = await _mediator.Send(new GetAllLibrariesQuery());
        
        return Ok(library);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Library>> GetById([FromRoute] int id)
    {
        string key = $"library-{id}";
        
        var library = await _cache.GetOrCreateAsync(key, async token =>
        { 
            var library = await _mediator.Send(new GetByIdLibraryQuery { Id = id });
            return library;
        });
        
        return Ok(library);

    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateLibraryCommand command)
    {
        var libraryId = await _mediator.Send(command);

        return Created("", libraryId);
    }

    [HttpPut("{id}")]
    public async Task<NoContentResult> Update([FromRoute] int id, [FromBody] UpdateLibraryCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _mediator.Send(new DeleteLibraryCommand()
        {
            Id = id
        });
        
        return NoContent();
    }
    
    [HttpPost("{libraryId}/books/{bookId}")]
    public async Task<IActionResult> AddBook([FromRoute] int libraryId, [FromRoute] int bookId )
    {
        await _mediator.Send(new AddBookToLibraryCommand()
        {
            Id = libraryId,
            BookId = bookId,
        });

        return NoContent();
    }
    
    [HttpDelete("{libraryId}/books/{bookId}")]
    public async Task<IActionResult> RemoveBookFromPerson([FromRoute] int libraryId, [FromRoute] int bookId)
    {
        await _mediator.Send(new RemoveBookFromLibraryCommand()
        {
            Id = libraryId,
            BookId = bookId,
        });

        return NoContent();
    }
}
﻿using Api.People;
using Application.Application.Caching;
using Application.People.Commands.AddBook;
using Application.People.Commands.Create;
using Application.People.Commands.Delete;
using Application.People.Commands.RemoveBook;
using Application.People.Commands.Update;
using Application.People.Queries.GetAll;
using Application.People.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CleanArchitectire.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PeopleController : Controller
{
    private readonly IDistributedCache _cache;
    private readonly IMediator _mediator;

    public PeopleController(IMediator mediator, IDistributedCache cache)
    {
        _cache = cache;
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Person>>> GetAll()
    {
        var people = await _mediator.Send(new GetAllPeopleQuery());
        
        return Ok(people);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> GetById([FromRoute] int id)
    {
        string key = $"person-{id}";
        
        var person = await _cache.GetOrCreateAsync(key, async token => 
        {
            var person = await _mediator.Send(new GetByIdPersonQuery { Id = id });
            return person;
        });

        return Ok(person);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreatePersonCommand command)
    {
        var personId = await _mediator.Send(command);

        return Created("", personId);
    }

    [HttpPut("{id}")]
    public async Task<NoContentResult> Update([FromRoute] int id, [FromBody] UpdatePersonCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await _mediator.Send(new DeletePersonCommand()
        {
            Id = id
        });

        return NoContent();
    }
    
    [HttpPost("{personId}/books/{bookId}")]
    public async Task<IActionResult> AddBook([FromRoute] int personId, [FromRoute] int bookId )
    {
        await _mediator.Send(new AddBookToPersonCommand()
        {
            Id = personId,
            BookId = bookId,
        });

        return NoContent();
    }
    
    [HttpDelete("{personId}/books/{bookId}")]
    public async Task<IActionResult> RemoveBookFromPerson([FromRoute] int personId, [FromRoute] int bookId)
    {
        await _mediator.Send(new RemoveBookFromPersonCommand()
        {
            Id = personId,
            BookId = bookId,
        });

        return NoContent();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentistService.API.Models;

namespace DentistService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DentistsController : ControllerBase
{
    private readonly DentistDbContext _db;

    public DentistsController(DentistDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dentist>>> GetAll()
        => await _db.Dentists.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Dentist>> GetById(Guid id)
    {
        var dentist = await _db.Dentists.FindAsync(id);
        if (dentist is null) return NotFound();
        return dentist;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Dentist>> Create(Dentist input)
    {
        var dentist = new Dentist
        {
            FullName = input.FullName,
            Specialty = input.Specialty,
            Email = input.Email
        };

        _db.Dentists.Add(dentist);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dentist.Id }, dentist);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Dentist input)
    {
        var dentist = await _db.Dentists.FindAsync(id);
        if (dentist is null) return NotFound();

        dentist.FullName = input.FullName;
        dentist.Specialty = input.Specialty;
        dentist.Email = input.Email;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dentist = await _db.Dentists.FindAsync(id);
        if (dentist is null) return NotFound();

        _db.Dentists.Remove(dentist);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

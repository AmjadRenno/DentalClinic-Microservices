using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.API.Models;

namespace PatientService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly PatientDbContext _db;

    public PatientsController(PatientDbContext db)
    {
        _db = db;
    }

    // 🔓 يمكن لأي مستخدم مسجّل رؤية القائمة
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetAll()
        => await _db.Patients.ToListAsync();

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Patient>> GetById(Guid id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient is null) return NotFound();
        return patient;
    }

    // 🔒 فقط Admin يمكنه إنشاء/تعديل/حذف
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Patient>> Create(Patient input)
    {
        var patient = new Patient
        {
            FullName = input.FullName,
            Email = input.Email,
            Phone = input.Phone,
            DateOfBirth = input.DateOfBirth
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Patient input)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient is null) return NotFound();

        patient.FullName = input.FullName;
        patient.Email = input.Email;
        patient.Phone = input.Phone;
        patient.DateOfBirth = input.DateOfBirth;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient is null) return NotFound();

        _db.Patients.Remove(patient);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

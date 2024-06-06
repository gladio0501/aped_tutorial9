using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial9.Data;
using Tutorial9.Models;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly ApbdContext _context;

    public ClientsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients.Include(c => c.ClientTrips).FirstOrDefaultAsync(c => c.IdClient == idClient);
        if (client == null)
        {
            return NotFound();
        }

        if (client.ClientTrips.Any())
        {
            return BadRequest("Client cannot be deleted as they are assigned to one or more trips.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientDto clientDto)
    {
        if (await _context.Clients.AnyAsync(c => c.Pesel == clientDto.Pesel))
        {
            return BadRequest("Client with given PESEL already exists.");
        }

        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);
        if (trip == null || trip.DateFrom <= DateTime.Now)
        {
            return BadRequest("Trip does not exist or has already occurred.");
        }

        var client = new Client
        {
            FirstName = clientDto.FirstName,
            LastName = clientDto.LastName,
            Email = clientDto.Email,
            Telephone = clientDto.Telephone,
            Pesel = clientDto.Pesel
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            PaymentDate = clientDto.PaymentDate,
            RegisteredAt = DateTime.Now
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return Ok();
    }

}
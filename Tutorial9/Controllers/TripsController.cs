using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial9.Data;
using Tutorial9.Models;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.IdCountries)
            .Include(t => t.ClientTrips)
            .ToListAsync();

        var totalTrips = await _context.Trips.CountAsync();

        return Ok(new
        {
            pageNum = page,
            pageSize,
            allPages = (int)Math.Ceiling(totalTrips / (double)pageSize),
            trips
        });
    }
}
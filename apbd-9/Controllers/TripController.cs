using apbd_9.Context;
using apbd_9.DTO;
using apbd_9.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd_9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly TripsContext _context;

        public TripsController(TripsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (_context.Trips == null)
            {
                return NotFound();
            }

            var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TripDto()
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,

                Countries = t.IdCountries.Select(ct => new CountryDto
                {
                    Name = ct.Name
                }).ToList(),

                Clients = t.ClientTrips.Select(ct => new ClientDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            }).ToListAsync();

            var totalTrips = await _context.Trips.CountAsync();
            var totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);

            return Ok(new
            {
                pageNum = page,
                pageSize,
                allPages = totalPages,
                trips
            });
        }
        
        [HttpPost("{idTrip}/clients")]
        public async Task<ActionResult> AddClientToTrip(TripaddClientDto tripaddClientDto, int idTrip)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == tripaddClientDto.Pesel);
            if (client == null)
            {
                return BadRequest("Client with the given PESEL number does not exist.");
            }

            var trip_client = await _context.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip);
            if (trip_client != null)
            {
                return BadRequest("The client is already registered for the trip.");
            }
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);
            if (trip == null)
            {
                return BadRequest("Trip with the given ID does not exist.");
            }

            if (trip.DateFrom < DateTime.Now)
            {
                return BadRequest("The trip has already occurred.");
            }

            var clientTrip = await _context.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == trip.IdTrip);
            if (clientTrip != null)
            {
                return BadRequest("The client is already registered for the trip.");
            }

            var newClientTrip = new ClientTrip
            {
                IdClient = client.IdClient,
                IdTrip = trip.IdTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = tripaddClientDto.PaymentDate
            };

            _context.ClientTrips.Add(newClientTrip);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
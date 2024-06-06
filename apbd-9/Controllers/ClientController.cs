using apbd_9.Context;
using Microsoft.AspNetCore.Mvc;

namespace apbd_9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly TripsContext _dbContext;

        public ClientsController(TripsContext dbContext)
        {
            _dbContext = dbContext;
        }
        private bool ClientExists(int id)
        {
            return _dbContext.Clients.Any(e => e.IdClient == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientById(int id)
        {
            var client = await _dbContext.Clients.FindAsync(id);
            if (!ClientExists(client.IdClient))
            {
                return NotFound("Client not found.");
            }

            _dbContext.Clients.Remove(client);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        
    }
}

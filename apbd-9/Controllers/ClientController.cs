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

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientById(int id)
        {
            var client = await _dbContext.Clients.FindAsync(id);
            if (client != null && !ClientExists(client.IdClient))
            {
                return NotFound("Client not found.");
            }

            if (client != null) _dbContext.Clients.Remove(client);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        
        private bool ClientExists(int id)
        {
            return _dbContext.Clients.Any(e => e.IdClient == id);
        }
    }
}
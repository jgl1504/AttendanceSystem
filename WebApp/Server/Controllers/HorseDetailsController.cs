using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using WebApp.Server.Services;
using WebApp.Server.Data;
using WebApp.Shared.Model.HorsesDetails;
using WebApp.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorseDetailsController : ControllerBase
    {
        private readonly DataContext _context;

        public HorseDetailsController(DataContext context)
        {
            _context = context;
        }

      

        // PUT: api/HorseDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHorseDetail(int id, HorseDetail horseDetail)
        {
            if (id != horseDetail.Id)
            {
                return BadRequest();
            }

            try
            {
                await HorseDetailService.SaveHorseDetail(_context, horseDetail, false, id);

                await _context.SaveChangesAsync();
            }
            catch (DuplicateNameException)
            {
                return new ContentResult
                {
                    Content = $"Name: {horseDetail.Name} already exist",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HorseDetailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            return NoContent();
        }

        // POST: api/HorseDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<HorseDetail>> PostHorseDetail(HorseDetail horseDetail)
        {
            if (_context.HorseDetail == null)
            {
                return Problem("Entity set 'DataContext.HorseDetail'  is null.");
            }
            try
            {
                await HorseDetailService.SaveHorseDetail(_context, horseDetail, true);

                _context.HorseDetail.Add(horseDetail);
                await _context.SaveChangesAsync();
            }
            catch (DuplicateNameException)
            {
                return new ContentResult
                {
                    Content = $"Name: {horseDetail.Name} already exist",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                return new ContentResult
                {
                    Content = $"Exception: {ex.Message}",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
            return CreatedAtAction("GetHorseDetail", new { id = horseDetail.Id }, horseDetail);
        }

        // DELETE: api/HorseDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHorseDetail(int id)
        {
            if (_context.HorseDetail == null)
            {
                return NotFound();
            }
            var horseDetail = await _context.HorseDetail.FindAsync(id);
            if (horseDetail == null)
            {
                return NotFound();
            }

            _context.HorseDetail.Remove(horseDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HorseDetailExists(int id)
        {
            return (_context.HorseDetail?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

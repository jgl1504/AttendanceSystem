using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Attendance;

public class SiteService
{
    private readonly DataContext _context;

    public SiteService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Site>> GetAllAsync()
    {
        return await _context.Sites
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<Site>> GetActiveAsync()
    {
        return await _context.Sites
            .Where(s => s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Site?> GetByIdAsync(Guid id)
    {
        return await _context.Sites
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Site> CreateAsync(Site site)
    {
        // Ensure new ID
        if (site.Id == Guid.Empty)
            site.Id = Guid.NewGuid();

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();
        return site;
    }

    public async Task<bool> UpdateAsync(Site site)
    {
        var existing = await _context.Sites
            .FirstOrDefaultAsync(s => s.Id == site.Id && !s.IsDeleted);

        if (existing is null)
            return false;

        existing.Name = site.Name;
        existing.Address = site.Address;
        existing.Latitude = site.Latitude;
        existing.Longitude = site.Longitude;
        existing.MapUrl = site.MapUrl;
        existing.PreferredMapApp = site.PreferredMapApp;
        existing.IsActive = site.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var site = await _context.Sites
            .FirstOrDefaultAsync(s => s.Id == id);

        if (site is null)
            return false;

        // Soft delete to keep history
        site.IsDeleted = true;
        site.IsActive = false;

        await _context.SaveChangesAsync();
        return true;
    }
}

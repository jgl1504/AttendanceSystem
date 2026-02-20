using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services
{
    public class CompanyService
    {
        private readonly DataContext _context;

        public CompanyService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company> CreateAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Company?> UpdateAsync(int id, Company company)
        {
            var existing = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
                return null;

            existing.Name = company.Name;
            existing.Code = company.Code;
            existing.IsActive = company.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
                return false;

            _context.Companies.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Leave;

public class LeaveTypeService
{
    private readonly DataContext _context;

    public LeaveTypeService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveType>> GetAllAsync()
    {
        return await _context.LeaveTypes
            .Include(lt => lt.PrimaryPoolLeaveType)
            .Include(lt => lt.FallbackPoolLeaveType)
            .Where(lt => !lt.IsDeleted)
            .OrderBy(lt => lt.SortOrder)
            .ThenBy(lt => lt.Name)
            .ToListAsync();
    }

    public async Task<List<LeaveType>> GetActiveAsync()
    {
        return await _context.LeaveTypes
            .Where(lt => lt.IsActive && !lt.IsDeleted)
            .OrderBy(lt => lt.SortOrder)
            .ThenBy(lt => lt.Name)
            .ToListAsync();
    }

    public async Task<LeaveType?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveTypes
            .Include(lt => lt.PrimaryPoolLeaveType)
            .Include(lt => lt.FallbackPoolLeaveType)
            .FirstOrDefaultAsync(lt => lt.Id == id && !lt.IsDeleted);
    }

    public async Task<LeaveType> CreateAsync(LeaveType leaveType)
    {
        if (leaveType.Id == Guid.Empty)
            leaveType.Id = Guid.NewGuid();

        leaveType.CreatedAt = DateTime.UtcNow;
        leaveType.IsDeleted = false;

        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();
        return leaveType;
    }

    public async Task<bool> UpdateAsync(LeaveType leaveType)
    {
        var existing = await _context.LeaveTypes
            .FirstOrDefaultAsync(lt => lt.Id == leaveType.Id && !lt.IsDeleted);

        if (existing == null)
            return false;

        existing.Name = leaveType.Name;
        existing.Description = leaveType.Description;
        existing.ColorCode = leaveType.ColorCode;
        existing.IsActive = leaveType.IsActive;
        existing.PoolType = leaveType.PoolType;
        existing.PrimaryPoolLeaveTypeId = leaveType.PrimaryPoolLeaveTypeId;
        existing.FallbackPoolLeaveTypeId = leaveType.FallbackPoolLeaveTypeId;
        existing.AccrualType = leaveType.AccrualType;
        existing.DaysPerYear = leaveType.DaysPerYear;
        existing.AccrualCycleDurationMonths = leaveType.AccrualCycleDurationMonths;
        existing.DaysPerCycle = leaveType.DaysPerCycle;
        existing.AllowsCarryover = leaveType.AllowsCarryover;
        existing.MaxCarryoverDays = leaveType.MaxCarryoverDays;
        existing.RequiresSupportingDocument = leaveType.RequiresSupportingDocument;
        existing.RequiresApproval = leaveType.RequiresApproval;
        existing.MinNoticeDays = leaveType.MinNoticeDays;
        existing.MaxConsecutiveDays = leaveType.MaxConsecutiveDays;

        // New flag
        existing.AllowsHalfDays = leaveType.AllowsHalfDays;

        existing.IsPaid = leaveType.IsPaid;
        existing.PaymentPercentage = leaveType.PaymentPercentage;
        existing.IsGenderSpecific = leaveType.IsGenderSpecific;
        existing.RequiredGender = leaveType.RequiredGender;
        existing.SortOrder = leaveType.SortOrder;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> DeleteAsync(Guid id)
    {
        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(lt => lt.Id == id);

        if (leaveType == null)
            return false;

        // Soft delete
        leaveType.IsDeleted = true;
        leaveType.IsActive = false;
        leaveType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LeaveType>> GetPoolTypesAsync()
    {
        // Return leave types that have their own pool (for dropdown when selecting pools)
        return await _context.LeaveTypes
            .Where(lt => lt.PoolType == LeavePoolType.OwnPool && lt.IsActive && !lt.IsDeleted)
            .OrderBy(lt => lt.Name)
            .ToListAsync();
    }
}

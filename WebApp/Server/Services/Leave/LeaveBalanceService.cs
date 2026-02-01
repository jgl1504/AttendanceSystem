using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Leave
{
    public class LeaveBalanceService
    {
        private readonly DataContext _context;

        public LeaveBalanceService(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get leave balances, optionally filtered by employeeId.
        /// </summary>
        public async Task<List<LeaveBalanceEditDto>> GetLeaveBalancesAsync(int? employeeId = null)
        {
            var query = _context.LeaveBalances
                .Include(b => b.Employee)
                .Include(b => b.LeaveType)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(b => b.EmployeeId == employeeId.Value);
            }

            return await query
                .OrderBy(b => b.Employee.Name)
                .ThenBy(b => b.LeaveType.SortOrder)
                .Select(b => new LeaveBalanceEditDto
                {
                    Id = b.Id,
                    EmployeeId = b.EmployeeId,
                    EmployeeName = b.Employee.Name,
                    LeaveTypeId = b.LeaveTypeId,
                    LeaveTypeName = b.LeaveType.Name,
                    BalanceStartDate = b.BalanceStartDate,
                    OpeningBalance = b.OpeningBalance,
                    CurrentBalance = b.CurrentBalance
                })
                .ToListAsync();
        }

        /// <summary>
        /// Update a single leave balance row (start date, opening, current).
        /// </summary>
        public async Task<bool> UpdateLeaveBalanceAsync(Guid id, LeaveBalanceEditDto dto)
        {
            var entity = await _context.LeaveBalances.FindAsync(id);
            if (entity == null)
                return false;

            entity.BalanceStartDate = dto.BalanceStartDate;
            entity.OpeningBalance = dto.OpeningBalance;
            entity.CurrentBalance = dto.CurrentBalance;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> InitializeMissingBalancesAsync(DateTime? balanceStartDate = null, decimal defaultOpening = 0m)
        {
            var startDate = balanceStartDate?.Date ?? DateTime.UtcNow.Date;

            // Only leave types that have their own pool / one-time allocation
            var leaveTypes = await _context.LeaveTypes
                .Where(lt => lt.PoolType == LeavePoolType.OwnPool
                          || lt.PoolType == LeavePoolType.OneTime)
                .ToListAsync();

            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new { e.Id })
                .ToListAsync();

            var existing = await _context.LeaveBalances
                .Select(b => new { b.EmployeeId, b.LeaveTypeId })
                .ToListAsync();

            var toInsert = new List<LeaveBalance>();

            foreach (var e in employees)
            {
                foreach (var lt in leaveTypes)
                {
                    // Skip if already has a balance row
                    if (existing.Any(x => x.EmployeeId == e.Id && x.LeaveTypeId == lt.Id))
                        continue;

                    toInsert.Add(new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = e.Id,
                        LeaveTypeId = lt.Id,
                        BalanceStartDate = startDate,
                        OpeningBalance = defaultOpening,
                        CurrentBalance = defaultOpening,
                        CurrentCycleStartDate = null,
                        CurrentCycleEndDate = null,
                        HasBeenUsed = false,
                        UsedDate = null,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "InitBalances"
                    });
                }
            }

            if (toInsert.Count > 0)
            {
                _context.LeaveBalances.AddRange(toInsert);
                await _context.SaveChangesAsync();
            }

            return toInsert.Count;
        }

    }
}

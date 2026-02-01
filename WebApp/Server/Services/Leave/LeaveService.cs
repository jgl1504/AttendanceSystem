using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Leave
{
    /// <summary>
    /// Legacy annual leave balance + setup, based on EmployeeLeaves.
    /// New per-pool balances are handled by LeaveBalanceService.
    /// Also provides per-type balance summaries for admin view.
    /// </summary>
    public class LeaveService
    {
        private readonly DataContext _context;

        public LeaveService(DataContext context)
        {
            _context = context;
        }

        // ===== LEGACY EMPLOYEE ANNUAL BALANCE (EmployeeLeaves) =====

        // Get employee's current legacy leave balance (EmployeeLeaves)
        public async Task<EmployeeLeaveDto?> GetLeaveBalanceAsync(int employeeId)
        {
            var leave = await _context.EmployeeLeaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.EmployeeId == employeeId);

            if (leave == null)
                return null;

            await AccrueLeaveIfDueAsync(leave);

            return new EmployeeLeaveDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                EmployeeName = leave.Employee.Name,
                DaysBalance = leave.DaysBalance,
                AccrualRatePerMonth = leave.AccrualRatePerMonth,
                DaysPerWeek = leave.DaysPerWeek,
                LastAccrualDate = leave.LastAccrualDate
            };
        }

        // Auto-accrue monthly leave on the legacy balance
        public async Task AccrueLeaveIfDueAsync(EmployeeLeave leave)
        {
            var today = DateTime.UtcNow;

            if (leave.LastAccrualMonth == 0 || today.Month != leave.LastAccrualMonth)
            {
                leave.DaysBalance += leave.AccrualRatePerMonth;
                leave.LastAccrualDate = today;
                leave.LastAccrualMonth = today.Month;

                _context.EmployeeLeaves.Update(leave);
                await _context.SaveChangesAsync();
            }
        }

        // Initialize legacy leave for new employee
        public async Task InitializeEmployeeLeaveAsync(int employeeId, int daysPerWeek = 5)
        {
            var accrualRate = daysPerWeek == 5 ? 1.25m : 1.5m;

            var leave = new EmployeeLeave
            {
                EmployeeId = employeeId,
                DaysBalance = 0,
                AccrualRatePerMonth = accrualRate,
                DaysPerWeek = daysPerWeek,
                LastAccrualDate = DateTime.UtcNow,
                LastAccrualMonth = DateTime.UtcNow.Month,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EmployeeLeaves.Add(leave);
            await _context.SaveChangesAsync();
        }

        // Legacy setup screen (admin /admin/leave/setup)
        public async Task<List<LeaveSetupRowDto>> GetLeaveSetupAsync()
        {
            var employees = await _context.Employees
                .OrderBy(e => e.Name)
                .ToListAsync();

            var leaves = await _context.EmployeeLeaves.ToListAsync();

            var list = new List<LeaveSetupRowDto>();

            foreach (var e in employees)
            {
                var leave = leaves.FirstOrDefault(l => l.EmployeeId == e.Id);
                list.Add(new LeaveSetupRowDto
                {
                    EmployeeId = e.Id,
                    EmployeeName = e.Name,
                    DaysPerWeek = leave?.DaysPerWeek ?? 5,
                    AccrualRatePerMonth = leave?.AccrualRatePerMonth ?? 1.25m,
                    CurrentBalance = leave?.DaysBalance ?? 0m,
                    NewBalance = leave?.DaysBalance ?? 0m
                });
            }

            return list;
        }

        public async Task<bool> SaveLeaveSetupAsync(LeaveSetupRowDto dto)
        {
            var leave = await _context.EmployeeLeaves
                .FirstOrDefaultAsync(l => l.EmployeeId == dto.EmployeeId);

            if (leave == null)
            {
                leave = new EmployeeLeave
                {
                    EmployeeId = dto.EmployeeId,
                    DaysPerWeek = dto.DaysPerWeek,
                    AccrualRatePerMonth = dto.AccrualRatePerMonth,
                    DaysBalance = dto.NewBalance,
                    LastAccrualDate = DateTime.UtcNow,
                    LastAccrualMonth = DateTime.UtcNow.Month,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.EmployeeLeaves.Add(leave);
            }
            else
            {
                leave.DaysPerWeek = dto.DaysPerWeek;
                leave.AccrualRatePerMonth = dto.AccrualRatePerMonth;
                leave.DaysBalance = dto.NewBalance;
                leave.UpdatedAt = DateTime.UtcNow;
                _context.EmployeeLeaves.Update(leave);
            }

            await _context.SaveChangesAsync();
            return true;
        }       

        // ===== PER-TYPE BALANCE SUMMARY FOR ADMIN (by LeaveType) =====

        // Returns entitlement, taken, remaining for a specific employee + leave type
        public async Task<LeaveBalanceSummaryDto?> GetLeaveBalanceSummaryAsync(int employeeId, Guid leaveTypeId)
        {
            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(lt => lt.Id == leaveTypeId && !lt.IsDeleted);

            if (leaveType == null)
                return null;

            decimal totalEntitlement;

            // If you have per-type balances in LeaveBalances, use that
            var balanceRow = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId);

            if (balanceRow != null)
            {
                // OpeningBalance + CurrentBalance, adjust if your schema is different
                totalEntitlement = balanceRow.OpeningBalance + balanceRow.CurrentBalance;
            }
            else
            {
                // Fallback: use LeaveType configuration (simplified)
                // Annual / Fixed: DaysPerYear, Cycle: DaysPerCycle, Unlimited/None: treat as "unlimited"
                if (leaveType.AccrualType == LeaveAccrualType.Annual ||
                    leaveType.AccrualType == LeaveAccrualType.Fixed)
                {
                    totalEntitlement = leaveType.DaysPerYear;
                }
                else if (leaveType.AccrualType == LeaveAccrualType.Cycle &&
                         leaveType.DaysPerCycle.HasValue)
                {
                    totalEntitlement = leaveType.DaysPerCycle.Value;
                }
                else
                {
                    // Unlimited or None: use a large sentinel value
                    totalEntitlement = 9999m;
                }
            }

            var taken = await _context.LeaveRecords
                .Where(r => r.EmployeeId == employeeId
                            && r.LeaveTypeId == leaveTypeId
                            && r.Status == LeaveStatus.Approved)
                .SumAsync(r => r.DaysTaken);

            var remaining = totalEntitlement - taken;

            return new LeaveBalanceSummaryDto
            {
                LeaveTypeName = leaveType.Name,
                TotalEntitlement = totalEntitlement,
                Taken = taken,
                Remaining = remaining
            };
        }
    }
}

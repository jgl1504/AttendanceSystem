using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;
using WebApp.Shared.Model.Constants;
using WebApp.Shared.Model.Payroll;

namespace WebApp.Server.Services
{
    public class LeaveDebugService
    {
        private readonly DataContext _context;

        public LeaveDebugService(DataContext context)
        {
            _context = context;
        }

        // ==== CORE SUMMARY (as at date) ======================================

        public async Task<LeaveBalanceSummaryDto?> GetLeaveSummaryAsync(
            int employeeId,
            Guid leaveTypeId,
            DateTime asAtDate)
        {
            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(lt => lt.Id == leaveTypeId && !lt.IsDeleted);

            if (leaveType == null)
                return null;

            // Block Annual before cut-off
            if (leaveType.Id == LeaveTypeIds.Annual &&
                asAtDate.Date < LeaveConstants.AccrualHireDateCutOff.Date)
            {
                return new LeaveBalanceSummaryDto
                {
                    LeaveTypeName = leaveType.Name,
                    TotalEntitlement = 0m,
                    Taken = 0m,
                    Remaining = 0m,
                    OpeningFromDate = LeaveConstants.AccrualHireDateCutOff.Date,
                    AccruedSinceStart = 0m
                };
            }

            if (leaveType.Id == LeaveTypeIds.Sick)
                return await GetSickLeaveSummaryAsync(employeeId, leaveType, asAtDate);

            if (leaveType.Id == LeaveTypeIds.Unpaid)
                return await GetUnpaidLeaveSummaryAsync(employeeId, leaveType, asAtDate);

            if (leaveType.Id == LeaveTypeIds.Annual)
                return await GetAnnualLeaveSummaryAsync(employeeId, leaveType, asAtDate);

            if (leaveType.Id == LeaveTypeIds.FamilyResponsibility)
                return await GetFamilyResponsibilitySummaryAsync(employeeId, leaveType, asAtDate);

            // Everything else, including maternity / paternity / parental
            return await GetStandardLeaveSummaryAsync(employeeId, leaveType, asAtDate);
        }

        // ==== EMPLOYEE VIEW: per month, all types ============================

        public async Task<List<EmployeeLeaveLineDto>> GetEmployeeLeaveLinesForMonthAsync(
            int employeeId,
            int year,
            int month)
        {
            if (year < 1900 || year > 2100)
                throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1900 and 2100.");

            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var leaveTypes = await _context.LeaveTypes
                .Where(lt => !lt.IsDeleted)
                .OrderBy(lt => lt.Name)
                .ToListAsync();

            if (!leaveTypes.Any())
                return new List<EmployeeLeaveLineDto>();

            var monthlyRecords = await _context.LeaveRecords
                .Where(r =>
                    r.EmployeeId == employeeId &&
                    r.Status == LeaveStatus.Approved &&
                    r.StartDate >= start &&
                    r.StartDate < start.AddMonths(1))
                .ToListAsync();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            var result = new List<EmployeeLeaveLineDto>();

            foreach (var lt in leaveTypes)
            {
                var summary = await GetLeaveSummaryAsync(employeeId, lt.Id, end.Date);
                if (summary == null)
                    continue;

                var takenInMonth = monthlyRecords
                    .Where(r => r.LeaveTypeId == lt.Id)
                    .Sum(r => r.DaysTaken);

                result.Add(new EmployeeLeaveLineDto
                {
                    LeaveTypeId = lt.Id,
                    LeaveTypeName = summary.LeaveTypeName,
                    TotalEntitlement = summary.TotalEntitlement,
                    TakenToDate = summary.Taken,
                    Remaining = summary.Remaining,
                    TakenInMonth = takenInMonth,
                    EmployeeStartDate = employee?.HireDate,
                    AnnualOpeningFromDate = summary.OpeningFromDate,
                    AccruedSinceStart = summary.AccruedSinceStart
                });
            }

            return result;
        }

        // ==== ADMIN VIEW: per employee + type for a month ====================

        public async Task<List<LeaveTypeBalanceRowDto>> GetLeaveTypeBalancesForMonthAsync(
            int year,
            int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var leaveTypes = await _context.LeaveTypes
                .Where(lt => !lt.IsDeleted)
                .OrderBy(lt => lt.Name)
                .ToListAsync();

            var employees = await _context.Employees
                .Include(e => e.Department)
                .OrderBy(e => e.Name)
                .ToListAsync();

            if (!leaveTypes.Any() || !employees.Any())
                return new List<LeaveTypeBalanceRowDto>();

            var recordsInMonth = await _context.LeaveRecords
                .Where(r =>
                    r.Status == LeaveStatus.Approved &&
                    r.StartDate >= start &&
                    r.StartDate < start.AddMonths(1))
                .ToListAsync();

            var result = new List<LeaveTypeBalanceRowDto>();

            foreach (var emp in employees)
            {
                foreach (var lt in leaveTypes)
                {
                    var takenInMonth = recordsInMonth
                        .Where(r => r.EmployeeId == emp.Id && r.LeaveTypeId == lt.Id)
                        .Sum(r => r.DaysTaken);

                    var summaryAsAtEnd = await GetLeaveSummaryAsync(emp.Id, lt.Id, end.Date);
                    if (summaryAsAtEnd == null)
                        continue;

                    var prevEnd = start.AddDays(-1);
                    var summaryAtStart = await GetLeaveSummaryAsync(emp.Id, lt.Id, prevEnd);
                    var opening = summaryAtStart?.Remaining ?? 0m;

                    var currentBalance = summaryAsAtEnd.Remaining;

                    var accruedInMonth = (currentBalance + takenInMonth) - opening;

                    result.Add(new LeaveTypeBalanceRowDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.Name,
                        DepartmentName = emp.Department?.Name ?? string.Empty,
                        LeaveTypeId = lt.Id,
                        LeaveTypeName = lt.Name,
                        OpeningBalance = opening,
                        AccruedInMonth = accruedInMonth,
                        TakenInMonth = takenInMonth,
                        CurrentBalance = currentBalance
                    });
                }
            }

            return result;
        }

        // ==== INTERNAL HELPERS ===============================================

        // SICK: 30 days per 3‑year cycle
        // - If hired before AccrualHireDateCutOff: cycles from that cutoff
        // - If hired after: must complete 4 months service, then cycles from hire date
        private async Task<LeaveBalanceSummaryDto> GetSickLeaveSummaryAsync(
            int employeeId,
            LeaveType leaveType,
            DateTime asAtDate)
        {
            const decimal sickEntitlementPerCycle = 30m; // 30 days every 3 years[web:51][web:58]
            var cycleLengthYears = 3;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                return new LeaveBalanceSummaryDto
                {
                    LeaveTypeName = leaveType.Name,
                    TotalEntitlement = 0m,
                    Taken = 0m,
                    Remaining = 0m
                };
            }

            // Determine base cycle start
            DateTime cycleStart;

            if (employee.HireDate.Date < LeaveConstants.AccrualHireDateCutOff.Date)
            {
                // Old employees: start sick cycles from global cut‑off
                cycleStart = LeaveConstants.AccrualHireDateCutOff.Date;
            }
            else
            {
                // New employees: must complete 4 months service
                var qualifiesFrom = employee.HireDate.AddMonths(4);

                if (asAtDate.Date < qualifiesFrom.Date)
                {
                    return new LeaveBalanceSummaryDto
                    {
                        LeaveTypeName = leaveType.Name,
                        TotalEntitlement = 0m,
                        Taken = 0m,
                        Remaining = 0m
                    };
                }

                cycleStart = employee.HireDate.Date;
            }

            if (asAtDate.Date < cycleStart.Date)
            {
                return new LeaveBalanceSummaryDto
                {
                    LeaveTypeName = leaveType.Name,
                    TotalEntitlement = 0m,
                    Taken = 0m,
                    Remaining = 0m
                };
            }

            // Work out which 3‑year block we’re in from cycleStart
            var yearsSinceStart = asAtDate.Year - cycleStart.Year;

            if (asAtDate.Month < cycleStart.Month ||
                (asAtDate.Month == cycleStart.Month && asAtDate.Day < cycleStart.Day))
            {
                yearsSinceStart--;
            }

            if (yearsSinceStart < 0)
                yearsSinceStart = 0;

            var cycleIndex = yearsSinceStart / cycleLengthYears;

            var currentCycleStart = cycleStart.AddYears(cycleIndex * cycleLengthYears);
            var currentCycleEnd = currentCycleStart.AddYears(cycleLengthYears);

            var totalEntitlement = sickEntitlementPerCycle;

            var taken = await _context.LeaveRecords
                .Where(r => r.EmployeeId == employeeId
                            && r.LeaveTypeId == leaveType.Id
                            && r.Status == LeaveStatus.Approved
                            && r.StartDate.Date >= currentCycleStart.Date
                            && r.StartDate.Date <= asAtDate.Date
                            && r.StartDate.Date < currentCycleEnd.Date)
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

        private async Task<LeaveBalanceSummaryDto> GetUnpaidLeaveSummaryAsync(
            int employeeId,
            LeaveType leaveType,
            DateTime asAtDate)
        {
            var taken = await _context.LeaveRecords
                .Where(r => r.EmployeeId == employeeId
                            && r.LeaveTypeId == leaveType.Id
                            && r.Status == LeaveStatus.Approved
                            && r.StartDate.Date <= asAtDate.Date)
                .SumAsync(r => r.DaysTaken);

            return new LeaveBalanceSummaryDto
            {
                LeaveTypeName = leaveType.Name,
                TotalEntitlement = 0m,
                Taken = taken,
                Remaining = 0m
            };
        }

        // === Helper for generic taken calculation ===========================

        private Task<decimal> GetTakenLeaveAsync(int employeeId, Guid leaveTypeId, DateTime asAtDate)
        {
            return _context.LeaveRecords
                .Where(r => r.EmployeeId == employeeId
                            && r.LeaveTypeId == leaveTypeId
                            && r.Status == LeaveStatus.Approved
                            && r.StartDate.Date <= asAtDate.Date)
                .SumAsync(r => r.DaysTaken);
        }

        // === Parental / maternity / paternity helpers =======================

        private static bool IsParentalLeaveType(LeaveType leaveType)
        {
            var name = leaveType.Name ?? string.Empty;
            return name.Contains("Maternity", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("Paternity", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("Parental", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMaternity(LeaveType leaveType)
        {
            var name = leaveType.Name ?? string.Empty;
            return name.Contains("Maternity", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPaternity(LeaveType leaveType)
        {
            var name = leaveType.Name ?? string.Empty;
            return name.Contains("Paternity", StringComparison.OrdinalIgnoreCase);
        }

        // Once-off entitlement per event
        // Maternity: 120 days (4 months) per event, Paternity: 10 days per event.[web:57][web:61]
        private static decimal GetParentalEntitlementPerEvent(LeaveType leaveType)
        {
            if (IsMaternity(leaveType))
                return 120m;

            if (IsPaternity(leaveType))
                return 10m;

            return 10m;
        }

        private async Task<LeaveBalanceSummaryDto> GetStandardLeaveSummaryAsync(
            int employeeId,
            LeaveType leaveType,
            DateTime asAtDate)
        {
            decimal totalEntitlement;
            decimal taken;
            decimal remaining;

            var balanceRow = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveType.Id);

            // === SPECIAL: parental / maternity / paternity – 10/120 consecutive days, 12‑month block (stateless) ===
            if (IsParentalLeaveType(leaveType))
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                if (leaveType.IsGenderSpecific && leaveType.RequiredGender.HasValue)
                {
                    if (employee == null || employee.Gender != leaveType.RequiredGender.Value)
                    {
                        return new LeaveBalanceSummaryDto
                        {
                            LeaveTypeName = leaveType.Name,
                            TotalEntitlement = 0m,
                            Taken = 0m,
                            Remaining = 0m,
                            OpeningFromDate = LeaveConstants.AccrualHireDateCutOff.Date,
                            AccruedSinceStart = 0m
                        };
                    }
                }

                if (asAtDate.Date < LeaveConstants.AccrualHireDateCutOff.Date)
                {
                    return new LeaveBalanceSummaryDto
                    {
                        LeaveTypeName = leaveType.Name,
                        TotalEntitlement = 0m,
                        Taken = 0m,
                        Remaining = 0m,
                        OpeningFromDate = LeaveConstants.AccrualHireDateCutOff.Date,
                        AccruedSinceStart = 0m
                    };
                }

                var defaultEntitlement = GetParentalEntitlementPerEvent(leaveType);
                decimal totalEntitlementParental;

                if (balanceRow != null &&
                    balanceRow.OpeningBalance > 0 &&
                    balanceRow.OpeningBalance > defaultEntitlement)
                {
                    totalEntitlementParental = balanceRow.OpeningBalance;
                }
                else
                {
                    totalEntitlementParental = defaultEntitlement;
                }

                var firstRecord = await _context.LeaveRecords
                    .Where(r => r.EmployeeId == employeeId
                                && r.LeaveTypeId == leaveType.Id
                                && r.Status == LeaveStatus.Approved
                                && r.StartDate.Date >= LeaveConstants.AccrualHireDateCutOff.Date
                                && r.StartDate.Date <= asAtDate.Date)
                    .OrderBy(r => r.StartDate)
                    .FirstOrDefaultAsync();

                if (firstRecord == null)
                {
                    return new LeaveBalanceSummaryDto
                    {
                        LeaveTypeName = leaveType.Name,
                        TotalEntitlement = totalEntitlementParental,
                        Taken = 0m,
                        Remaining = totalEntitlementParental,
                        OpeningFromDate = LeaveConstants.AccrualHireDateCutOff.Date,
                        AccruedSinceStart = 0m
                    };
                }

                var blockStart = firstRecord.StartDate.Date;
                var blockEnd = blockStart.AddYears(1);

                if (asAtDate.Date < blockEnd)
                {
                    var takenInBlock = await _context.LeaveRecords
                        .Where(r => r.EmployeeId == employeeId
                                    && r.LeaveTypeId == leaveType.Id
                                    && r.Status == LeaveStatus.Approved
                                    && r.StartDate.Date >= blockStart
                                    && r.StartDate.Date <= asAtDate.Date)
                        .SumAsync(r => r.DaysTaken);

                    return new LeaveBalanceSummaryDto
                    {
                        LeaveTypeName = leaveType.Name,
                        TotalEntitlement = totalEntitlementParental,
                        Taken = takenInBlock,
                        Remaining = 0m,
                        OpeningFromDate = blockStart,
                        AccruedSinceStart = 0m
                    };
                }

                return new LeaveBalanceSummaryDto
                {
                    LeaveTypeName = leaveType.Name,
                    TotalEntitlement = totalEntitlementParental,
                    Taken = 0m,
                    Remaining = totalEntitlementParental,
                    OpeningFromDate = blockEnd,
                    AccruedSinceStart = 0m
                };
            }

            // === DEFAULT STANDARD HANDLING =========================================

            if (balanceRow != null)
            {
                totalEntitlement = balanceRow.OpeningBalance;

                taken = await GetTakenLeaveAsync(employeeId, leaveType.Id, asAtDate);
                remaining = totalEntitlement - taken;
            }
            else
            {
                if ((leaveType.AccrualType == LeaveAccrualType.Annual ||
                     leaveType.AccrualType == LeaveAccrualType.Fixed) &&
                    leaveType.DaysPerYear > 0)
                {
                    totalEntitlement = leaveType.DaysPerYear;
                }
                else if ((leaveType.AccrualType == LeaveAccrualType.Cycle ||
                          leaveType.AccrualType == LeaveAccrualType.Fixed) &&
                         leaveType.DaysPerCycle.HasValue)
                {
                    totalEntitlement = leaveType.DaysPerCycle.Value;
                }
                else
                {
                    totalEntitlement = 9999m;
                }

                taken = await GetTakenLeaveAsync(employeeId, leaveType.Id, asAtDate);
                remaining = totalEntitlement - taken;
            }

            return new LeaveBalanceSummaryDto
            {
                LeaveTypeName = leaveType.Name,
                TotalEntitlement = totalEntitlement,
                Taken = taken,
                Remaining = remaining
            };
        }

        private async Task<LeaveBalanceSummaryDto> GetFamilyResponsibilitySummaryAsync(
            int employeeId,
            LeaveType leaveType,
            DateTime asAtDate)
        {
            const decimal entitlementPerCycle = 3m;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                return new LeaveBalanceSummaryDto
                {
                    LeaveTypeName = leaveType.Name,
                    TotalEntitlement = 0m,
                    Taken = 0m,
                    Remaining = 0m,
                    OpeningFromDate = LeaveConstants.AccrualHireDateCutOff.Date,
                    AccruedSinceStart = 0m
                };
            }

            var qualifiesFrom = employee.HireDate.AddMonths(4);

            if (asAtDate.Date < qualifiesFrom.Date)
            {
                return new LeaveBalanceSummaryDto
                {
                    LeaveTypeName = leaveType.Name,
                    TotalEntitlement = 0m,
                    Taken = 0m,
                    Remaining = 0m,
                    OpeningFromDate = LeaveConstants.AccrualHireDateCutOff.Date,
                    AccruedSinceStart = 0m
                };
            }

            var cycleStart = LeaveConstants.AccrualHireDateCutOff.Date;
            while (cycleStart.AddYears(1) <= asAtDate.Date)
            {
                cycleStart = cycleStart.AddYears(1);
            }
            var cycleEnd = cycleStart.AddYears(1);

            var takenInCycle = await _context.LeaveRecords
                .Where(r => r.EmployeeId == employeeId
                            && r.LeaveTypeId == leaveType.Id
                            && r.Status == LeaveStatus.Approved
                            && r.StartDate.Date >= cycleStart
                            && r.StartDate.Date <= asAtDate.Date
                            && r.StartDate.Date < cycleEnd)
                .SumAsync(r => r.DaysTaken);

            var totalEntitlementFR = entitlementPerCycle;
            var remainingFR = entitlementPerCycle - takenInCycle;
            if (remainingFR < 0) remainingFR = 0;

            return new LeaveBalanceSummaryDto
            {
                LeaveTypeName = leaveType.Name,
                TotalEntitlement = totalEntitlementFR,
                Taken = takenInCycle,
                Remaining = remainingFR,
                OpeningFromDate = cycleStart,
                AccruedSinceStart = 0m
            };
        }

        private async Task<LeaveBalanceSummaryDto> GetAnnualLeaveSummaryAsync(
            int employeeId,
            LeaveType leaveType,
            DateTime asAtDate)
        {
            var balanceRow = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveType.Id);

            var empLeave = await _context.EmployeeLeaves
                .FirstOrDefaultAsync(el => el.EmployeeId == employeeId);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            decimal accrualRatePerMonth = empLeave?.AccrualRatePerMonth ?? 0m;

            decimal openingBalance = 0m;
            DateTime? accrualStart = null;

            if (employee != null &&
                employee.HireDate >= LeaveConstants.AccrualHireDateCutOff)
            {
                accrualStart = new DateTime(employee.HireDate.Year, employee.HireDate.Month, 1);
                openingBalance = 0m;
            }
            else
            {
                if (balanceRow != null)
                {
                    openingBalance = balanceRow.OpeningBalance;
                }
                else
                {
                    openingBalance = 0m;
                }

                accrualStart = new DateTime(
                    LeaveConstants.AccrualHireDateCutOff.Year,
                    LeaveConstants.AccrualHireDateCutOff.Month,
                    1);
            }

            decimal accruedSinceStart = 0m;

            if (accrualStart.HasValue && accrualRatePerMonth > 0m)
            {
                var startMonth = new DateTime(accrualStart.Value.Year, accrualStart.Value.Month, 1);
                var endMonth = new DateTime(asAtDate.Year, asAtDate.Month, 1);

                int months = ((endMonth.Year - startMonth.Year) * 12) +
                             (endMonth.Month - startMonth.Month);

                if (asAtDate.Day == DateTime.DaysInMonth(asAtDate.Year, asAtDate.Month))
                {
                    months += 1;
                }

                if (months < 0)
                    months = 0;

                accruedSinceStart = months * accrualRatePerMonth;
            }

            var totalEntitlementAnnual = openingBalance + accruedSinceStart;

            var takenAnnual = await GetTakenLeaveAsync(employeeId, leaveType.Id, asAtDate);

            var remainingAnnual = totalEntitlementAnnual - takenAnnual;

            return new LeaveBalanceSummaryDto
            {
                LeaveTypeName = leaveType.Name,
                TotalEntitlement = totalEntitlementAnnual,
                Taken = takenAnnual,
                Remaining = remainingAnnual,
                OpeningFromDate = accrualStart,
                AccruedSinceStart = accruedSinceStart
            };
        }
    }
}

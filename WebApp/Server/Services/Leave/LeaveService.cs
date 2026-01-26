using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Leave
{
    public class LeaveService
    {
        private readonly DataContext _context;

        public LeaveService(DataContext context)
        {
            _context = context;
        }

        // Get employee's current leave balance
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

        // Auto-accrue monthly leave
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

        // Request leave WITHOUT attachment
        public async Task<bool> RequestLeaveAsync(RequestLeaveDto request)
        {
            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
                return false;

            var leave = await _context.EmployeeLeaves
                .FirstOrDefaultAsync(l => l.EmployeeId == request.EmployeeId);

            // If no leave row yet, create one with default values so requests can still be stored
            if (leave == null)
            {
                leave = new EmployeeLeave
                {
                    EmployeeId = request.EmployeeId,
                    DaysBalance = 0,
                    // Use same defaults as InitializeEmployeeLeaveAsync
                    DaysPerWeek = 5,
                    AccrualRatePerMonth = 1.25m,
                    LastAccrualDate = DateTime.UtcNow,
                    LastAccrualMonth = DateTime.UtcNow.Month,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.EmployeeLeaves.Add(leave);
                await _context.SaveChangesAsync();
            }

            var workingDays = CalculateWorkingDays(request.StartDate, request.EndDate);

            decimal daysTaken;
            if (request.Portion == LeavePortion.HalfDay)
            {
                daysTaken = workingDays <= 0 ? 0.5m : workingDays * 0.5m;
            }
            else
            {
                daysTaken = workingDays <= 0 ? 1.0m : workingDays;
            }

            var leaveRecord = new LeaveRecord
            {
                EmployeeId = request.EmployeeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DaysTaken = daysTaken,
                LeaveType = request.LeaveType,
                Status = LeaveStatus.Pending,
                Reason = request.Reason,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Portion = request.Portion,
                AttachmentFileName = null
            };

            _context.LeaveRecords.Add(leaveRecord);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> GetAttachmentFileNameAsync(int leaveRecordId)
        {
            var record = await _context.LeaveRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == leaveRecordId);

            return record?.AttachmentFileName;
        }

        // Approve leave and deduct from balance
        public async Task<bool> ApproveLeaveAsync(int leaveRecordId)
        {
            var record = await _context.LeaveRecords
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == leaveRecordId);

            if (record == null || record.Status != LeaveStatus.Pending)
                return false;

            if (record.LeaveType == LeaveType.Annual)
            {
                var leave = await _context.EmployeeLeaves
                    .FirstOrDefaultAsync(l => l.EmployeeId == record.EmployeeId);

                if (leave == null)
                    return false;

                // This can take the balance to 0 or below
                leave.DaysBalance -= record.DaysTaken;
                _context.EmployeeLeaves.Update(leave);
            }

            record.Status = LeaveStatus.Approved;
            record.ApprovedAt = DateTime.UtcNow;

            _context.LeaveRecords.Update(record);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all leave records for one employee
        public async Task<List<LeaveRecordDto>> GetLeaveRecordsForEmployeeAsync(int employeeId)
        {
            return await _context.LeaveRecords
                .Include(r => r.Employee)
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.StartDate)
                .Select(r => new LeaveRecordDto
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee != null ? r.Employee.Name : string.Empty,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    DaysTaken = r.DaysTaken,
                    LeaveType = r.LeaveType,
                    Status = r.Status,
                    Reason = r.Reason,
                    RequestedAt = r.RequestedAt,
                    ApprovedAt = r.ApprovedAt,
                    Portion = r.Portion,
                    AttachmentFileName = r.AttachmentFileName
                })
                .ToListAsync();
        }

        // Get all pending leave requests
        public async Task<List<LeaveRecordDto>> GetPendingLeaveRequestsAsync()
        {
            return await _context.LeaveRecords
                .Include(r => r.Employee)
                .Where(r => r.Status == LeaveStatus.Pending)
                .OrderBy(r => r.StartDate)
                .Select(r => new LeaveRecordDto
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee != null ? r.Employee.Name : string.Empty,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    DaysTaken = r.DaysTaken,
                    LeaveType = r.LeaveType,
                    Status = r.Status,
                    Reason = r.Reason,
                    RequestedAt = r.RequestedAt,
                    ApprovedAt = r.ApprovedAt,
                    Portion = r.Portion,
                    AttachmentFileName = r.AttachmentFileName
                })
                .ToListAsync();
        }

        // Reject leave
        public async Task<bool> RejectLeaveAsync(int leaveRecordId)
        {
            var record = await _context.LeaveRecords.FindAsync(leaveRecordId);
            if (record == null || record.Status != LeaveStatus.Pending)
                return false;

            record.Status = LeaveStatus.Rejected;
            record.ApprovedAt = DateTime.UtcNow;

            _context.LeaveRecords.Update(record);
            await _context.SaveChangesAsync();
            return true;
        }

        // Calculate working days (excludes weekends)
        private decimal CalculateWorkingDays(DateTime start, DateTime end)
        {
            decimal days = 0;
            var current = start.Date;

            while (current <= end.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday &&
                    current.DayOfWeek != DayOfWeek.Sunday)
                {
                    days += 1;
                }
                current = current.AddDays(1);
            }

            return days;
        }

        // Initialize leave for new employee
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
    }
}

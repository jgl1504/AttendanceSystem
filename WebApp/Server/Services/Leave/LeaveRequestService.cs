using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Leave
{
    public class LeaveRequestService
    {
        private readonly DataContext _context;

        public LeaveRequestService(DataContext context)
        {
            _context = context;
        }

        // Request leave WITHOUT attachment (same logic as before)
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
                LeaveTypeId = request.LeaveTypeId,
                Status = LeaveStatus.Pending,
                Reason = request.Reason,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Portion = request.Portion,
                AttachmentFileName = null,
                AttachmentPath = null
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

        // Approve leave and deduct from balance (still using EmployeeLeaves for now)
        public async Task<bool> ApproveLeaveAsync(int leaveRecordId)
        {
            var record = await _context.LeaveRecords
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == leaveRecordId);

            if (record == null || record.Status != LeaveStatus.Pending)
                return false;

            // TODO: later use LeaveBalance pools instead of EmployeeLeaves
            var leaveType = record.LeaveType;
            if (leaveType.PoolType == LeavePoolType.OwnPool && leaveType.Name == "Annual Leave")
            {
                var leave = await _context.EmployeeLeaves
                    .FirstOrDefaultAsync(l => l.EmployeeId == record.EmployeeId);

                if (leave == null)
                    return false;

                leave.DaysBalance -= record.DaysTaken;
                _context.EmployeeLeaves.Update(leave);
            }

            record.Status = LeaveStatus.Approved;
            record.ApprovedAt = DateTime.UtcNow;

            _context.LeaveRecords.Update(record);
            await _context.SaveChangesAsync();
            return true;
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

        // Get all leave records for one employee
        public async Task<List<LeaveRecordDto>> GetLeaveRecordsForEmployeeAsync(int employeeId)
        {
            return await _context.LeaveRecords
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
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
                    LeaveTypeId = r.LeaveTypeId,
                    LeaveTypeName = r.LeaveType.Name,
                    RequiresDocument = r.LeaveType.RequiresSupportingDocument,
                    Status = r.Status,
                    Reason = r.Reason,
                    RequestedAt = r.RequestedAt,
                    ApprovedAt = r.ApprovedAt,
                    Portion = r.Portion,
                    AttachmentFileName = r.AttachmentFileName,
                    AttachmentPath = r.AttachmentPath
                })
                .ToListAsync();
        }

        // Get all pending leave requests
        public async Task<List<LeaveRecordDto>> GetPendingLeaveRequestsAsync()
        {
            return await _context.LeaveRecords
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
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
                    LeaveTypeId = r.LeaveTypeId,
                    LeaveTypeName = r.LeaveType.Name,
                    RequiresDocument = r.LeaveType.RequiresSupportingDocument,
                    Status = r.Status,
                    Reason = r.Reason,
                    RequestedAt = r.RequestedAt,
                    ApprovedAt = r.ApprovedAt,
                    Portion = r.Portion,
                    AttachmentFileName = r.AttachmentFileName,
                    AttachmentPath = r.AttachmentPath
                })
                .ToListAsync();
        }

        // Calculate working days (excludes weekends) - same as before
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
    }
}

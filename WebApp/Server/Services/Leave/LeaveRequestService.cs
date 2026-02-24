using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApp.Server.Configuration;
using WebApp.Server.Data;
using WebApp.Shared.Model;
using WebApp.Shared.Model.Payroll;

namespace WebApp.Server.Services.Leave
{
    public class LeaveRequestService
    {
        private readonly DataContext _context;
        private readonly LeaveAttachmentsOptions _attachmentOptions;

        public LeaveRequestService(DataContext context, IOptions<LeaveAttachmentsOptions> attachmentOptions)
        {
            _context = context;
            _attachmentOptions = attachmentOptions.Value;
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

        // NEW: Save attachment file to disk and update LeaveRecord
        public async Task<bool> SaveAttachmentAsync(int leaveRecordId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var record = await _context.LeaveRecords
                .FirstOrDefaultAsync(r => r.Id == leaveRecordId);

            if (record == null)
                return false;

            var root = _attachmentOptions.RootPath;
            if (string.IsNullOrWhiteSpace(root))
                return false;

            Directory.CreateDirectory(root);

            var safeFileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(root, safeFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            record.AttachmentFileName = safeFileName;
            record.AttachmentPath = null; // we only store file name now, path comes from config
            //record.UpdatedAt = DateTime.UtcNow;

            _context.LeaveRecords.Update(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeLeaveTypeAsync(int leaveRecordId, Guid newLeaveTypeId)
        {
            var record = await _context.LeaveRecords
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == leaveRecordId);

            if (record == null || record.Status != LeaveStatus.Pending)
                return false;

            var newType = await _context.LeaveTypes
                .FirstOrDefaultAsync(t => t.Id == newLeaveTypeId && !t.IsDeleted && t.IsActive);

            if (newType == null)
                return false;

            record.LeaveTypeId = newLeaveTypeId;
            // Optionally update cached name if you keep it on the entity
            // record.LeaveTypeName = newType.Name;

            _context.LeaveRecords.Update(record);
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

        public async Task<bool> ChangePortionAsync(int leaveRecordId, LeavePortion newPortion)
        {
            var record = await _context.LeaveRecords
                .FirstOrDefaultAsync(r => r.Id == leaveRecordId);

            if (record == null || record.Status != LeaveStatus.Pending)
                return false;

            record.Portion = newPortion;

            var workingDays = CalculateWorkingDays(record.StartDate, record.EndDate);

            decimal daysTaken;
            if (newPortion == LeavePortion.HalfDay)
            {
                daysTaken = workingDays <= 0 ? 0.5m : workingDays * 0.5m;
            }
            else // LeavePortion.FullDay
            {
                daysTaken = workingDays <= 0 ? 1.0m : workingDays;
            }

            record.DaysTaken = daysTaken;

            _context.LeaveRecords.Update(record);
            await _context.SaveChangesAsync();
            return true;
        }



        // NEW: Get leave records for payroll report (approved only, for a date range)
        public async Task<List<PayrollLeaveDetailDto>> GetLeaveRecordsForPeriodAsync(
            DateTime from,
            DateTime to,
            int? employeeId = null,
            int? departmentId = null)
        {
            var query = _context.LeaveRecords
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Department)
                .Include(r => r.LeaveType)
                .Where(r => r.Status == LeaveStatus.Approved
                            && r.StartDate >= from
                            && r.StartDate <= to)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (departmentId.HasValue)
                query = query.Where(r => r.Employee.DepartmentId == departmentId.Value);

            var records = await query
                .OrderBy(r => r.StartDate)
                .ThenBy(r => r.Employee.Name)
                .ToListAsync();

            return records.Select(r => new PayrollLeaveDetailDto
            {
                EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee.Name,
                DepartmentName = r.Employee.Department?.Name ?? "",
                Type = r.LeaveType.Name,
                FromDate = r.StartDate,
                ToDate = r.EndDate,
                Hours = r.DaysTaken * 8m, // Convert days to hours
                Reason = r.Reason ?? "" // Use Reason instead of Notes
            }).ToList();
        }
    }
}

using static WebApp.Server.SqlStoredProc.StoreProcModel;

namespace WebApp.Server.SqlStoredProc
{
    public class StoreProcDirectory
    {
        public static List<StoreProcModel> StoreProcedureDirectories()
        {
            return new List<StoreProcModel>()
            {
                   new StoreProcModel
                {
                    Name = "VwMaintenanceGrouped.sql",
                    StoredProcType = StoreProcTypeEnum.SubTask.ToString(),
                    Description = "View to group top for each MaintenanceType and Horse",
                    StoredProcPath = @"SqlStoredProc\Views\VwMaintenanceGrouped.sql"
                },
                       new StoreProcModel
                {
                    Name = "VwVaccinationGrouped.sql",
                    StoredProcType = StoreProcTypeEnum.SubTask.ToString(),
                    Description = "View to group top for each VaccinationType and Horse",
                    StoredProcPath = @"SqlStoredProc\Views\VwVaccinationGrouped.sql"
                }
            };
        }
    }

    public class StoreProcModel
    {
        public string? Name { get; set; }
        public string? StoredProcType { get; set; }
        public string? Description { get; set; }
        public string? StoredProcPath { get; set; }

        public enum StoreProcTypeEnum
        {
            MainTask = 1,
            SubTask = 2,
        }

    }
}

USE [HorseTracker]
GO

/****** Object:  View [dbo].[VwMaintenanceGrouped]    Script Date: 2023/09/05 20:49:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[VwMaintenanceGrouped]
AS
WITH LatestMaintenance AS (SELECT        HorseDetailId, MaintenanceTypeId, MAX(DateTimeCreated) AS LatestDateTimeCreated
                                                                FROM            dbo.Maintenance AS M
                                                                GROUP BY HorseDetailId, MaintenanceTypeId)
    SELECT        M.Id AS MaintenanceId, M.NextDate, M.Comments, M.HorseDetailId, HD.Name AS HorseName, M.MaintenanceTypeId, MT.Type AS MaintType, DATEDIFF(DAY, GETDATE(), M.NextDate) AS DaysLeft
     FROM            dbo.Maintenance AS M INNER JOIN
                              LatestMaintenance AS LM ON M.HorseDetailId = LM.HorseDetailId AND M.MaintenanceTypeId = LM.MaintenanceTypeId AND M.DateTimeCreated = LM.LatestDateTimeCreated LEFT OUTER JOIN
                              dbo.HorseDetail AS HD ON M.HorseDetailId = HD.Id LEFT OUTER JOIN
                              dbo.MaintenanceType AS MT ON M.MaintenanceTypeId = MT.Id


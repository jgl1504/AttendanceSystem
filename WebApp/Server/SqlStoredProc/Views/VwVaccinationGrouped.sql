USE [HorseTracker]
GO

/****** Object:  View [dbo].[VwVaccinationGrouped]    Script Date: 2023/09/05 23:09:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[VwVaccinationGrouped]
AS
WITH LatestVaccination AS (SELECT        HorseDetailId, VaccinationTypeId, MAX(DateTimeCreated) AS LatestDateTimeCreated
                                                                FROM            dbo.Vaccination AS M
                                                                GROUP BY HorseDetailId, VaccinationTypeId)
    SELECT        M.Id AS VaccinationId, M.NextVaccineDate, M.VaccineNotes, M.HorseDetailId, HD.Name AS HorseName, M.VaccinationTypeId, MT.Type AS VacType, DATEDIFF(DAY, GETDATE(), M.NextVaccineDate) AS DaysLeft
     FROM            dbo.Vaccination AS M INNER JOIN
                              LatestVaccination AS LM ON M.HorseDetailId = LM.HorseDetailId AND M.VaccinationTypeId = LM.VaccinationTypeId AND M.DateTimeCreated = LM.LatestDateTimeCreated LEFT OUTER JOIN
                              dbo.HorseDetail AS HD ON M.HorseDetailId = HD.Id LEFT OUTER JOIN
                              dbo.VaccinationType AS MT ON M.VaccinationTypeId = MT.Id

GO



namespace WebApp.Shared.Model
{
    public class LeaveMonthlyReportRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        // JANUARY
        public decimal JanAnnual { get; set; }
        public decimal JanSick { get; set; }
        public decimal JanSpecial { get; set; }
        public decimal JanUnpaid { get; set; }

        // FEBRUARY
        public decimal FebAnnual { get; set; }
        public decimal FebSick { get; set; }
        public decimal FebSpecial { get; set; }
        public decimal FebUnpaid { get; set; }

        // MARCH
        public decimal MarAnnual { get; set; }
        public decimal MarSick { get; set; }
        public decimal MarSpecial { get; set; }
        public decimal MarUnpaid { get; set; }

        // APRIL
        public decimal AprAnnual { get; set; }
        public decimal AprSick { get; set; }
        public decimal AprSpecial { get; set; }
        public decimal AprUnpaid { get; set; }

        // MAY
        public decimal MayAnnual { get; set; }
        public decimal MaySick { get; set; }
        public decimal MaySpecial { get; set; }
        public decimal MayUnpaid { get; set; }

        // JUNE
        public decimal JunAnnual { get; set; }
        public decimal JunSick { get; set; }
        public decimal JunSpecial { get; set; }
        public decimal JunUnpaid { get; set; }

        // JULY
        public decimal JulAnnual { get; set; }
        public decimal JulSick { get; set; }
        public decimal JulSpecial { get; set; }
        public decimal JulUnpaid { get; set; }

        // AUGUST
        public decimal AugAnnual { get; set; }
        public decimal AugSick { get; set; }
        public decimal AugSpecial { get; set; }
        public decimal AugUnpaid { get; set; }

        // SEPTEMBER
        public decimal SepAnnual { get; set; }
        public decimal SepSick { get; set; }
        public decimal SepSpecial { get; set; }
        public decimal SepUnpaid { get; set; }

        // OCTOBER
        public decimal OctAnnual { get; set; }
        public decimal OctSick { get; set; }
        public decimal OctSpecial { get; set; }
        public decimal OctUnpaid { get; set; }

        // NOVEMBER
        public decimal NovAnnual { get; set; }
        public decimal NovSick { get; set; }
        public decimal NovSpecial { get; set; }
        public decimal NovUnpaid { get; set; }

        // DECEMBER
        public decimal DecAnnual { get; set; }
        public decimal DecSick { get; set; }
        public decimal DecSpecial { get; set; }
        public decimal DecUnpaid { get; set; }

        // BALANCE AS OF YEAR END
        public decimal BalanceAnnual { get; set; }
        public decimal BalanceSick { get; set; }
        public decimal BalanceSpecial { get; set; }
        public decimal BalanceUnpaid { get; set; }
    }
}

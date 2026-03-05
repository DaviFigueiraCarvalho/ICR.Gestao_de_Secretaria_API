namespace ICR.Domain.DTOs
{
    public class DashboardStatsDTO
    {
        public int TotalFederations { get; set; }
        public int TotalChurches { get; set; }
        public int TotalMissionaryCommunities { get; set; } // CellType.ComunidadeMissionaria
        public int TotalFamilies { get; set; }
        public int TotalCells { get; set; } // CellType.Celula
        public int TotalMembers { get; set; }
        public string ResultMessage { get; set; } = "Sucesso";
    }
}
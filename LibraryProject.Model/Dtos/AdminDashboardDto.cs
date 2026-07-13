namespace LibraryProject.Model.Dtos
{
    public class AdminDashboardDto
    {
        public int CurrentlyInside { get; set; }     // şu an içeride (Active)
        public int OnBreak { get; set; }             // molada
        public int TodaysReservations { get; set; }  // bugünkü toplam rezervasyon
        public int ActiveLoans { get; set; }         // dışarıdaki kitap
        public int OverdueLoans { get; set; }        // gecikmiş kitap
        public int TotalVisitors { get; set; }       // kayıtlı ziyaretçi
        public int PendingGiftRequests { get; set; } // bekleyen hediye talebi
    }
}
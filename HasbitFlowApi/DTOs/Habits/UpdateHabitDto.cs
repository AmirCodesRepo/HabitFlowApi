namespace HasbitFlowApi.DTOs.Habits
{
    public class UpdateHabitDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsActive {  get; set; }
    }
}

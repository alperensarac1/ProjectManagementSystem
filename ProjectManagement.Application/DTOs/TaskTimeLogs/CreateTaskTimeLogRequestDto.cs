namespace ProjectManagement.Application.DTOs.TaskTimeLogs;


public sealed class CreateTaskTimeLogRequestDto
{
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public DateTime WorkDate { get; set; }
}
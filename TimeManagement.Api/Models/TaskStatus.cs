namespace TimeManagement.Api.Models;

public enum TaskStatus
{
    Pending = 0,
    Assigned = 1,
    Accepted = 2,
    InProgress = 3,
    PendingApproval = 4,
    Completed = 5,
    Approved = 6,
    Rejected = 7
}

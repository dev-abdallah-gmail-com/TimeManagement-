using System;
using System.Collections.Generic;

namespace TimeManagement.Client.Services
{
 // Models used by the ApiService to match backend DTOs
 public class RegisterModel
 {
 public string FirstName { get; set; } = string.Empty;
 public string LastName { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 }

 public class LoginModel
 {
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 }

 public class AuthResponse
 {
 public string Token { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string UserId { get; set; } = string.Empty;
 public string FirstName { get; set; } = string.Empty;
 public string LastName { get; set; } = string.Empty;
 public List<string> Roles { get; set; } = new List<string>();
 }

 // Task models
 public class CreateTaskModel
 {
 public string Title { get; set; } = string.Empty;
 public string? Description { get; set; }
 public DateTime? ScheduledStartDate { get; set; }
 public DateTime? ScheduledEndDate { get; set; }
 public List<int> TagIds { get; set; } = new List<int>();
 public string? AssigneeEmail { get; set; }
 }

 public class UpdateTaskModel
 {
 public string Title { get; set; } = string.Empty;
 public string? Description { get; set; }
 public DateTime? ScheduledStartDate { get; set; }
 public DateTime? ScheduledEndDate { get; set; }
 public List<int> TagIds { get; set; } = new List<int>();
 }

 public class TaskResponseDto
 {
 public int Id { get; set; }
 public string Title { get; set; } = string.Empty;
 public string? Description { get; set; }
 public DateTime? ScheduledStartDate { get; set; }
 public DateTime? ScheduledEndDate { get; set; }
 public TaskStatus Status { get; set; } = TaskStatus.Pending;
 public string CreatedBy { get; set; } = string.Empty;
 public string CreatorEmail { get; set; } = string.Empty;
 public string? AssignedTo { get; set; }
 public string? AssigneeEmail { get; set; }
 public DateTime CreatedAt { get; set; }
 public DateTime? UpdatedAt { get; set; }
 public DateTime? CompletedAt { get; set; }
 public string? RejectionReason { get; set; }
 public DateTime? ActualStartDate { get; set; }
 public DateTime? ActualEndDate { get; set; }
 public List<TagDto> Tags { get; set; } = new List<TagDto>();
 }

 public class TagDto
 {
 public int Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public string Color { get; set; } = string.Empty;
 }

 public class AssignTaskModel
 {
 public string? AssigneeEmail { get; set; }
 }

 public class AcceptRejectModel
 {
 public bool Accept { get; set; }
 public string? RejectionReason { get; set; }
 }

 public enum TaskStatus
 {
 Pending =0,
 Assigned =1,
 Accepted =2,
 InProgress =3,
 PendingApproval =4,
 Completed =5,
 Approved =6,
 Rejected =7
 }

 public class CompleteTaskModel
 {
 public DateTime? ActualStartDate { get; set; }
 public DateTime? ActualEndDate { get; set; }
 }

 public class ApproveRejectModel
 {
 public bool Approve { get; set; }
 public string? RejectionReason { get; set; }
 }

 public class TaskHistoryDto
 {
 public int Id { get; set; }
 public int TaskId { get; set; }
 public string Action { get; set; } = string.Empty;
 public string PerformedBy { get; set; } = string.Empty;
 public string PerformedByEmail { get; set; } = string.Empty;
 public string? Details { get; set; }
 public int? OldStatus { get; set; }
 public int? NewStatus { get; set; }
 public DateTime PerformedAt { get; set; }
 }

 // Tag create model
 public class CreateTagModel
 {
 public string Name { get; set; } = string.Empty;
 public string Color { get; set; } = "#cccccc";
 }

 // Admin user DTO
 public class UserDto
 {
 public string Id { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string? FirstName { get; set; }
 public string? LastName { get; set; }
 public DateTime CreatedAt { get; set; }
 public List<string> Roles { get; set; } = new List<string>();
 }
}

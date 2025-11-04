public class MemberDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public bool CanEdit { get; set; }
    public bool CanShare { get; set; }
    public DateTime JoinTime { get; set; }
}

namespace BHunter_Final.Models;

public class GroupPage
{
    public User CurrentUser { get; set; }

    public Group? CurrentGroup { get; set; }

    public List<Group> AvailableGroups { get; set; } = new();

    public List<Messages> Messages { get; set; } = new();

    public List<Movie> Movies { get; set; } = new();

    public bool IsGroupCreator =>
        CurrentGroup != null && CurrentUser != null && CurrentGroup.CreatedByUserId == CurrentUser.UserId;

    public string? NewGroupName { get; set; }
    public string? NewGroupDescription { get; set; }

    public string? NewMessageText { get; set; }

    public int? SelectedMovieId { get; set; }
}
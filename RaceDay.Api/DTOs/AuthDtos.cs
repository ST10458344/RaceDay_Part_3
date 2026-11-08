namespace RaceDay.Api.DTOs
{
    // Used when a user registers a new account
    public record RegisterDto(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? PhoneNumber
    );


    // Used when a user logs in
    public record LoginDto(
        string Email,
        string Password
    );

    // Used when an Organiser creates a new race event
    public record CreateEventDto(
        string Name,
        string Description,
        DateTime EventDate,
        TimeSpan StartTime,
        string Location,
        string RouteDescription,
        int MaxParticipants,
        bool IsPublished
    );
}
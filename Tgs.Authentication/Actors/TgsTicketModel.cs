namespace Tgs.Authentication.Actors;

public class TgsTicketModel
{
    public required Guid Id { get; set; }
    public required byte[] TicketData { get; set; }
}
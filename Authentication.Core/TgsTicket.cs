namespace Tgs.Authentication;
public record TgsTicket(
	Guid Id,
	TokenType Type,
	byte[] TicketData,
	DateTime Expiry,
	bool Enabled);

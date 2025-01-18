using System.ComponentModel;
using Billing.Domain.SeedWorks;

namespace Billing.Domain.Billing;

public class CommandType : ValueObject<CommandType>
{
    public static CommandType Select => new(1, "SELECT");
    public static CommandType Update => new(2, "UPDATE");
    public static CommandType Delete => new(3, "DELETE");
    public static CommandType Insert => new(3, "INSERT");

    private CommandType(byte code, string title)
    {
        Code = code;
        Title = title;
    }

    public byte Code { get; init; }
    public string Title { get; init; }

    public static CommandType FromCode(byte code)
        => code switch
        {
            1 => Select,
            2 => Update,
            3 => Delete,
            4 => Insert,
            _ => throw new InvalidEnumArgumentException(nameof(code))
        };

    public static List<CommandType> GetAll() => [Select, Update, Delete, Insert];

    public static List<CommandType> ConvertToCommandType(List<string> commands)
    {
        var commandTypes = CommandType.GetAll();
        return commands
            .Select(c => commandTypes.FirstOrDefault(t => c.Equals(t.Title, StringComparison.OrdinalIgnoreCase)))
            .Where(x => x is not null).ToList()!;
    }
    protected override IEnumerable<object>? GetEqualityComponents()
    {
        yield return Code;
        yield return Title;
    }
}
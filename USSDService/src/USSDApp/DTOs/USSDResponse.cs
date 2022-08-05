using System.Text;

namespace USSDTest.DTOs;

public class USSDResponse
{
    private readonly StringBuilder _messageBuilder = new();
    private bool _endSession;

    public static USSDResponse InvalidInput { get; } = new USSDResponse().AddMessage("Invalid input");
    public string Text => $"{(_endSession ? "END" : "CON")}{_messageBuilder}";


    public USSDResponse AddMessage(string message)
    {
        _messageBuilder.Append(message).AppendLine();

        return this;
    }

    public USSDResponse AddEndLine()
    {
        _messageBuilder.AppendLine();

        return this;
    }

    public USSDResponse AddOption(string optionNumber, string optionMessage)
    {
        _messageBuilder
            .AppendLine()
            .Append(optionNumber)
            .Append(". ")
            .Append(optionMessage);

        return this;
    }
    
    public USSDResponse EndSession()
    {
        _endSession = true;

        return this;
    }

}
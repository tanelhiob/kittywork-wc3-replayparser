namespace kittywork.Wc3ReplayParser.Business;

public interface IReplayParser
{
    ReplayInfo Parse(string filePath);
    ReplayInfo Parse(Stream stream);
}

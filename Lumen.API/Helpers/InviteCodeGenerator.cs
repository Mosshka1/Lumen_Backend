namespace Lumen.API.Helpers;

public static class InviteCodeGenerator
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int length = 8)
    {
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => Chars[random.Next(Chars.Length)])
            .ToArray());
    }
}

using System;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            Console.WriteLine(FormatOwnedCode.Main(Array.IndexOf(args, "--fix") < 0));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}

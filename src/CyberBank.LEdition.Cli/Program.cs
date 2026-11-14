namespace CyberBank.LEdition.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Any(argument => string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine(LocalCliHelp.Text);
            return 0;
        }

        var result = await new LocalCliExecutor().ExecuteAsync(args);
        Console.WriteLine(result.Message);
        return result.Succeeded ? 0 : 2;
    }
}

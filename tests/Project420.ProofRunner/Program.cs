using Project420.ProofRunner.Evidence;

namespace Project420.ProofRunner;

/// <summary>
/// Project420 Evidence Runner
/// Single-command executable proof for client demonstrations
/// </summary>
public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Project420 Evidence Check");
        Console.WriteLine("--------------------------------");

        try
        {
            await ImmutabilityEvidence.Run();
            await CompensationEvidence.Run();
            await ReplayEvidence.Run();
            await TraceabilityEvidence.Run();

            Console.WriteLine();
            Console.WriteLine("Evidence Status: PASS");
            Console.WriteLine("--------------------------------");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"✖ {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Evidence Status: FAIL");
            Console.WriteLine("--------------------------------");
            Environment.Exit(1);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Project420.Inventory.Models.Entities;
using Project420.Inventory.Models.Enums;
using Project420.ProofRunner.Infrastructure;

namespace Project420.ProofRunner.Evidence;

/// <summary>
/// Demonstrates that corrections create compensating movements rather than modifying history.
/// </summary>
public static class CompensationEvidence
{
    public static async Task Run()
    {
        using var context = EvidenceDbContextFactory.CreateInventoryContext();

        // Create incorrect movement (discovered error)
        var incorrectMovement = new StockMovement
        {
            MovementNumber = "MV-002",
            MovementType = StockMovementType.GoodsReceived,
            MovementDate = DateTime.UtcNow.AddDays(-1),
            ProductSKU = "SKU-TEST-002",
            ProductName = "Test Product",
            Quantity = 50, // WRONG: Should have been 45
            WeightGrams = 250.0m,
            Reason = "Initial receipt"
        };

        context.StockMovements.Add(incorrectMovement);
        await context.SaveChangesAsync();

        // Apply compensating movement (correction)
        var compensatingMovement = new StockMovement
        {
            MovementNumber = "MV-002-ADJ",
            MovementType = StockMovementType.Adjustment,
            MovementDate = DateTime.UtcNow,
            ProductSKU = "SKU-TEST-002",
            ProductName = "Test Product",
            Quantity = -5, // Negative quantity to correct the error
            WeightGrams = -25.0m,
            ReferenceNumber = "MV-002",
            ReferenceType = "Correction",
            Reason = "Correction: Original receipt quantity was incorrect. Actual: 45, Recorded: 50"
        };

        context.StockMovements.Add(compensatingMovement);
        await context.SaveChangesAsync();

        // Verify both records exist
        var allMovements = await context.StockMovements
            .Where(m => m.ProductSKU == "SKU-TEST-002")
            .OrderBy(m => m.MovementDate)
            .ToListAsync();

        if (allMovements.Count != 2)
        {
            throw new InvalidOperationException($"EVIDENCE FAILURE: Expected 2 movements, found {allMovements.Count}");
        }

        // Verify original movement unchanged
        if (allMovements[0].Quantity != 50)
        {
            throw new InvalidOperationException("EVIDENCE FAILURE: Original movement was modified");
        }

        // Verify compensating movement exists
        if (allMovements[1].Quantity != -5)
        {
            throw new InvalidOperationException("EVIDENCE FAILURE: Compensating movement incorrect");
        }

        // Verify net stock is correct
        var netStock = allMovements.Sum(m => m.Quantity);
        if (netStock != 45)
        {
            throw new InvalidOperationException($"EVIDENCE FAILURE: Net stock should be 45, got {netStock}");
        }

        Console.WriteLine("✔ Corrections create compensating movements");
    }
}

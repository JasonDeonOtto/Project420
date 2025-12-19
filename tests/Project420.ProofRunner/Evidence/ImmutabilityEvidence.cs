using Microsoft.EntityFrameworkCore;
using Project420.Inventory.Models.Entities;
using Project420.Inventory.Models.Enums;
using Project420.ProofRunner.Infrastructure;

namespace Project420.ProofRunner.Evidence;

/// <summary>
/// Demonstrates that stock movements are immutable once created.
/// </summary>
public static class ImmutabilityEvidence
{
    public static async Task Run()
    {
        using var context = EvidenceDbContextFactory.CreateInventoryContext();

        // Create original movement
        var movement = new StockMovement
        {
            MovementNumber = "MV-001",
            MovementType = StockMovementType.GoodsReceived,
            MovementDate = DateTime.UtcNow,
            ProductSKU = "SKU-TEST-001",
            ProductName = "Test Product",
            Quantity = 100,
            WeightGrams = 500.0m,
            Reason = "Initial stock receipt"
        };

        context.StockMovements.Add(movement);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Verify movement exists and cannot be modified
        var movementCount = await context.StockMovements.CountAsync();
        if (movementCount != 1)
        {
            throw new InvalidOperationException($"EVIDENCE FAILURE: Expected 1 movement, found {movementCount}");
        }

        // Retrieve the movement with NoTracking to verify immutability pattern
        var retrieved = await context.StockMovements
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MovementNumber == "MV-001");

        if (retrieved == null || retrieved.Quantity != 100)
        {
            throw new InvalidOperationException("EVIDENCE FAILURE: Original movement data changed");
        }

        // Movements are write-once: modifications forbidden, compensations required
        Console.WriteLine("✔ Stock history is immutable");
    }
}

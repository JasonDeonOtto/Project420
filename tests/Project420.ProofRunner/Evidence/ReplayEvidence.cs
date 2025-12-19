using Microsoft.EntityFrameworkCore;
using Project420.Inventory.Models.Entities;
using Project420.Inventory.Models.Enums;
using Project420.ProofRunner.Infrastructure;

namespace Project420.ProofRunner.Evidence;

/// <summary>
/// Demonstrates that stock can be reconstructed as-of any historical date.
/// </summary>
public static class ReplayEvidence
{
    public static async Task Run()
    {
        using var context = EvidenceDbContextFactory.CreateInventoryContext();

        var baseDate = DateTime.UtcNow.AddDays(-10);

        // Day 1: Receive 100 units
        var movement1 = new StockMovement
        {
            MovementNumber = "MV-003",
            MovementType = StockMovementType.GoodsReceived,
            MovementDate = baseDate.AddDays(1),
            ProductSKU = "SKU-TEST-003",
            ProductName = "Test Product",
            Quantity = 100,
            Reason = "Initial stock"
        };

        // Day 3: Sell 30 units
        var movement2 = new StockMovement
        {
            MovementNumber = "MV-004",
            MovementType = StockMovementType.Sale,
            MovementDate = baseDate.AddDays(3),
            ProductSKU = "SKU-TEST-003",
            ProductName = "Test Product",
            Quantity = -30,
            Reason = "Retail sale"
        };

        // Day 7: Receive 50 units
        var movement3 = new StockMovement
        {
            MovementNumber = "MV-005",
            MovementType = StockMovementType.GoodsReceived,
            MovementDate = baseDate.AddDays(7),
            ProductSKU = "SKU-TEST-003",
            ProductName = "Test Product",
            Quantity = 50,
            Reason = "Restock"
        };

        context.StockMovements.AddRange(movement1, movement2, movement3);
        await context.SaveChangesAsync();

        // Calculate stock as-of Day 2 (after first receipt, before sale)
        var asOfDay2 = baseDate.AddDays(2);
        var stockDay2 = await context.StockMovements
            .Where(m => m.ProductSKU == "SKU-TEST-003" && m.MovementDate <= asOfDay2)
            .SumAsync(m => m.Quantity);

        // Calculate stock as-of Day 5 (after sale, before restock)
        var asOfDay5 = baseDate.AddDays(5);
        var stockDay5 = await context.StockMovements
            .Where(m => m.ProductSKU == "SKU-TEST-003" && m.MovementDate <= asOfDay5)
            .SumAsync(m => m.Quantity);

        // Calculate stock as-of Day 10 (after all movements)
        var asOfDay10 = baseDate.AddDays(10);
        var stockDay10 = await context.StockMovements
            .Where(m => m.ProductSKU == "SKU-TEST-003" && m.MovementDate <= asOfDay10)
            .SumAsync(m => m.Quantity);

        // Verify different results for different dates
        if (stockDay2 != 100)
        {
            throw new InvalidOperationException($"EVIDENCE FAILURE: Stock on Day 2 should be 100, got {stockDay2}");
        }

        if (stockDay5 != 70)
        {
            throw new InvalidOperationException($"EVIDENCE FAILURE: Stock on Day 5 should be 70, got {stockDay5}");
        }

        if (stockDay10 != 120)
        {
            throw new InvalidOperationException($"EVIDENCE FAILURE: Stock on Day 10 should be 120, got {stockDay10}");
        }

        // Verify results differ (time-based reconstruction works)
        if (stockDay2 == stockDay5 || stockDay5 == stockDay10)
        {
            throw new InvalidOperationException("EVIDENCE FAILURE: Stock levels should differ across dates");
        }

        Console.WriteLine("✔ Stock can be reconstructed as-of a date");
    }
}

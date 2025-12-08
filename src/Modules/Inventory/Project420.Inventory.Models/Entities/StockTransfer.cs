using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;

namespace Project420.Inventory.Models.Entities
{
    /// <summary>
    /// Represents a transfer of stock between locations
    /// </summary>
    /// <remarks>
    /// Use cases:
    /// - Warehouse → Retail store
    /// - Store A → Store B
    /// - Production → Warehouse
    /// - Main location → Satellite location
    /// </remarks>
    public class StockTransfer : AuditableEntity
    {
        /// <summary>
        /// Unique transfer number
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Transfer Number")]
        public string TransferNumber { get; set; } = string.Empty;

        /// <summary>
        /// Transfer date
        /// </summary>
        [Required]
        [Display(Name = "Transfer Date")]
        public DateTime TransferDate { get; set; }

        /// <summary>
        /// Source location
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "From Location")]
        public string FromLocation { get; set; } = string.Empty;

        /// <summary>
        /// Destination location
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "To Location")]
        public string ToLocation { get; set; } = string.Empty;

        /// <summary>
        /// Transfer status
        /// </summary>
        /// <remarks>
        /// Values: "Pending", "In Transit", "Received", "Cancelled"
        /// </remarks>
        [MaxLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        /// <summary>
        /// Who requested the transfer
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Requested By")]
        public string? RequestedBy { get; set; }

        /// <summary>
        /// Who authorized the transfer
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Authorized By")]
        public string? AuthorizedBy { get; set; }

        /// <summary>
        /// Notes about this transfer
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}

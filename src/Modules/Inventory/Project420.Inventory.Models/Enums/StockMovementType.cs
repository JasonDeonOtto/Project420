using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Inventory.Models.Enums
{
    /// <summary>
    /// Types of stock movements (increases and decreases)
    /// </summary>
    public enum StockMovementType
    {
        /// <summary>
        /// Stock received from production/supplier
        /// </summary>
        [Display(Name = "Goods Received")]
        GoodsReceived = 0,

        /// <summary>
        /// Stock sold to customer (POS sale)
        /// </summary>
        [Display(Name = "Sale")]
        Sale = 1,

        /// <summary>
        /// Stock transferred to another location
        /// </summary>
        [Display(Name = "Transfer Out")]
        TransferOut = 2,

        /// <summary>
        /// Stock received from another location
        /// </summary>
        [Display(Name = "Transfer In")]
        TransferIn = 3,

        /// <summary>
        /// Stock adjustment (increase/decrease due to count variance)
        /// </summary>
        [Display(Name = "Adjustment")]
        Adjustment = 4,

        /// <summary>
        /// Stock returned from customer
        /// </summary>
        [Display(Name = "Return")]
        Return = 5,

        /// <summary>
        /// Stock damaged/destroyed (waste tracking)
        /// </summary>
        [Display(Name = "Waste/Destruction")]
        Waste = 6
    }
}

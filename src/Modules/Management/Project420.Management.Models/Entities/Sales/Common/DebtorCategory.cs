using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Shared.Core.Entities;

namespace Project420.Management.Models.Entities.Sales.Common
{
    public class DebtorCategory : AuditableEntity
    {

        [MaxLength(50)]
        [Display(Name = "Debtor Category Name")]
        public string? Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Debtor Code")]
        public string DebtorCode { get; set; } 

        [Required]
        [Display(Name = "Special Rules")]
        public bool SpecialRules { get; set; } = false;

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = false;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project420.Production.Models.Enums
{
    public enum ProductionBatchStatus
    {
        [Display(Name = "New")]
        New = 0,

        [Display(Name = "Open")]
        Open = 1,

        [Display(Name = "In Process")]
        InProcess = 2, // IF qc OR lab Fail mark as failed

        [Display(Name = "Lab Failed")]
        LabFailed = 3,

        [Display(Name = "Completed")]
        Completed = 4,

        [Display(Name = "Cancelled")] //or deleted? soft deletes or Cancelled 
        Cancelled = 4,
    }
}

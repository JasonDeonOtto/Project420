using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Shared.Core.Entities
{
    /// <summary>
    /// Base entity class that provides audit trail and soft delete functionality.
    /// ALL entities in the system must inherit from this class to ensure POPIA compliance.
    /// </summary>
    /// <remarks>
    /// POPIA (Protection of Personal Information Act) Requirements:
    /// - 7-year audit trail of all data access and modifications
    /// - Soft delete only (no permanent deletion of personal information)
    /// - Must track WHO did WHAT and WHEN for all data changes
    ///
    /// Penalty for non-compliance: Up to R10 million fine or 10 years imprisonment
    /// </remarks>
    public abstract class AuditableEntity
    {
        /// <summary>
        /// Unique identifier for this entity (Primary Key)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Date and time when this record was created (UTC)
        /// </summary>
        /// <remarks>
        /// POPIA Requirement: Must track creation timestamp for audit trail
        /// </remarks>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Username or ID of the user who created this record
        /// </summary>
        /// <remarks>
        /// POPIA Requirement: Must track who created the record
        /// </remarks>
        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when this record was last modified (UTC)
        /// </summary>
        /// <remarks>
        /// POPIA Requirement: Must track modification timestamp for audit trail
        /// Nullable because it's not set until first modification occurs
        /// </remarks>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Username or ID of the user who last modified this record
        /// </summary>
        /// <remarks>
        /// POPIA Requirement: Must track who modified the record
        /// Nullable because it's not set until first modification occurs
        /// </remarks>
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Indicates whether this record has been soft-deleted
        /// </summary>
        /// <remarks>
        /// POPIA Requirement: Cannot permanently delete personal information.
        /// Must use soft delete (mark as deleted) instead of hard delete.
        /// This allows for data retention requirements and audit compliance.
        ///
        /// Soft Delete Benefits:
        /// - Maintains referential integrity
        /// - Preserves audit trail
        /// - Allows data recovery if needed
        /// - Complies with 7-year retention requirement
        /// </remarks>
        [Required]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Date and time when this record was soft-deleted (UTC)
        /// </summary>
        /// <remarks>
        /// Optional field to track WHEN deletion occurred for audit purposes
        /// </remarks>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Username or ID of the user who deleted this record
        /// </summary>
        /// <remarks>
        /// Optional field to track WHO performed the deletion for audit purposes
        /// </remarks>
        [MaxLength(100)]
        public string? DeletedBy { get; set; }
    }

    //TODO: Dates should be using system time nad reginal settings.
    //      Users should be a numeric code that can reference back to the data layerfor phase  containing the WHO (Eg User -> ID\Code,Name,etc..)
}

﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Bill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount owing must be a positive value.")]
        public decimal CurrentAmountOwing { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Paid amount must be a positive value.")]
        public decimal BillPaidAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PaidDate { get; set; }

        [Required]
        public virtual User User { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
    }
}
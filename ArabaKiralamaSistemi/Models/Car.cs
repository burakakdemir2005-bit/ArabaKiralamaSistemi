using System.ComponentModel.DataAnnotations;

namespace ArabaKiralamaSistemi.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; } // Benzersiz numara

        [Display(Name = "Marka")]
        [Required(ErrorMessage = "Marka girmek zorunludur.")]
        [StringLength(50)]
        public string Brand { get; set; } // Örn: Toyota

        [Display(Name = "Model")]
        [Required(ErrorMessage = "Model girmek zorunludur.")]
        [StringLength(50)]
        public string Model { get; set; } // Örn: Corolla

        [Display(Name = "Plaka")]
        [Required]
        [StringLength(20)]
        public string Plate { get; set; } // Örn: 34 ABC 123

        [Display(Name = "Günlük Ücret")]
        [Required]
        public decimal DailyPrice { get; set; } // Örn: 1500.00

        [Display(Name = "Yıl")]
        public int Year { get; set; } // Örn: 2023

        [Display(Name = "Müsait mi?")]
        public bool IsAvailable { get; set; } = true; // Kirada mı, boşta mı?

        [Display(Name = "Resim URL")]
        public string ImageUrl { get; set; } // Arabanın resmi için link
    }
}
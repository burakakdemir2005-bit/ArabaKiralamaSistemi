using System;
using System.ComponentModel.DataAnnotations;

namespace ArabaKiralamaSistemi.Models
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }

        // Hangi Araba? (Araba ile ilişki)
        public int CarId { get; set; }
        public virtual Car Car { get; set; }

        // Kim Kiraladı? (Kullanıcı ile ilişki)
        // Not: Identity sisteminde User ID'ler genelde 'string' (uzun yazı) olur.
        public string UserId { get; set; }

        // Ne Zaman Alacak?
        [Required(ErrorMessage = "Alış tarihi zorunludur.")]
        [Display(Name = "Alış Tarihi")]
        [DataType(DataType.Date)]
        public DateTime RentDate { get; set; }

        // Ne Zaman İade Edecek?
        [Required(ErrorMessage = "İade tarihi zorunludur.")]
        [Display(Name = "İade Tarihi")]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        // Toplam Tutar Ne Kadar?
        [Display(Name = "Toplam Tutar")]
        public decimal TotalPrice { get; set; }
    }
}
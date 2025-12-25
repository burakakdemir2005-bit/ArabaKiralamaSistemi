// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Sayfa tamamen yüklendiğinde çalışsın
document.addEventListener("DOMContentLoaded", function () {

    // Sitedeki tüm input (giriş) ve select (seçim) kutularını bul
    var inputs = document.querySelectorAll('input, select, textarea');

    inputs.forEach(function (input) {

        // Mouse üzerine gelince (JS ile kontrol)
        input.addEventListener('mouseenter', function () {
            this.style.backgroundColor = "#e8f0fe"; // Açık mavi yap
        });

        // Mouse üzerinden gidince
        input.addEventListener('mouseleave', function () {
            this.style.backgroundColor = ""; // Eski haline döndür
        });
    });

    console.log("Animasyon efektleri yüklendi!"); // Tarayıcı konsoluna not düşelim
});
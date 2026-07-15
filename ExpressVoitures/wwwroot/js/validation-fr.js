// jQuery Validate attend un point comme séparateur décimal (usage anglophone).
// Ces deux surcharges lui apprennent à accepter aussi la virgule française.
// Le serveur, lui, est déjà en fr-FR : il accepte la virgule nativement.
$(function () {
    if (typeof $.validator === "undefined") {
        return;
    }

    // Accepte "1800", "1800,50" et "1800.50"
    $.validator.methods.number = function (value, element) {
        return this.optional(element) || /^-?\d+(?:[.,]\d+)?$/.test(value);
    };

    // Compare les bornes après avoir normalisé la virgule en point
    $.validator.methods.range = function (value, element, param) {
        var nombre = parseFloat(String(value).replace(",", "."));
        return this.optional(element) || (nombre >= param[0] && nombre <= param[1]);
    };
});

// Affiche un aperçu local de l'image choisie, avant l'envoi au serveur.
// Chaque input de type "file" portant un attribut data-apercu="<id de l'image>"
// met à jour l'image correspondante dès qu'un fichier est sélectionné.
document.querySelectorAll('input[type="file"][data-apercu]').forEach(function (input) {
    input.addEventListener('change', function () {
        var cible = document.getElementById(input.getAttribute('data-apercu'));
        var fichier = input.files && input.files[0];
        if (cible && fichier) {
            cible.src = URL.createObjectURL(fichier);   // URL locale temporaire, sans upload
            cible.style.display = 'block';
        }
    });
});

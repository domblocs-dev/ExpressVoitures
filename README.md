# Express Voitures

Application web de gestion du stock pour la concession de voitures d'occasion **Express Voitures**.

Projet réalisé dans le cadre du **Projet 5** du parcours Développeur back-end .NET (OpenClassrooms).

## Contexte

Jacques, gérant d'Express Voitures, achète des voitures aux enchères, les répare, puis les revend.
Il souhaite une application web pour gérer son stock à la place de ses feuilles de calcul, et publier une
vitrine consultable par le public.

Règle métier de tarification : **Prix de vente = Prix d'achat + Coût des réparations + 500 €**
(recalculé tant que la voiture n'est pas vendue, puis figé à la vente).

## Fonctionnalités

**Visiteur (public, sans connexion)**
- Consulter la vitrine des voitures (accueil en cartes)
- Voir la fiche détaillée d'une voiture (photo, prix, caractéristiques, statut)

**Gérant (authentifié)**
- Ajouter, modifier, supprimer une voiture (avec téléversement de photo)
- Gérer les réparations d'une voiture (le prix de vente se recalcule automatiquement)
- Marquer une voiture comme vendue (renseigner la date de vente fige le prix)
- Gérer les référentiels : marques, modèles, types de réparation (avec coût par défaut)
- Consulter l'inventaire interne (avec prix d'achat, non visible du public)

> **Statut : application complète et fonctionnelle** (back-end, front-end conforme aux maquettes, sécurité).

## Stack technique

- **ASP.NET Core MVC** (.NET 8)
- **Entity Framework Core** (approche *Code First*)
- **SQL Server** (LocalDB en développement)
- **ASP.NET Core Identity** (authentification et autorisation par rôles)
- **Bootstrap 5** (mise en page responsive)

## Prérequis

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- SQL Server ou **SQL Server Express LocalDB** (fourni avec Visual Studio)
- Visual Studio 2022/2026 (recommandé) ou l'outil `dotnet-ef`

## Installation et lancement

```bash
# 1. Cloner le dépôt
git clone https://github.com/domblocs-dev/ExpressVoitures.git
cd ExpressVoitures

# 2. Fournir le mot de passe du gérant (secret, non versionné)
dotnet user-secrets set "Gerant:MotDePasse" "Gerant@2026" --project ExpressVoitures
#    (ou, en alternative : variable d'environnement Gerant__MotDePasse=Gerant@2026)

# 3. Créer la base de données à partir des migrations
dotnet ef database update --project ExpressVoitures

# 4. Lancer l'application (profil HTTPS)
dotnet run --project ExpressVoitures --launch-profile https
```

L'application s'ouvre sur `https://localhost:7056` (ou le port indiqué dans la console).

La chaîne de connexion par défaut (dans `ExpressVoitures/appsettings.json`) pointe vers LocalDB.
Adaptez-la si vous utilisez une autre instance SQL Server.

## Compte gérant (démonstration)

Le rôle « Gérant » et le compte du gérant sont **créés automatiquement au premier démarrage** (via un
*seeder*). Aucune inscription n'est nécessaire.

| Champ | Valeur |
|-------|--------|
| Email | `gerant@expressvoitures.fr` |
| Mot de passe | `Gerant@2026` |

> **Note de sécurité.** Le mot de passe n'est **pas versionné** : il est fourni par les *user-secrets*,
> via la commande `dotnet user-secrets set "Gerant:MotDePasse" "Gerant@2026" --project ExpressVoitures`
> (depuis la racine du dépôt).
> Ainsi, aucun secret ne figure dans le dépôt. Sans lui, le compte gérant n'est pas créé (un avertissement
> est journalisé au démarrage). L'inscription publique est par ailleurs désactivée : seul le gérant peut
> administrer le site.
>
> Le compte gérant est synchronisé sur ce secret au démarrage : **modifier le mot de passe et relancer
> l'application** suffit à le changer (une modification de configuration nécessite un redémarrage).

## Sécurité

- **Authentification** : ASP.NET Core Identity (mots de passe hachés, sessions, cookies).
- **Autorisation par rôle** : `[Authorize(Roles = "Gérant")]` sur toutes les actions de création,
  modification et suppression ; la consultation reste publique (`[AllowAnonymous]`).
- **Anti-CSRF** : `[ValidateAntiForgeryToken]` sur chaque formulaire (POST).
- **Téléversement de photos sécurisé** : nom de fichier généré (`Guid`, anti-traversée de chemin),
  liste blanche d'extensions, taille maximale, contrôle du type.
- **Validation serveur** systématique (règles métier, cohérence des dates, année 1990–courante).

## Accessibilité

- Langue de la page déclarée (`<html lang="fr">`).
- **Contraste WCAG** : la teinte ambre de la maquette (contraste 2,15:1 sur blanc, non conforme) a été
  remplacée par une variante plus foncée (5,78:1) pour le texte, tout en conservant l'identité visuelle.
- Textes alternatifs descriptifs sur les images, libellés associés aux champs, champs obligatoires signalés.
- Dates saisies via un sélecteur de calendrier natif.

## Modèle de données

Le modèle complet (schéma relationnel, diagramme, justifications de conception) est disponible en PDF :

- [docs/Modele_de_donnees_Express_Voitures.pdf](docs/Modele_de_donnees_Express_Voitures.pdf)

Entités : `Marque`, `Modele`, `Voiture`, `Vente` (relation 1–1 avec `Voiture`), `Reparation`
(entité d'association), `TypeReparation`.

## Livrables

- Code source (ce dépôt) avec ce README
- Modèle de données au format PDF (dossier `docs/`)

## Auteur

Dominique — Projet 5 OpenClassrooms.

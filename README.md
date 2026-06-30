# Express Voitures

Prototype d'application web de gestion du stock pour la concession de voitures d'occasion **Express Voitures**.

Projet réalisé dans le cadre du **Projet 5** du parcours Développeur back-end .NET.

## Contexte

Jacques, gérant d'Express Voitures, achète des voitures aux enchères, les répare, puis les revend.
Il souhaite une application web pour gérer son stock à la place de ses feuilles de calcul.

Règle métier de tarification : **Prix de vente = Prix d'achat + Coût des réparations + 500 €**.

## Fonctionnalités

- Consulter les voitures de l'inventaire (accès ouvert à tous)
- Ajouter une voiture, modifier une annonce (photo, description)
- Marquer une voiture comme vendue
- Seul le gérant (authentifié) peut ajouter ou modifier ; la consultation reste publique

> Statut : projet en cours de développement. L'infrastructure de données (entités + base) est en place ; le back-end et les vues sont en cours.

## Stack technique

- **ASP.NET Core MVC** (.NET 8)
- **Entity Framework Core** (approche *Code First*)
- **SQL Server** (LocalDB en développement)
- **ASP.NET Core Identity** (authentification et autorisation)

## Modèle de données

Le modèle complet (schéma relationnel, diagramme UML, détail des tables et justifications de conception) est disponible en PDF :

- [docs/Modele_de_donnees_Express_Voitures.pdf](docs/Modele_de_donnees_Express_Voitures.pdf)

Entités principales : `Marque`, `Modele`, `Voiture`, `Vente`, `Reparation`, `TypeReparation`.

## Prérequis

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- SQL Server ou **SQL Server Express LocalDB** (fourni avec Visual Studio)
- Visual Studio 2022/2026 (recommandé) ou l'outil `dotnet-ef`

## Installation et lancement

```bash
# 1. Cloner le dépôt
git clone https://github.com/domblocs-dev/ExpressVoitures.git
cd ExpressVoitures

# 2. Créer la base de données à partir des migrations
dotnet ef database update --project ExpressVoitures

# 3. Lancer l'application
dotnet run --project ExpressVoitures
```

La chaîne de connexion par défaut (dans `ExpressVoitures/appsettings.json`) pointe vers LocalDB :
`Server=(localdb)\mssqllocaldb;Database=aspnet-ExpressVoitures;...`
Adaptez-la si vous utilisez une autre instance SQL Server.

## Livrables

- Code source (ce dépôt) avec ce README
- Modèle de données au format PDF (dossier `docs/`)

## Auteur

Dominique - Projet 5 OpenClassrooms.

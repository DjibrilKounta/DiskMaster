# Comprendre la refonte de DiskMaster

## 1. Le trajet d'une action

Quand tu cliques sur **Analyser** :

1. `MainWindow.xaml` transmet l'action à `ScanCommand` par un binding.
2. `MainViewModel.ScanAsync` active l'état occupé et crée un jeton d'annulation.
3. `ICleanupService.ScanAsync` exécute le parcours sur un thread de travail.
4. Le service retourne des `ScanResult` contenant les fichiers candidats et leurs métadonnées.
5. Le ViewModel affecte les résultats aux catégories sur le thread de l'interface.
6. `PropertyChanged` signale les modifications ; WPF relit les bindings.

Le nettoyage demande une confirmation, utilise les résultats de cette analyse et revérifie les fichiers. Il ne recherche pas de nouveaux fichiers à supprimer. Après une tentative, l'analyse est invalidée pour empêcher la réutilisation d'une liste périmée.

## 2. Les dossiers et leurs responsabilités

| Emplacement | Rôle | Pourquoi ? |
| --- | --- | --- |
| `Models/CleanupModels.cs` | Données : catégories, candidats, résultats, historique | Décrire les données sans dépendre d'une fenêtre |
| `ViewModels/MainViewModel.cs` | État de l'écran et coordination des commandes | Rendre le comportement de présentation indépendant du XAML |
| `ViewModels/CategoryViewModel.cs` | Sélection, taille affichée et statut d'une catégorie | Garder les détails de présentation hors du moteur |
| `Services/CleanupService.cs` | Analyse et suppression contrôlée | Centraliser les règles d'accès aux fichiers |
| `Services/CleanupCatalog.cs` | Liste fermée des catégories | Empêcher un dossier arbitraire de devenir une cible de nettoyage |
| `Services/UserDataService.cs` | Historique JSON et diagnostics locaux | Isoler le stockage de la présentation |
| `Services/DesktopService.cs` | Dialogues et ouverture de Windows | Concentrer la dépendance aux fonctionnalités du bureau |
| `Infrastructure/ObservableObject.cs` | Notifications de propriétés | Mutualiser un mécanisme technique sans le recopier |
| `Infrastructure/Commands.cs` | Commandes et activation des boutons | Décrire une action et les conditions dans lesquelles elle est disponible |
| `App.xaml.cs` | Assemblage des objets au démarrage | Centraliser le choix des implémentations |
| `MainWindow.xaml.cs` | Initialisation et fermeture de la fenêtre | Conserver uniquement le comportement visuel propre à cette fenêtre |

## 3. Les patterns réellement utilisés

### MVVM

La **View** est le XAML. Le **ViewModel** contient les propriétés et commandes qu'il affiche. Les **Models** décrivent les données de travail. Le moteur de nettoyage appartient aux services.

Un ViewModel n'est pas un endroit où déplacer tout l'ancien code-behind : il coordonne le travail et expose son état, sans parcourir lui-même le disque.

### Injection de dépendances par constructeur

`MainViewModel` reçoit `ICleanupService`, `IUserDataService` et `IDesktopService`. Il utilise leurs contrats sans choisir leurs implémentations. Dans un test, on pourrait lui fournir un faux service qui ne touche jamais au disque.

L'assemblage se fait manuellement dans `App.OnStartup`. Un conteneur d'injection n'est pas nécessaire à cette taille de projet.

### Command et Observer

`AsyncCommand` exécute une tâche, empêche sa propre réentrance et relaie les erreurs. `CanExecute` commande l'état des boutons. Les exceptions des commandes synchrones d'ouverture sont aussi gérées.

`INotifyPropertyChanged` informe WPF qu'une propriété a changé ; `ObservableCollection` l'informe des ajouts et retraits. Ce sont deux notifications différentes.

Les notifications de `SelectedSize` et `SelectionLabel` sont aussi nécessaires : ce sont des valeurs calculées à partir des catégories.

### Responsabilité unique

Le service décrit le résultat technique ; le ViewModel compose le message présenté ; le service de bureau affiche la confirmation. Chaque composant a une raison claire de changer.

Il n'y a pas de Repository ni de Strategy ajoutés artificiellement. Trois catégories avec une politique commune ne justifient pas encore cette complexité.

## 4. Asynchronisme et annulation

`Task.Run` déplace le travail de fichiers hors du thread WPF. `await` rend la main à l'interface pendant ce travail. Les résultats sont ensuite appliqués sur le contexte WPF.

`CancellationToken` exprime une demande d'arrêt. L'analyse peut lancer `OperationCanceledException`. Le nettoyage retourne au contraire un bilan partiel : les suppressions déjà effectuées doivent rester comptabilisées. Annuler ne restaure pas les fichiers supprimés.

`Progress<T>` transmet les informations au contexte de l'interface. Le service ne touche jamais aux contrôles WPF.

La barre représente les catégories pendant l'analyse et les fichiers pendant le nettoyage ; elle ne prédit pas le temps restant.

## 5. Politique de nettoyage

- Trois dossiers du profil : `Temp`, `D3DSCache`, `CrashDumps` (uniquement `.dmp`).
- Fichiers non modifiés depuis plus de sept jours ; ce délai réduit le risque mais ne garantit pas qu'un fichier est inutile.
- Fichiers système, en lecture seule, liens et jonctions exclus.
- Vérification du chemin absolu et de ses ancêtres à l'analyse et avant suppression.
- Taille et date de modification comparées à l'instantané.
- Aucun dossier supprimé, aucun attribut de protection forcé.
- Suppression définitive après sélection et confirmation ; pas de restauration intégrée.
- Aucun nettoyage de Defender, pilotes, ressources linguistiques, corbeille ou Windows Update par suppression générique.

Ces vérifications sont une politique prudente, pas une isolation contre un programme malveillant qui modifierait les chemins exactement entre leur contrôle et la suppression (course TOCTOU). Une garantie de cette nature demanderait un moteur Windows fondé sur les handles et une analyse de sécurité dédiée.

Le volume comptabilisé correspond à la somme des tailles logiques des fichiers supprimés, pas à une mesure de l'espace physique libéré : compression, liens matériels et activité concurrente peuvent produire une différence.

## 6. Données locales et confidentialité

L'historique est écrit après l'opération dans `%LOCALAPPDATA%\DiskMaster\history.json`, par remplacement d'un fichier temporaire unique. Le diagnostic local effectue une rotation au-delà d'environ 1 Mo. Une erreur de journalisation n'empêche pas l'application de fonctionner.

Les diagnostics peuvent contenir des chemins locaux : les relire avant de les partager. Aucun envoi réseau, aucune télémétrie et aucune récupération d'actualités ne sont intégrés à cette version.

## 7. Lire et expérimenter

Commence par `CleanupModels.cs`, puis `CategoryViewModel.cs`, `ObservableObject.cs`, `MainViewModel.ScanAsync`, et enfin `CleanupService.cs`.

Trois exercices utiles :

1. Ajouter au ViewModel un texte calculé qui compte les catégories cochées et notifier son changement.
2. Ajouter au bilan la durée de l'opération avec `Stopwatch`, sans changer le service de suppression.
3. Tester un nouveau cas de refus de suppression avec des fichiers de fixture, avant de modifier le moteur.

## 8. Validation et limites de livraison

`dotnet build DiskMaster.slnx -c Release` compile l'application et les tests.

`dotnet run --project DiskMaster.Tests -c Release` lance le programme de tests de sécurité. C'est volontairement un exécutable sans dépendance de test externe ; ce n'est pas un projet découvert par `dotnet test`.

La compilation ne suffit pas à garantir le fonctionnement des bindings WPF : un essai réel du démarrage est nécessaire. Par exemple, `ProgressBar.Value` utilise ici explicitement `Mode=OneWay`, car la propriété du ViewModel n'a pas de setter public.

Les scripts de fabrication et un installateur Inno Setup sont fournis ; voir `RELEASING.md`. Une signature effective avec le certificat de l'éditeur, les essais de mise à niveau et la matrice Windows/DPI/accessibilité restent à qualifier. Le rapport local est dans `VALIDATION.md`. La refonte est une base maintenable ; elle ne constitue pas une certification de production.

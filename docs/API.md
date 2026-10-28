# Contrats internes et API

Ce projet est une application, pas une bibliothèque à API stable. Les éléments publics C# permettent la composition et les tests ; leur visibilité ne promet pas de compatibilité binaire entre versions.

## `ICleanupService`

| Membre | Contrat |
| --- | --- |
| `Categories` | Catalogue fermé de catégories, créé à partir des dossiers connus du profil |
| `ScanAsync(progress, token)` | Lit les métadonnées, retourne un `ScanResult` par catégorie et ne supprime rien. Annulation par `OperationCanceledException` |
| `CleanAsync(selection, progress, token)` | Traite les candidats des résultats sélectionnés. Refuse une catégorie étrangère avant toute suppression. Retourne un bilan partiel si annulé |

Les erreurs d'accès aux fichiers attendues sont comptabilisées et journalisées ; une erreur inattendue peut remonter. Les erreurs attendues ne doivent pas effacer le bilan des fichiers déjà traités. Les candidats sont revérifiés par chemin, attributs, âge, taille et date. Ce n'est pas un protocole contre un attaquant local concurrent.

Les appels de l'application sont séquentiels : pas d'analyse et de nettoyage simultanés sur la même instance. Le ViewModel impose cet ordre. Les callbacks de progression ne doivent pas lancer d'exception. `Progress<T>` est créé sur le contexte WPF et les résultats sont appliqués sur ce contexte.

## Données

- `CleanupCategory` : identité de catégorie, nom, description, racine autorisée.
- `FileCandidate` : chemin absolu, taille logique en octets et dernière modification UTC au moment de l'analyse.
- `ScanResult` : catégorie, candidats, compteur d'éléments ignorés pour erreur ou chemin, présence du dossier. `SizeBytes` additionne les candidats.
- `CleanupResult` : nombre supprimé, ignoré, octets logiques supprimés et annulation.
- `CleanupHistory` : date avec décalage et copie du bilan de la dernière opération.

Les `record` limitent la réaffectation des propriétés. `IReadOnlyList` décrit une lecture seule par contrat, pas une garantie d'immuabilité de toute implémentation possible. Le service retourne ses listes sous forme de collections en lecture seule.

## `IUserDataService`

`Load()` retourne le dernier historique ou `null` s'il est absent ou illisible. `Save()` écrit un fichier temporaire unique puis remplace l'historique ; une erreur remonte au ViewModel, qui conserve le bilan à l'écran. `Log()` ne doit pas faire échouer une opération pour une erreur habituelle d'accès au journal. `Folder` expose le dossier local pour la commande Diagnostics.

## `IDesktopService`

`Confirm()` retourne explicitement un booléen ; le défaut du dialogue réel est le refus. `ShowError()` présente une erreur. `OpenFolder()` et `OpenStorageSettings()` délèguent au bureau Windows. Cette interface permet aux tests d'éviter tout dialogue ou ouverture réelle.

## Commandes et notifications

`AsyncCommand.Execute()` est l'adaptateur `ICommand` pour WPF. `ExecuteAsync()` permet d'attendre la fin en test. Une commande occupée refuse une seconde exécution ; les erreurs sont relayées au gestionnaire fourni ; `CanExecuteChanged` signale les changements d'activation.

`ObservableObject.Set()` n'émet une notification que si la valeur change. Les propriétés calculées dépendantes doivent être notifiées explicitement. `CategoryViewModel.Result` notifie donc aussi `Size` et `Status`.

`MainViewModel` coordonne les services et invalide un instantané après une tentative de nettoyage. Les callbacks tardifs sont filtrés par numéro d'opération et état occupé pour ne pas remplacer un résultat final.

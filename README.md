# DiskMaster

Application Windows de nettoyage du profil utilisateur, développée en C# / WPF avec .NET 10.

## Utilisation

1. Lancez **Analyser** : aucune suppression n'est effectuée.
2. Consultez les catégories, leur description et l'aperçu (200 premiers fichiers de la catégorie).
3. Cochez les catégories souhaitées et choisissez **Nettoyer la sélection**.
4. Relisez la confirmation : la suppression est définitive.
5. Consultez le bilan, puis relancez une analyse pour actualiser les tailles.

Le bouton **Annuler** interrompt le travail dès que possible ; il ne restaure pas les fichiers déjà supprimés. La fermeture est bloquée pendant une opération : annulez puis attendez sa fin.

## Périmètre

Seuls les fichiers non modifiés depuis plus de sept jours sont proposés :

- `%LOCALAPPDATA%\Temp` : fichiers temporaires du profil.
- `%LOCALAPPDATA%\D3DSCache` : cache des shaders, susceptible d'être recréé par les applications.
- `%LOCALAPPDATA%\CrashDumps` : fichiers `.dmp`, utiles au diagnostic des plantages.

Les fichiers en lecture seule, système et les liens/jonctions sont exclus. Un fichier modifié depuis l'analyse est ignoré. Aucun dossier n'est supprimé. Le programme fonctionne sans demander les droits administrateur.

L'ancien nettoyage générique des dossiers Defender, pilotes, langues et Windows Update a été retiré. Le bouton **Stockage Windows** ouvre l'outil Windows pour les opérations supplémentaires.

L'âge d'un fichier ne garantit pas son inutilité : consultez les descriptions et l'aperçu. Les tailles indiquées sont logiques, pas une mesure de l'espace physique réellement récupéré.

## Compiler et tester

Prérequis : Windows et SDK .NET 10. Visual Studio est facultatif.

```powershell
dotnet build DiskMaster.slnx -c Release
dotnet run --project DiskMaster.Tests -c Release
dotnet run --project DiskMaster -c Release
```

Le projet de tests est un exécutable sans dépendance externe. Il valide le moteur sur des fichiers isolés, dont les fichiers verrouillés ou modifiés, l'annulation, les limites de chemins et les jonctions. Il ne nettoie pas les caches réels.

Après compilation, le programme se trouve dans `DiskMaster/bin/Release/net10.0-windows/DiskMaster.exe`. Cette compilation nécessite le runtime .NET Desktop 10 sur le poste d'exécution.

## Architecture et apprentissage

MVVM, commandes asynchrones, services injectés par constructeur, modèles de résultats et notifications WPF. L'interface utilise uniquement les composants WPF intégrés.

Le guide [Comprendre la refonte](docs/ARCHITECTURE.md) explique le rôle de chaque fichier, le trajet d'un clic, les choix techniques et les limites de sécurité.

Pour le parcours complet, commencez par [Créer une application de zéro à sa maintenance](docs/FROM-ZERO-TO-RELEASE.md).

## Documentation

| Sujet | Document |
| --- | --- |
| Utiliser et désinstaller | [Guide utilisateur](docs/USER-GUIDE.md) |
| Besoin et critères d'acceptation | [Périmètre produit](docs/PRODUCT.md) |
| Organisation du code et contrats | [Architecture](docs/ARCHITECTURE.md), [API](docs/API.md) |
| Contribuer | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Changements | [CHANGELOG.md](CHANGELOG.md) |
| Données, droits et composants | [Confidentialité](PRIVACY.md), [notices](THIRD-PARTY-NOTICES.md), [licence](LICENSE) |
| Signaler un problème ou une faille | [Support](SUPPORT.md), [sécurité](SECURITY.md) |
| Tester et connaître les preuves | [Plan de test](docs/TEST-PLAN.md), [validation](docs/VALIDATION.md) |
| Fabriquer, signer et décider de publier | [Livraison](docs/RELEASING.md), [checklist](docs/RELEASE-CHECKLIST.md) |
| Entretenir et traiter les incidents | [Maintenance](docs/MAINTENANCE.md) |

## Validation et fabrication automatisées

```powershell
powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Test.ps1
powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Build-Release.ps1
```

La seconde commande produit une archive autonome Windows x64 avec manifeste et empreintes SHA256 dans `artifacts/releases`. Le runtime est embarqué. L'option `-WithInstaller -IsccPath 'CHEMIN\ISCC.exe'` compile également l'installateur ; la signature requiert le certificat de l'éditeur. Voir le guide de livraison pour les paramètres complets.

Le workflow GitHub Actions valide et fabrique une archive non signée ; il ne publie aucune release publique. Il doit être exécuté sur le dépôt avant de pouvoir annoncer une CI validée.

## Données locales

Historique et diagnostics : `%LOCALAPPDATA%\DiskMaster`. Aucune télémétrie ni requête réseau intégrée. Les diagnostics peuvent inclure des chemins locaux ; vérifiez leur contenu avant de les partager.

## Distribution

Version de développement `0.2.0-alpha.1`. Les scripts de fabrication et la définition d'installateur sont fournis ; leur état de vérification figure dans le rapport de validation. Aucun certificat d'éditeur ni mécanisme de mise à jour automatique n'est fourni. La qualification Windows/DPI/accessibilité et une revue de sécurité complémentaire restent nécessaires avant une diffusion stable.

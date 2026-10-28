# Contribuer à DiskMaster

## Préparer son environnement

Utiliser Windows x64 et le SDK .NET 10. `global.json` accepte la dernière bande de fonctionnalités stable de cette version majeure. Visual Studio avec WPF ou VS Code sont possibles. Inno Setup est nécessaire uniquement pour compiler l'installateur ; le SDK Windows fournit SignTool.

Le dossier livré ici peut être une simple copie sans `.git`. Pour commencer un nouveau dépôt, initialiser Git localement puis créer le dépôt distant dans son propre compte. Aucun dépôt n'a été créé ou publié automatiquement.

## Modifier

1. Décrire le problème et un critère observable de réussite.
2. Créer une branche de travail dans son dépôt et limiter la modification à ce problème.
3. Lire `docs/ARCHITECTURE.md` et conserver les responsabilités View / ViewModel / services.
4. Pour une règle de suppression, écrire d'abord un test de refus et un test d'autorisation dans les fixtures.
5. Lancer `scripts/Test.ps1` depuis PowerShell. Si l'exécution des scripts locaux est bloquée, utiliser une session autorisée par l'organisation ; la commande locale documentée est `powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Test.ps1`, sans modification permanente de la stratégie.
6. Ajouter au changelog les changements visibles et mettre à jour les guides concernés.
7. Relire le diff avant le commit : aucun secret, journal personnel, fixture résiduelle ni fichier de build.
8. Ouvrir une pull request avec le problème, le résultat, les vérifications et les limites.

## Conventions

- Suivre `.editorconfig`, le nullable et les analyseurs ; les avertissements font échouer la compilation.
- Les nouveaux fichiers C# utilisent un namespace de fichier et des noms qui décrivent le domaine.
- Ne pas masquer une erreur de suppression. La comptabiliser, journaliser si utile et poursuivre uniquement si le résultat reste fiable.
- Ne pas afficher de dialogue dans le moteur. Injecter un contrat si une dépendance doit être remplacée en test.
- Ne pas ajouter un pattern uniquement pour son nom : expliquer le problème concret qu'il résout.
- Les tests de suppression ne visent jamais `%TEMP%` ou un cache réel ; les cibles appartiennent aux fixtures.

## Dépendances

Les projets ont un fichier `packages.lock.json`. Après une mise à jour volontaire, effectuer une restauration normale, relire le lockfile puis relancer la restauration verrouillée. La CI échoue si le graphe déclaré et le verrouillage divergent.

Les actions GitHub sont figées par SHA ; Dependabot propose des mises à jour. Le fait qu'un bot ouvre une PR ne remplace pas la revue.

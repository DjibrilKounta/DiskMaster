# Assistance et maintenance

1. Identifier la version et le canal dans `release-manifest.json` du paquet.
2. Relire `docs/USER-GUIDE.md` et vérifier les limites connues dans `CHANGELOG.md`.
3. Pour un bug, fournir les étapes, la version Windows, le résultat attendu et le résultat obtenu. Utiliser des données fictives pour reproduire.
4. Joindre uniquement des extraits de diagnostic relus et expurgés. Ne pas joindre de dump mémoire ni d'archive de fichiers personnels.
5. Pour une vulnérabilité, suivre `SECURITY.md`, plutôt qu'un ticket public détaillé.

Le bouton Diagnostics ouvre le dossier local. Un élément « ignoré » n'est pas nécessairement un bug : fichier modifié, verrouillé, absent ou exclu par la politique. Une analyse partielle indique qu'au moins un élément n'a pas pu être lu ou a été exclu pour des raisons de chemin.

L'alpha n'inclut pas d'assistance contractuelle ni de récupération des fichiers supprimés. En cas de résultat inattendu, interrompre les nouveaux nettoyages, préserver les diagnostics et reproduire uniquement sur une fixture ou une machine virtuelle.

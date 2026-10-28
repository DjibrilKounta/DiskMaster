# Plan de test et qualification

## Automatique

Lancer `scripts/Test.ps1`. La restauration verrouillée, la compilation sans avertissement et le programme de tests doivent réussir. Les tests couvrent : sélection par âge/type, limites de chemins, jonction, fichiers modifiés/verrouillés/absents/protégés, refus d'accès ACL, doublons, annulation avant et pendant suppression, contrats du ViewModel, confirmation, erreurs, persistance, rotation, commandes et chargement WPF avec faux services.

Le test WPF détecte les erreurs de chargement et de binding et vérifie un changement de dimensions. Il n'est pas une validation visuelle, une mesure de contraste ou une qualification du lecteur d'écran. Le programme de tests est un exécutable, pas un projet découvert par `dotnet test`.

## Matrice manuelle à exécuter sur le paquet final

Pour chaque ligne, enregistrer version/empreinte du paquet, date, système, testeur, résultat, capture ou référence au défaut. Ne pas cocher par déduction.

| Environnement ou cas | Essai | Statut initial |
| --- | --- | --- |
| Windows 11 x64, utilisateur standard | Installation et parcours complet sur fixture | À exécuter |
| VM sans SDK ni runtime Desktop | Démarrage du paquet autonome | À exécuter |
| Plus ancienne version Windows annoncée | Installation, démarrage, analyse et désinstallation | À exécuter avant de l'annoncer |
| Affichage 100 %, 150 %, 200 % | Texte, défilement, boutons accessibles, fenêtre minimum | À exécuter |
| Écran à résolution limitée | Accès au bilan et au bouton Annuler | À exécuter |
| Clavier et Narrateur | Ordre de tabulation, cases, commandes, messages | À exécuter |
| Chemins Unicode, longs et dossier redirigé | Analyse, message d'exclusion, aucune sortie de périmètre | À exécuter |
| ACL refusant un sous-dossier | Analyse partielle, autres dossiers encore parcourus | À exécuter en VM |
| Annulation pendant suppression | Bilan partiel exact et nouvelle analyse nécessaire | À exécuter sur fixture volumineuse |
| Fermeture pendant travail | Refus de fermeture et annulation possible | À exécuter |
| Mise à niveau d'une alpha précédente | Même identité d'installation, données conservées | À exécuter |
| Désinstallation | Binaires/raccourcis retirés, autres données intactes | À exécuter |
| Réinstallation et retour de version | Démarrage et historique compatibles, ou limite documentée | À exécuter |
| Signature et téléchargement | Éditeur attendu et empreintes identiques | À exécuter avec certificat réel |

## Règles de livraison

Tout effacement hors périmètre, confirmation contournée, bilan trompeur ou crash reproductible bloque la version stable. Les défauts d'accessibilité ou de compatibilité doivent être corrigés ou conduire à réduire explicitement le périmètre annoncé. Une CI verte sur une machine ne qualifie pas toutes les versions Windows.

Le rapport courant se trouve dans `VALIDATION.md` ; ce plan doit rester un protocole réutilisable.

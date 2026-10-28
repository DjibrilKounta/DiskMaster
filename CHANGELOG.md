# Journal des changements

Les versions non publiées sont décrites ici avant leur mise à disposition. Une entrée n'est pas une preuve de publication.

## 0.2.0-alpha.1 — préparation locale

### Ajouts

- Architecture MVVM, services injectés, commandes asynchrones et notifications WPF.
- Sélection des catégories, aperçu limité à 200 fichiers et confirmation de suppression.
- Annulation, résultats partiels, historique JSON et diagnostics locaux avec rotation.
- Tests du moteur, du ViewModel, des commandes, du stockage et des bindings WPF.
- Scripts de validation, publication autonome x64, archive ZIP, manifeste et SHA256.
- Définition d'installateur utilisateur Inno Setup ; signature optionnelle avec un certificat fourni par l'éditeur.
- CI GitHub Actions et procédures de contribution, sécurité, qualification et livraison.
- Guides utilisateur, architecture, API et création d'une application de A à Z.

### Corrections

- Invalidation des résultats après une tentative de nettoyage.
- Refus des fichiers modifiés depuis l'analyse et des chemins hors périmètre.
- Notifications de progression tardives ignorées après la fin d'une opération.
- Ressource d'icône explicitement rattachée à l'assembly de l'application.
- Écriture de l'historique par fichier temporaire unique.

### Retraits

- Suppression générique dans Defender, DriverStore, ressources linguistiques et Windows Update.
- Nettoyage générique de la corbeille et du dossier Explorer.
- Appels réseau aux actualités pendant le nettoyage et demande systématique d'élévation.
- Dépendances Material Design inutilisées.

### Limites connues

- Alpha : pas de restauration des suppressions, ni de mise à jour automatique.
- Qualification multi-machines et revue de sécurité complémentaires nécessaires.
- Protection par vérification de chemins : ce moteur ne garantit pas l'absence de courses TOCTOU face à un programme malveillant concurrent.
- La signature effective dépend du certificat de l'éditeur ; voir le manifeste du paquet.

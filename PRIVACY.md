# Données et confidentialité

Cette notice décrit le comportement de la version 0.2.0-alpha.1 ; elle ne couvre pas les services tiers employés pour télécharger le programme ou signaler un problème.

## Ce qui est lu

Le programme lit les noms, chemins, tailles, attributs et dates de modification des fichiers dans les catégories du profil. Il ne lit pas leur contenu pour décider de leur éligibilité. Les candidats restent en mémoire pendant la session.

## Ce qui est enregistré

Dans `%LOCALAPPDATA%\DiskMaster` :

- `history.json` : dernière opération, date, nombre de fichiers supprimés ou ignorés, somme de leurs tailles et interruption éventuelle.
- `diagnostic.log` et éventuellement `.old` : messages d'erreur et dates. Ils peuvent contenir des chemins ou des noms locaux. Rotation approximative à 1 Mo pour le journal actif ; un message très long peut dépasser ce seuil.

L'historique est remplacé à chaque opération. Les diagnostics demeurent jusqu'à leur remplacement par rotation ou leur suppression manuelle. Des fichiers temporaires peuvent rester après un arrêt brutal du programme.

## Réseau et partage

Aucune télémétrie, publicité ni fonction d'envoi automatique n'est intégrée. Le bouton Stockage ouvre les paramètres Windows. Le partage d'un rapport ou le téléchargement d'une release relève du service utilisé et de ses propres règles.

## Supprimer ces données

Fermer DiskMaster puis ouvrir `%LOCALAPPDATA%\DiskMaster` dans l'Explorateur et retirer uniquement ce dossier si l'on souhaite effacer l'historique et les diagnostics. Cela ne restaure pas les fichiers nettoyés.

L'installateur conserve ces données à la désinstallation pour éviter une suppression silencieuse de diagnostics. Le dossier d'installation, distinct, est `%LOCALAPPDATA%\Programs\DiskMaster`.

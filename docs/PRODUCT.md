# Périmètre et critères produit

## Objectif

Aider un utilisateur Windows à examiner trois catégories de fichiers de son profil et à supprimer volontairement des candidats. Aucun gain de performance n'est garanti.

## Public et périmètre

Application locale, mono-utilisateur, interface française. Cible de distribution : Windows x64 ; configurations effectivement qualifiées à relever pour chaque release. Pas de serveur, compte utilisateur, télémétrie, abonnement ou base distante.

## Critères d'acceptation

| Scénario | Résultat attendu | Vérification |
| --- | --- | --- |
| Ouverture | Fenêtre utilisable, suppression désactivée | Test WPF + essai manuel |
| Analyse | Métadonnées et total des candidats sans suppression | Tests moteur |
| Dossier absent | Statut explicite et zéro candidat | Test moteur |
| Erreur d'accès | Résultat partiel signalé, pas de taille présentée comme complète | Tests existants + cas ACL manuel |
| Sélection | Total et disponibilité du nettoyage recalculés | Test ViewModel |
| Refus de confirmation | Zéro appel de suppression, aucun historique écrit | Test ViewModel |
| Modification après analyse | Fichier ignoré | Test moteur |
| Annulation | Arrêt demandé, bilan des suppressions conservé | Tests moteur/ViewModel + cas en cours manuel |
| Échec de sauvegarde | Bilan visible, avertissement sur l'historique | Test ViewModel |
| Fermeture pendant opération | Fermeture refusée jusqu'à la fin | Essai manuel |
| Distribution | Paquet autonome, notices, manifeste et empreintes | Script de vérification + VM sans SDK |

## Exclusions et backlog

Les nettoyages de Defender, pilotes, Windows Update, ressources linguistiques, corbeille et chemins arbitraires sont exclus. Pas de récupération, nettoyage planifié, auto-update ou sélection fichier par fichier dans cette alpha.

Priorités futures : revue du moteur face aux modifications concurrentes, qualification accessibilité/DPI, catalogue de catégories fondé sur des opérations Windows documentées, tests d'installation et éventuelle sélection fine. Une nouvelle catégorie doit justifier sa politique de suppression et fournir des tests avant son ajout.

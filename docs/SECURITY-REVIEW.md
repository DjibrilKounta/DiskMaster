# Revue de sécurité et modèle de menace

Cette revue décrit les contrôles du code et les risques restants. Elle ne remplace pas un audit indépendant ni un test d'intrusion.

## Actifs et frontières

Actifs : fichiers du profil, configuration Windows, historique local et confiance dans les paquets distribués. Frontières : catalogue autorisé → parcours du disque, instantané → suppression, application → Explorateur/paramètres, poste de fabrication → paquet téléchargé.

Le système de fichiers change pendant l'opération. Les chemins et métadonnées lus sont des données à revérifier. Le système Windows et le runtime installés restent des dépendances de confiance ; un compte ou un poste déjà compromis dépasse les garanties annoncées.

| Risque | Contrôle présent | Limite ou suite |
| --- | --- | --- |
| Nettoyage d'un dossier arbitraire | Catalogue fermé et validation de catégorie | Pas d'extension du catalogue sans revue |
| Préfixe voisin ou sortie de racine | Chemins absolus avec séparateur de frontière | Tests sur les chemins atypiques à compléter |
| Liens/jonctions | Refus des points de réanalyse, y compris les ancêtres | Contrôle par chemin sujet aux changements concurrents |
| Fichier modifié après analyse | Taille et dernière modification comparées | Métadonnées non équivalentes à une identité de fichier ; une substitution malveillante peut les reproduire |
| Course entre contrôle et suppression | Revérification juste avant suppression | **Non résolu comme garantie anti-attaquant** : étudier ouverture relative par handles et suppression du handle vérifié |
| Fichier utilisé ou protégé | Échec comptabilisé, pas de forçage d'attributs | Les droits du processus restent déterminants |
| Double suppression | Déduplication par chemin | Pas de gestion des tailles physiques de liens matériels |
| Annulation | Jeton vérifié entre fichiers | L'opération système en cours n'est pas interrompue et rien n'est restauré |
| Perte d'historique | Remplacement par fichier temporaire unique | Pas de garantie de persistance en cas de coupure électrique |
| Fuite de diagnostic | Stockage local, aucun envoi intégré | Chemins personnels possibles ; partage à expurger |
| Compromission de CI | Actions par SHA, permissions minimales | Configuration réelle du compte, secrets, protections et revue requises |
| Faux paquet | SHA256 et prise en charge de signature | Signature effective et source de téléchargement à vérifier |

## Décision pour l'alpha

La distribution reste expérimentale. Aucun contrôle ne doit être présenté comme une garantie de récupération ou de résistance à un logiciel malveillant concurrent. Exécuter les essais de suppression avec des données fictives et documenter les défauts bloquants avant de déclarer une version stable.

## Avant une diffusion stable

Faire relire le service par une seconde personne, traiter les courses de chemins selon le niveau de menace retenu, tester les ACL et dossiers redirigés en VM, qualifier le binaire signé et établir un canal de signalement privé. Le mainteneur doit accepter explicitement les risques résiduels et annoncer les limites compatibles avec le produit.

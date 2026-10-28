# Sécurité

## Périmètre actuel

DiskMaster 0.2.0-alpha.1 est une version de développement. Aucune garantie de support de sécurité à long terme n'est annoncée. Les corrections sont destinées à la version en cours ; les versions plus anciennes doivent être remplacées après qualification.

Le programme supprime définitivement des fichiers. Le moteur autorise seulement trois catégories du profil et revérifie les fichiers de l'instantané. Il n'exécute aucune commande fournie par l'utilisateur et n'effectue aucune requête réseau intégrée.

## Signaler une vulnérabilité

Ne pas publier de données personnelles, de contenu de dumps mémoire ou d'exploit destructeur dans une issue publique. Sur le dépôt `DjibrilKounta/DiskMaster`, employer le signalement privé GitHub **si cette fonctionnalité a été activée par le mainteneur**. Sinon, demander au mainteneur un canal privé sans publier les détails de la faille. L'activation de ce canal n'a pas été vérifiée à distance.

Fournir : version, configuration Windows, préconditions, reproduction avec des fichiers fictifs, impact et résultat attendu. Le mainteneur doit convenir du calendrier de divulgation ; aucun délai de réponse fictif n'est promis.

## Limites importantes

- L'âge d'un fichier ne prouve pas qu'il est inutile.
- Il n'y a pas de restauration intégrée.
- Les contrôles de chemins et métadonnées ne constituent pas une isolation contre un attaquant local qui les modifie au même instant (TOCTOU).
- Un journal de diagnostic peut contenir des chemins personnels. Le relire avant partage.
- Une archive SHA256 assure l'intégrité par rapport à la liste obtenue ; seule une source de confiance et, si disponible, une signature vérifiée permettent d'authentifier l'éditeur.

Le modèle de menace et les actions de qualification sont détaillés dans `docs/SECURITY-REVIEW.md`.

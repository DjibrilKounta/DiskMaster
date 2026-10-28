# Maintenir l'application

## À chaque version

Trier les retours selon leur impact, reproduire avec des fixtures, corriger, ajouter un test de non-régression et mettre à jour les guides. Conserver le tag, le paquet final, les empreintes et la preuve de qualification.

La mise à jour de cette alpha est manuelle. L'installateur conserve le même AppId ; les données utilisateur sont séparées des binaires. Tester la compatibilité de `history.json` avant de changer son schéma. Si un changement devient incompatible, prévoir une migration versionnée et une copie de secours avant conversion.

## Dépendances et runtime

Surveiller les correctifs .NET, Windows et les outils de fabrication. Dependabot facilite les propositions pour les packages et actions ; les mises à jour de runtime autonome doivent être republiées dans un nouveau paquet. Relancer les tests et la qualification appropriée après chaque mise à jour.

Éviter une auto-update improvisée qui télécharge puis exécute un binaire sans chaîne d'authenticité. Une future mise à jour automatique doit prévoir signature, origine de confiance, interruption, repli, compatibilité de données et essais dédiés.

## Incident de suppression ou de sécurité

1. Arrêter les nouvelles distributions concernées et suspendre les recommandations de nettoyage.
2. Préserver les sources, paquets, empreintes et diagnostics expurgés. Ne pas lancer de nouvelles suppressions sur le poste touché pour « vérifier ».
3. Évaluer le périmètre et reproduire uniquement en environnement isolé.
4. Corriger et ajouter un test ; qualifier la correction et communiquer des faits vérifiés.
5. Retirer ou marquer la release défectueuse, orienter les utilisateurs vers la version corrigée et documenter les limites de récupération.

Ne pas promettre qu'un retour à un ancien binaire restaure les fichiers supprimés. Le retour de version concerne le programme et éventuellement les données compatibles, pas les effets déjà produits.

## Sauvegarde du projet

Conserver au moins une copie distincte des sources, tags, paquets et preuves. Tester périodiquement une restauration sur une machine propre. Une synchronisation de dossier seule ne protège pas de toutes les suppressions ou corruptions propagées.

## Fin de support

Si le mainteneur ne peut plus corriger le produit, annoncer la fin du support, archiver le dépôt et laisser accessibles les limites et avis de sécurité. Ne pas laisser une ancienne alpha présentée comme un nettoyeur stable maintenu.

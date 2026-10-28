# Créer une application de zéro à sa maintenance

Ce guide utilise DiskMaster comme fil conducteur. Une application professionnelle n'est pas définie par le nombre de patterns ou par son apparence : elle répond à un besoin, maîtrise ses effets, se teste, se livre et se maintient. Les livrables ci-dessous permettent de vérifier chaque étape.

## 1. Définir le besoin avant le code

Écrire qui utilise le logiciel, le problème et le résultat attendu. Pour DiskMaster : aider un utilisateur Windows à examiner certains caches de son profil et supprimer volontairement des fichiers proposés.

Définir aussi les exclusions : pas d'optimisation magique, pas de suppression de pilotes, pas de nettoyage arbitraire du système, pas de restauration intégrée. Une petite application qui tient ses promesses est plus utile qu'une longue liste de fonctions mal maîtrisées.

**Livrable :** `PRODUCT.md`, avec des scénarios et critères d'acceptation. Exemple : « Quand je refuse la confirmation, aucun fichier n'est supprimé et aucun nettoyage n'est enregistré. »

## 2. Choisir le périmètre minimal et les risques

Lister les fonctions indispensables et celles qui peuvent attendre. Prioriser selon l'impact : pour un nettoyeur, une erreur de suppression est plus grave qu'un bouton mal aligné.

Identifier les actifs à protéger, les personnes et logiciels susceptibles d'agir en parallèle, et les limites de confiance. Ici : fichiers du profil, métadonnées changeantes, permissions Windows, liens et dossiers redirigés.

**Livrable :** `SECURITY-REVIEW.md`. Pour une autre application, les risques seraient différents : paiement, données de santé, comptes, serveur, etc. Il n'existe pas une checklist de sécurité identique pour tous les produits.

## 3. Préparer l'environnement et le dépôt

Choisir un langage et une interface adaptés au besoin, puis noter les prérequis. DiskMaster utilise C#, WPF et .NET 10 sur Windows.

Configurer Git, un `.gitignore`, un format de fichiers, la version du SDK et une compilation en une commande. Faire des commits petits et lisibles. Garder les secrets et données personnelles hors du dépôt ; ajouter un fichier de configuration d'exemple si nécessaire.

**Livrables :** `global.json`, `.editorconfig`, solution, `Directory.Build.props`, fichiers de verrouillage et `CONTRIBUTING.md`. Le dossier de travail est maintenant relié à `DjibrilKounta/DiskMaster` ; le propriétaire choisit ses règles d'accès et ses protections de branche.

## 4. Concevoir les parcours et les états

Dessiner le parcours principal avant les détails graphiques : analyse → examen → sélection → confirmation → bilan. Prévoir les états vide, occupé, partiel, annulé et en erreur.

Rendre les conséquences compréhensibles. Le bouton de suppression ne doit pas être disponible avant analyse ; la confirmation doit préciser ce qu'il va supprimer. Vérifier clavier, lisibilité, zoom, contraste et lecteurs d'écran.

**Livrables :** `USER-GUIDE.md`, critères produit et matrice manuelle. Une capture jolie ne prouve pas qu'un flux complet fonctionne.

## 5. Séparer les responsabilités

Commencer avec une structure simple et claire. Pour WPF, MVVM permet de séparer l'affichage, l'état de l'écran et les règles métier. Les services réalisent les opérations de fichiers ; les modèles décrivent les données.

Injecter les dépendances pour pouvoir substituer un faux moteur en test. Éviter les architectures trop larges : pas de serveur, de microservices ni de Repository si le besoin ne le justifie pas.

**Livrables :** `ARCHITECTURE.md`, `API.md`, interfaces et composition dans `App.xaml.cs`.

## 6. Implémenter un parcours complet

Construire une fonction de bout en bout plutôt que toutes les classes vides. Pour DiskMaster : afficher une catégorie, analyser des fichiers fictifs, présenter le résultat, puis seulement traiter la suppression contrôlée.

Prévoir les erreurs dès l'implémentation : fichier disparu, accès refusé, annulation, sauvegarde impossible. L'utilisateur doit pouvoir comprendre le résultat sans lire une pile technique. Le diagnostic local doit permettre au développeur d'enquêter.

**Livrable :** comportement démontrable, avec résultat partiel honnête. Ne pas annoncer « succès » avant d'avoir terminé les effets de bord.

## 7. Tester selon les risques

Tester les règles critiques sur des fixtures isolées. Ajouter les tests de coordination du ViewModel, puis un essai de chargement WPF pour détecter les erreurs de ressources et bindings.

Les tests automatiques ne remplacent pas les machines virtuelles, les parcours au clavier, les différents DPI ni les essais d'installation. Un pourcentage de couverture n'est pas une preuve d'absence de défaut ; tester les cas qui changent la décision d'autoriser ou refuser une opération.

**Livrables :** `scripts/Test.ps1`, tests, `TEST-PLAN.md` et résultats datés. Une case non exécutée reste non exécutée.

## 8. Automatiser la validation

La CI restaure les versions prévues, compile et lance les tests à chaque changement. Elle doit échouer quand un contrôle échoue. Les actions et dépendances externes doivent être mises à jour et revues.

DiskMaster fournit un workflow Windows avec permissions de lecture. Il fabrique une archive alpha non signée comme artefact de CI ; il ne publie pas de release publique. Son exécution réelle nécessite que le dépôt soit hébergé sur GitHub avec Actions activé.

**Livrable :** `.github/workflows/ci.yml`, exécution réussie sur le dépôt et protection de branche configurée par son propriétaire. La présence du YAML ne prouve pas qu'une exécution distante a eu lieu.

## 9. Documenter les usages, droits et données

Préparer un guide utilisateur, une description d'architecture, une procédure de contribution, un changelog et un canal de support. Décrire précisément ce que le logiciel lit, stocke et transmet.

Vérifier les droits sur le code, les images, polices, bibliothèques et runtime distribué. Conserver les notices nécessaires. Les exigences particulières de contrats, commerce, accessibilité ou protection des données dépendent du produit et de son contexte ; les documents techniques ne les certifient pas.

**Livrables :** `README.md`, `LICENSE`, `PRIVACY.md`, `THIRD-PARTY-NOTICES.md`, `SECURITY.md`, `SUPPORT.md`.

## 10. Fabriquer un paquet traçable

La compilation de développement et la distribution ne sont pas la même chose. Une publication autonome embarque le runtime ; une publication dépendante demande au poste de l'avoir déjà installé. WPF ne doit pas être traité comme un simple exécutable à copier seul.

Créer un ZIP ou un installateur, enregistrer version, SDK, commit et date dans un manifeste, puis calculer les empreintes des fichiers distribués. Qualifier exactement ces fichiers : reconstruire ensuite crée d'autres fichiers à vérifier.

**Livrables :** `scripts/Build-Release.ps1`, `installer/DiskMaster.iss`, manifeste et `SHA256SUMS.txt`. Le script ne publie rien à distance.

## 11. Signer, qualifier et décider de publier

Une signature Authenticode identifie l'éditeur quand sa chaîne de confiance est reconnue. Une empreinte seule n'identifie pas l'éditeur. Le certificat et sa clé doivent appartenir au mainteneur et être protégés, jamais ajoutés au dépôt.

Tester installation, mise à niveau, désinstallation, permissions, chemins atypiques, panne et annulation. Classer les défauts, décider lesquels bloquent la livraison et faire relire le moteur critique. Pour DiskMaster, la résistance à un attaquant local concurrent n'est pas garantie par les contrôles actuels.

**Livrables :** `RELEASING.md`, `RELEASE-CHECKLIST.md`, rapport de qualification et décision alpha/bêta/stable. Une version stable ne se déduit pas d'un numéro de version.

## 12. Maintenir après la publication

Lire les retours, reproduire sur un jeu de données fictif, corriger avec un test de non-régression et publier une nouvelle version. Suivre les correctifs du SDK et du runtime : un runtime embarqué doit être republié pour bénéficier de ses correctifs.

Prévoir comment retirer une release défectueuse, avertir les utilisateurs et revenir à une version antérieure compatible. Garder les sources, les paquets et les preuves de qualification associés à chaque version. Une archive du code non testée comme restauration n'est pas une stratégie de reprise complète.

**Livrables :** politique de mises à jour, procédure d'incident, backlog priorisé et versions archivées. Voir `MAINTENANCE.md`.

## Quand une étape est-elle terminée ?

Pour chaque fonctionnalité : besoin explicite, comportement implémenté, risques traités, tests appropriés, documentation ajustée, revue et validation du paquet concerné. À l'échelle du produit, il faut aussi un responsable capable de suivre les incidents et de livrer les corrections.

Pour ton apprentissage, suis ce trajet dans le code : modèle → règle de service → test → ViewModel → binding → test de parcours → documentation → paquet. C'est une boucle de travail que tu pourras réutiliser sur tes prochains projets.

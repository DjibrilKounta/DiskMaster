# Utiliser DiskMaster

## Installer ou utiliser la version portable

Le paquet ZIP autonome x64 se décompresse dans un dossier choisi par l'utilisateur. Conserver son contenu ensemble et lancer `DiskMaster.exe` ; il n'est pas nécessaire d'installer le runtime .NET séparément pour ce paquet.

L'installateur, lorsqu'il a été compilé et qualifié, installe pour l'utilisateur courant dans `%LOCALAPPDATA%\Programs\DiskMaster`, ajoute un raccourci et une entrée de désinstallation. Il ne demande pas d'élévation. Un exécutable lancé depuis un processus déjà élevé peut néanmoins hériter de ses droits.

Le programme vise Windows x64. La prise en charge effective des versions Windows et configurations d'affichage doit être établie par la matrice de qualification de la release ; ne pas confondre un système autorisé par l'installateur et un système testé.

## Nettoyer

La version et son statut alpha apparaissent en haut de la fenêtre. Le manifeste du paquet fournit aussi la version du SDK et la date de fabrication.

1. Fermer les applications dont on souhaite nettoyer les caches.
2. Choisir **Analyser** et attendre la fin. L'analyse ne supprime rien.
3. Sélectionner une ligne pour lire sa description et consulter les 200 premiers fichiers candidats. Le total couvre tous les fichiers analysés, pas seulement l'aperçu.
4. Cocher ou décocher les catégories. La sélection s'applique aux catégories entières, pas aux fichiers individuellement.
5. Choisir **Nettoyer la sélection**, vérifier le nombre, les catégories et la taille estimée, puis accepter seulement si l'on souhaite une suppression définitive.
6. Lire le bilan. Relancer une analyse pour obtenir une nouvelle estimation.

## Catégories

| Catégorie | Emplacement | Conséquence |
| --- | --- | --- |
| Temporaires | `%LOCALAPPDATA%\Temp` | Certains fichiers peuvent servir à des applications ; l'âge seul n'est pas une preuve d'inutilité |
| Shaders DirectX | `%LOCALAPPDATA%\D3DSCache` | Le cache peut être recréé, avec un coût temporaire de chargement |
| Plantages | `%LOCALAPPDATA%\CrashDumps`, `.dmp` uniquement | Les rapports supprimés ne seront plus disponibles pour diagnostiquer les plantages |

Tous doivent avoir une dernière modification datant de plus de sept jours. Les fichiers système, en lecture seule et les liens sont exclus. Les fichiers modifiés après analyse sont ignorés.

## Comprendre les résultats

- **À analyser** : pas d'instantané utilisable ; la taille n'est pas connue.
- **Dossier absent** : cette catégorie n'est pas présente dans le profil.
- **Analyse partielle** : des éléments n'ont pas pu être lus ou un chemin a été exclu.
- **Ignoré au nettoyage** : fichier verrouillé, modifié, disparu ou ne respectant plus une règle.
- **Octets supprimés** : somme des tailles logiques des fichiers supprimés, et non différence mesurée de l'espace libre du disque.

La barre de progression compte les catégories pendant l'analyse, les fichiers pendant le nettoyage. Elle ne mesure pas le temps restant. Une catégorie volumineuse peut donc prendre du temps sans que le pourcentage évolue.

## Annulation et fermeture

Annuler demande l'arrêt dès que possible. Les fichiers déjà supprimés ne sont pas restaurés. Attendre la fin de l'opération avant de fermer ; la fermeture est bloquée pendant le travail.

## Désinstallation et mises à jour

Pour le ZIP, fermer le programme puis retirer son dossier décompressé. Pour l'installateur, utiliser la désinstallation Windows. Dans les deux cas, l'historique reste dans `%LOCALAPPDATA%\DiskMaster` jusqu'à suppression volontaire.

Il n'y a pas de mise à jour automatique. Télécharger une nouvelle version depuis la source officielle du projet, vérifier son identité et ses empreintes puis remplacer la version après fermeture. Pour un installateur, garder le même AppId afin de reconnaître les versions installées.

## Limites

La suppression est définitive et les contrôles ne garantissent pas la résistance à un programme malveillant local modifiant simultanément les chemins. Les nettoyages Windows Update, Defender, pilotes, langues et corbeille sont délégués aux outils Windows. Une alpha se teste d'abord dans un environnement de test adapté.

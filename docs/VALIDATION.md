# Rapport de validation locale

Date de travail : 28 octobre 2026. Version candidate : `0.2.0-alpha.1`.

## Vérifications automatiques

- Compilation Release avec analyseurs, nullable et avertissements traités en erreurs : réussie, zéro avertissement et zéro erreur.
- 29 tests exécutés avec succès : moteur, ViewModel, commandes, stockage et chargement/bindings WPF avec faux services. Les cas incluent les ACL refusant un sous-dossier et l'annulation après 25 suppressions sur 30 fichiers fictifs.
- 19 documents : liens Markdown locaux vérifiés ; syntaxe des scripts PowerShell contrôlée.
- Les suppressions de test visent uniquement les fixtures créées dans les sorties de tests.
- Le test WPF n'effectue aucun nettoyage réel et ne lance pas les services du profil.

## Fabrication

- Scripts de tests, publication autonome, manifeste, SHA256 et vérification de ZIP ajoutés.
- Inno Setup 6.7.4 officiel téléchargé depuis la release GitHub référencée par l'éditeur ; signature Authenticode valide, éditeur Pyrsys B.V., vérifiée avant exécution. Compilateur préparé dans `artifacts/tools/InnoSetup`.
- Publication autonome Windows x64 et compilation Inno de l'installateur : réussies.
- Archive, fichiers requis, architecture PE x64 et empreintes SHA256 : contrôlés par le script. Les licences des packages exacts Core et Desktop sont désormais copiées explicitement et exigées dans l'archive.
- Installation puis désinstallation silencieuses : réussies sur ce poste dans une fixture du projet, avec conservation du témoin extérieur. Premier rapport : `artifacts/installer-tests/241dbfea728941a4ac95a8fa5dbdb1de`. Aucun nettoyage de fichiers utilisateur n'est lancé par ce test.
- Signature de DiskMaster : non effectuée, aucun certificat d'éditeur fourni.

## Essais visuels déjà réalisés pendant la refonte

La fenêtre réelle, l'analyse du profil, l'aperçu et le dialogue de confirmation ont été examinés. Le dialogue de nettoyage a été refusé ; aucun nettoyage réel n'a été effectué dans cet essai. Ces observations ne remplacent pas la qualification du paquet final.

## Non vérifié ici

CI distante GitHub, certificat réel et signatures application/Setup/désinstalleur, installation/mise à niveau/désinstallation en VM, démarrage sur poste sans runtime, matrice Windows/DPI/lecteur d'écran, audit indépendant et résistance aux courses malveillantes. Le succès d'installation local n'est pas présenté comme une qualification multi-machines.

Les paquets décrits ci-dessus ont été fabriqués avant la configuration Git ; leur manifeste indique donc un commit absent. Le dossier de travail est désormais relié à `https://github.com/DjibrilKounta/DiskMaster.git`, sur `main`. Les sources transférées doivent être committées puis poussées par le mainteneur. La prochaine fabrication depuis un dépôt propre enregistrera le commit correspondant. Aucun tag ni release publique n'a été créé par l'assistant.

## Décision

Les contrôles présents soutiennent une **candidate alpha non signée**, pas une certification de version stable. Le mainteneur doit compléter `RELEASE-CHECKLIST.md` et qualifier les paquets finaux avant de décider de les distribuer.

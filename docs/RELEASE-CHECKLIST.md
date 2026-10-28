# Décision de livraison

Version candidate : `0.2.0-alpha.1`. Une case doit correspondre à une preuve, pas à une intention. Noter le nom et le SHA256 des paquets qualifiés dans le rapport.

## Code et comportement

- [ ] Besoin et exclusions relus ; aucun comportement annoncé absent.
- [ ] Revue du moteur par une autre personne et décision sur les risques résiduels.
- [ ] Risques de modification concurrente traités au niveau retenu ou limites explicitement acceptées pour l'alpha.
- [ ] Tests automatiques réussis sur les sources exactes de la candidate.
- [ ] Parcours manuel analyse → sélection → confirmation refusée puis acceptée sur fixture.
- [ ] Annulation et bilan partiel vérifiés pendant une suppression longue sur fixture.
- [ ] Aucun défaut connu de suppression hors périmètre ou de bilan trompeur.

## Livraison

- [ ] Version, tag, sources, changelog et manifeste cohérents.
- [ ] Dépôt propre, ou archive source exacte conservée si le projet n'est pas sous Git.
- [ ] ZIP autonome vérifié avec `Verify-Package.ps1`.
- [ ] Installation, mise à niveau, désinstallation et réinstallation vérifiées en VM.
- [ ] Démarrage sans SDK ni runtime préinstallé vérifié.
- [ ] Paquet signé avec le certificat de l'éditeur et signatures application/Setup/désinstalleur vérifiées ; ou mention explicite « alpha non signée ».
- [ ] Empreintes recalculées après toute signature ou modification.
- [ ] Notices de licences du runtime et licence du projet conservées.
- [ ] Sources des ressources graphiques et droits confirmés.

## Utilisateur et exploitation

- [ ] Matrice Windows, DPI, clavier et lecteur d'écran exécutée pour le périmètre annoncé.
- [ ] Documentation utilisateur, confidentialité, limitations et support relus.
- [ ] Dépôt officiel, permissions, protections de branche et CI configurés puis réellement vérifiés.
- [ ] Canal privé de signalement de vulnérabilités activé et testé.
- [ ] Procédure de retrait d'une version et responsable de maintenance identifiés.
- [ ] Sources et paquets archivés avec rapport de qualification.

## Décision à renseigner par le mainteneur

- Canal : alpha / bêta / stable.
- Date et responsable : …
- Paquets et empreintes : …
- Preuves et défauts restants : …
- Risques acceptés et raison : …
- Décision : publier / différer / retirer.

Les résultats locaux déjà obtenus sont dans `VALIDATION.md`. Cette checklist reste volontairement ouverte tant que la décision finale du mainteneur et les essais externes n'ont pas eu lieu.

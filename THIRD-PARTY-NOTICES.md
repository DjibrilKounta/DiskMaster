# Composants tiers

Le code DiskMaster est distribué sous la licence MIT présente dans `LICENSE`, avec l'attribution indiquée dans ce fichier. Conserver également les attributions des contributions et composants réutilisés.

La publication autonome inclut des composants .NET et Windows Desktop de Microsoft. Le script de fabrication copie leurs licences et notices depuis les packages exacts utilisés dans `licenses/microsoft.netcore.app.runtime.win-x64` et `licenses/microsoft.windowsdesktop.app.runtime.win-x64`. Ne pas retirer ces dossiers du paquet. La licence `LICENSE` de DiskMaster est distincte de celles des runtimes.

Le SDK .NET, le SDK Windows, Inno Setup et GitHub Actions sont des outils de fabrication ; leurs droits et conditions sont ceux de leurs éditeurs. Vérifier ces conditions avant un usage commercial ou une redistribution de l'outillage. Le paquet utilisateur ne doit pas inclure le compilateur Inno Setup ni les outils de test.

Les dépendances directes de l'application ont été retirées : l'interface utilise WPF. Cette absence de package NuGet direct ne signifie pas absence de composants tiers, puisque le runtime autonome est embarqué.

Les images historiques `hard-drive.png` et `clean.png` ont été retirées des ressources et de la publication. Elles sont remplacées par une icône géométrique originale du projet dans `Assets`, générée par `scripts/Generate-Icon.ps1` et distribuée sous la licence MIT du projet. Les fichiers historiques conservés dans les sources ne font pas partie du paquet utilisateur ; leurs droits doivent être vérifiés avant tout autre usage.

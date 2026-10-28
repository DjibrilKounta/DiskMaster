# Fabriquer et livrer une version

## Préparer les sources

1. Terminer la revue, les tests et le changelog.
2. Régler `Version` dans le projet, en gardant `AssemblyVersion` et `FileVersion` numériques. L'alpha courante est `0.2.0-alpha.1`.
3. Si Git est présent, committer les changements. Le script refuse un dépôt sale pour éviter d'associer un paquet à un commit qui ne contient pas ses sources. Sans `.git`, le manifeste indique un commit absent : archiver les sources exactes à part.
4. Préparer une machine Windows avec SDK .NET 10 ; pour l'installateur, Inno Setup 6.4+ et son `ISCC.exe` ; pour signer, un certificat de signature de code et SignTool du SDK Windows.

## Valider

```powershell
powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Test.ps1
```

Cette option ne modifie pas la stratégie permanente. En environnement géré, respecter la stratégie imposée et employer une session autorisée.

## Archive autonome

```powershell
powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Build-Release.ps1
```

Le script relance les tests, publie pour `win-x64` avec runtime embarqué, ajoute les notices et les guides, produit le manifeste, le ZIP et `SHA256SUMS.txt`, puis vérifie le contenu et les empreintes. Le trimming et le single-file ne sont pas activés. Les sorties sont isolées dans un nouveau dossier `artifacts/releases/<version>/<fabrication>` : aucune ancienne sortie n'est effacée ni mélangée.

## Installateur

```powershell
powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Build-Release.ps1 `
    -WithInstaller -IsccPath 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
```

Si le compilateur est ailleurs, adapter uniquement son chemin. Le script ne le télécharge ni ne l'installe automatiquement. L'AppId est stable entre versions. L'installation est par utilisateur, sans demande d'élévation ; les données du profil ne sont pas effacées à la désinstallation.

## Signature avec le certificat de l'éditeur

```powershell
powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Build-Release.ps1 `
    -WithInstaller -IsccPath 'CHEMIN\ISCC.exe' `
    -CertificateThumbprint 'EMPREINTE_HEXADECIMALE_DE_40_CARACTERES' `
    -SignToolPath 'CHEMIN\signtool.exe'
```

L'empreinte identifie un certificat déjà accessible à SignTool dans le magasin utilisateur ; elle n'est pas une clé privée. Aucun mot de passe ni PFX ne doit être committé. Le script signe et vérifie l'application, configure Inno pour signer l'installateur et le désinstalleur, puis vérifie l'installateur. Il doit échouer si la signature échoue. Le mode certificat matériel ou service de signature distant exige un adaptateur adapté au fournisseur et sa propre qualification.

Sans certificat, le paquet et l'installateur sont **non signés** et le manifeste l'indique. Ne pas substituer un certificat auto-signé à une identité d'éditeur reconnue pour annoncer une signature publique de confiance.

## Vérifier une sortie

```powershell
powershell -NoProfile -ExecutionPolicy RemoteSigned -File scripts/Verify-Package.ps1 `
    -PackageDirectory 'CHEMIN\artifacts\releases\VERSION\FABRICATION'
```

Le vérificateur contrôle la liste d'empreintes, les fichiers requis, l'architecture PE x64 de l'exécutable, la présence du runtime et les chemins du ZIP. Ce contrôle n'exécute pas le programme, ne qualifie pas l'installateur et ne remplace pas la vérification Authenticode de l'éditeur.

Un essai local d'installation puis désinstallation est possible avec `scripts/Test-Installer.ps1 -InstallerPath 'CHEMIN\Setup.exe'`. Il refuse une installation DiskMaster existante, installe dans une fixture du projet, conserve les journaux et vérifie un témoin extérieur. Ce test modifie temporairement l'enregistrement de l'application dans le profil Windows : l'exécuter sur un poste de test, puis poursuivre les essais en VM propre. Il ne lance aucun nettoyage.

## Publier

Compléter `RELEASE-CHECKLIST.md` sur ces fichiers précis. Sur son dépôt GitHub, créer un tag correspondant aux sources et une release avec le changelog, les limites, le ZIP ou Setup et les empreintes. L'alpha doit rester explicitement marquée comme préversion. Le workflow fourni fabrique uniquement des artefacts de CI ; il ne crée pas de release publique.

Ne pas annoncer Windows 10, ARM64 ou une configuration non testée simplement parce que le paquet peut s'y lancer. Ne pas prétendre que la signature supprime tous les avertissements de réputation Windows.

Conserver les sources exactes, manifestes, empreintes, signatures, rapport de test et décision de livraison. Voir `MAINTENANCE.md` pour les mises à jour et incidents.

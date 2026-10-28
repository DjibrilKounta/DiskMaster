# Références officielles

Sources consultées pour les choix de fabrication et de livraison :

- [Publication d'applications .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/) : déploiement autonome ou dépendant du framework.
- [Publication single-file .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/) : mode distinct, non utilisé dans le paquet actuel.
- [Privilèges Inno Setup](https://jrsoftware.org/ishelp/topic_setup_privilegesrequired.htm) : installation utilisateur avec `lowest`.
- [Désinstalleur signé Inno Setup](https://jrsoftware.org/ishelp/topic_setup_signeduninstaller.htm) et [SignTool Inno Setup](https://jrsoftware.org/ishelp/topic_setup_signtool.htm) : signature durant la compilation.
- [Téléchargement Inno Setup](https://jrsoftware.org/isdl.php) et [vérification](https://jrsoftware.org/isdl-verify.php) : provenance officielle et identité de l'éditeur de l'outil.
- [SignTool Windows](https://learn.microsoft.com/en-us/windows/win32/seccrypto/signtool) : signer et vérifier les exécutables.
- [Durcissement de GitHub Actions](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions) : permissions et dépendances d'actions.

Les versions d'outillage évoluent : ces liens sont préférables à une copie figée de leurs instructions. La présence d'une référence ne signifie pas que toutes les configurations décrites ont été testées localement.

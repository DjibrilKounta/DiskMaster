using System.IO;
using DiskMaster.Models;

namespace DiskMaster.Services;

public static class CleanupCatalog
{
    public static IReadOnlyList<CleanupCategory> Create()
    {
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local) || !Path.IsPathFullyQualified(local))
            throw new InvalidOperationException("Le dossier local du profil Windows n’est pas disponible.");
        return
        [
            new(CleanupKind.TemporaryFiles, "Fichiers temporaires", 
                "Fichiers temporaires de votre profil non modifiés depuis au moins 7 jours.",
                Path.Combine(local, "Temp")),
            new(CleanupKind.DirectXCache, "Cache des shaders DirectX",
                "Cache D3DSCache de votre profil. Les applications peuvent le recréer ; leur prochain démarrage peut être plus lent.",
                Path.Combine(local, "D3DSCache")),
            new(CleanupKind.CrashDumps, "Rapports de plantage",
                "Fichiers .dmp du profil, utiles au diagnostic des applications. Leur suppression est définitive.",
                Path.Combine(local, "CrashDumps"))
        ];
    }
}

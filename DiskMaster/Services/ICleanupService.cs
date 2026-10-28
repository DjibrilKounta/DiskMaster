using DiskMaster.Models;

namespace DiskMaster.Services;

/// <summary>Analyse les catégories autorisées et nettoie uniquement leurs candidats vérifiés.</summary>
/// <remarks>Les appels de l'application sont séquentiels. Voir docs/API.md pour les limites.</remarks>
public interface ICleanupService
{
    /// <summary>Catalogue fermé des racines proposées à l'utilisateur.</summary>
    IReadOnlyList<CleanupCategory> Categories { get; }
    /// <summary>Analyse sans suppression. L'annulation lève OperationCanceledException.</summary>
    Task<IReadOnlyList<ScanResult>> ScanAsync(IProgress<CleanupProgress> progress, CancellationToken token);
    /// <summary>Nettoie un instantané sélectionné ; retourne un bilan partiel si annulé.</summary>
    Task<CleanupResult> CleanAsync(IReadOnlyList<ScanResult> selection,
        IProgress<CleanupProgress> progress, CancellationToken token);
}

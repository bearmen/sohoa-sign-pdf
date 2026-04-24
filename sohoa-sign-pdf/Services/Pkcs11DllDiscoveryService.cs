using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.Factories;

namespace sohoa_sign_pdf.Services;

public sealed class Pkcs11DllDiscoveryService
{
    private static readonly string[] FilePatterns =
    [
        "*pkcs11*.dll",
        "*etpkcs11*.dll",
        "*aetpkss1*.dll",
        "*eps2003*.dll",
        "*softhsm2*.dll",
        "*token*.dll",
        "*vnpt*.dll",
        "*viettel*.dll",
        "*fpt*.dll"
    ];

    public async Task<IReadOnlyList<Pkcs11DllCandidate>> DiscoverAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => DiscoverInternal(progress, cancellationToken), cancellationToken);
    }

    private IReadOnlyList<Pkcs11DllCandidate> DiscoverInternal(IProgress<string>? progress, CancellationToken cancellationToken)
    {
        var roots = GetKnownRoots().ToList();
        progress?.Report($"Quét {roots.Count} th? m?c g?c ?? tìm PKCS#11 DLL...");
        var discovered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            progress?.Report($"?ang quét: {root}");
            foreach (var pattern in FilePatterns)
            {
                foreach (var file in SafeEnumerateFiles(root, pattern))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    discovered.Add(file);
                    if (discovered.Count % 20 == 0)
                    {
                        progress?.Report($"?ã tìm th?y {discovered.Count} t?p ?ng viên...");
                    }

                    if (discovered.Count >= 200)
                    {
                        break;
                    }
                }

                if (discovered.Count >= 200)
                {
                    break;
                }
            }

            if (discovered.Count >= 200)
            {
                progress?.Report("??t gi?i h?n 200 ?ng viên, d?ng quét ?? ??m b?o t?c ??.");
                break;
            }
        }

        progress?.Report($"B?t ??u ki?m tra kh? n?ng load c?a {discovered.Count} ?ng viên...");
        var candidates = discovered
            .Select(ProbeCandidate)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.FilePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        progress?.Report($"Hoàn t?t ki?m tra. Có {candidates.Count(x => x.CanLoad)} DLL load ???c.");
        return candidates;
    }

    private static Pkcs11DllCandidate ProbeCandidate(string path)
    {
        var candidate = new Pkcs11DllCandidate
        {
            FilePath = path,
            FileName = Path.GetFileName(path)
        };

        var score = 0;
        if (candidate.FileName.Contains("pkcs11", StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
        }

        try
        {
            var factories = new Pkcs11InteropFactories();
            using var library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, path, AppType.MultiThreaded);
            candidate.CanLoad = true;
            score += 60;

            var withToken = library.GetSlotList(SlotsType.WithTokenPresent);
            candidate.HasTokenPresent = withToken.Count > 0;
            if (candidate.HasTokenPresent)
            {
                score += 100;
            }

            candidate.Details = candidate.HasTokenPresent
                ? $"Load OK, phát hi?n {withToken.Count} slot có token."
                : "Load OK, ch?a th?y token ?ang c?m.";
        }
        catch (Exception ex)
        {
            candidate.CanLoad = false;
            candidate.Details = $"Không load ???c: {ex.Message}";
            score -= 20;
        }

        candidate.Score = score;
        return candidate;
    }

    private static IEnumerable<string> GetKnownRoots()
    {
        var roots = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        return roots
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SafeEnumerateFiles(string root, string pattern)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            ReturnSpecialDirectories = false,
            MatchCasing = MatchCasing.CaseInsensitive
        };

        try
        {
            return Directory.EnumerateFiles(root, pattern, options);
        }
        catch
        {
            return [];
        }
    }
}

public sealed class Pkcs11DllCandidate
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool CanLoad { get; set; }
    public bool HasTokenPresent { get; set; }
    public int Score { get; set; }
    public string Details { get; set; } = string.Empty;
}

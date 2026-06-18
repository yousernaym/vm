using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.RegularExpressions;

// MUST run before any MSBuild types are touched.
RegisterMSBuild();

return await Run(args);

static void RegisterMSBuild()
{
    var instances = MSBuildLocator.QueryVisualStudioInstances().ToList();
    if (instances.Count > 0)
    {
        var best = instances.OrderByDescending(i => i.Version).First();
        Console.WriteLine($"Registering MSBuild: {best.Name} {best.Version}");
        MSBuildLocator.RegisterInstance(best);
        return;
    }
    string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? @"C:\Program Files\dotnet";
    string sdkRoot = Path.Combine(dotnetRoot, "sdk");
    var sdk = Directory.Exists(sdkRoot)
        ? Directory.GetDirectories(sdkRoot)
            .Where(d => File.Exists(Path.Combine(d, "MSBuild.dll")))
            .OrderByDescending(d => d).FirstOrDefault()
        : null;
    if (sdk == null) throw new InvalidOperationException($"No MSBuild found under {sdkRoot}.");
    Console.WriteLine($"Registering MSBuild SDK path: {sdk}");
    MSBuildLocator.RegisterMSBuildPath(sdk);
}

static async Task<int> Run(string[] args)
{
    string? openCsproj = Util.ArgValue(args, "--open");
    if (openCsproj == null)
        throw new ArgumentException("Specify the project to open with --open <path.csproj>.");
    openCsproj = Path.GetFullPath(openCsproj);

    // declarations under these roots get renamed; documents under rewrite-roots get updated.
    var renameRoots = Util.ArgValues(args, "--rename-root").Select(Path.GetFullPath).ToList();
    var rewriteRoots = Util.ArgValues(args, "--rewrite-root").Select(Path.GetFullPath).ToList();
    // default: both = the opened project's directory
    string openDir = Path.GetDirectoryName(openCsproj)!;
    if (renameRoots.Count == 0) renameRoots.Add(openDir);
    if (rewriteRoots.Count == 0) rewriteRoots.Add(openDir);
    bool dryRun = args.Contains("--dry-run");

    Console.WriteLine($"Opening: {openCsproj}");
    Console.WriteLine($"Rename declarations under:  {string.Join(", ", renameRoots)}");
    Console.WriteLine($"Rewrite documents under:    {string.Join(", ", rewriteRoots)}");

    using var ws = MSBuildWorkspace.Create();
    ws.WorkspaceFailed += (_, e) =>
    {
        if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure &&
            !e.Diagnostic.Message.Contains("vulnerability"))
            Console.WriteLine($"  [workspace] {e.Diagnostic.Message}");
    };

    await ws.OpenProjectAsync(openCsproj);
    var solution = ws.CurrentSolution;

    // every document under a rewrite-root (across all loaded projects), excluding generated obj.
    var docs = solution.Projects
        .SelectMany(p => p.Documents)
        .Where(d => d.FilePath != null && Util.IsUnderAny(d.FilePath, rewriteRoots) && !Util.IsObj(d.FilePath))
        .ToList();
    Console.WriteLine($"Loaded {solution.Projects.Count()} projects; {docs.Count} documents in scope.\n");

    var renameDecider = new RenameDecider(renameRoots);
    int filesChanged = 0, totalChanges = 0;
    var sampleMethods = new SortedSet<string>();
    var sampleFields = new SortedSet<string>();

    foreach (var doc in docs)
    {
        var root = await doc.GetSyntaxRootAsync();
        var model = await doc.GetSemanticModelAsync();
        if (root == null || model == null) continue;

        var tokenReplacements = new Dictionary<TextSpan, string>();
        var entryPoints = new Dictionary<TextSpan, string>();

        void Record(SyntaxToken idToken, ISymbol? sym)
        {
            if (sym == null) return;
            if (renameDecider.TryGetNewName(sym, out string? newName) && idToken.Text != newName)
            {
                tokenReplacements[idToken.Span] = newName!;
                if (sym is IMethodSymbol) sampleMethods.Add($"{sym.ContainingType?.Name}.{sym.Name} → {newName}");
                else if (sym is IFieldSymbol) sampleFields.Add($"{sym.ContainingType?.Name}.{sym.Name} → {newName}");
            }
        }

        // declarations
        foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var sym = model.GetDeclaredSymbol(m);
            if (sym != null && renameDecider.TryGetNewName(sym, out _))
            {
                Record(m.Identifier, sym);
                if (Util.NeedsEntryPoint(m)) entryPoints[m.Identifier.Span] = m.Identifier.Text;
            }
        }
        foreach (var lf in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>())
            Record(lf.Identifier, model.GetDeclaredSymbol(lf));
        foreach (var v in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            if (v.Parent?.Parent is FieldDeclarationSyntax)
                Record(v.Identifier, model.GetDeclaredSymbol(v));

        // usages
        foreach (var id in root.DescendantNodes().OfType<IdentifierNameSyntax>())
            Record(id.Identifier, Util.ResolveUsage(model, id));
        foreach (var gn in root.DescendantNodes().OfType<GenericNameSyntax>())
            Record(gn.Identifier, Util.ResolveUsage(model, gn));

        if (tokenReplacements.Count == 0 && entryPoints.Count == 0) continue;

        if (!dryRun)
        {
            var rewriter = new SpanRewriter(tokenReplacements, entryPoints);
            var newRoot = rewriter.Visit(root);
            var text = await doc.GetTextAsync();
            var enc = text.Encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            File.WriteAllText(doc.FilePath!, newRoot.ToFullString(), enc);
        }

        Console.WriteLine($"  [{tokenReplacements.Count,4} changes] {doc.FilePath}");
        filesChanged++;
        totalChanges += tokenReplacements.Count;
    }

    if (dryRun)
    {
        Console.WriteLine("\n-- METHODS --");
        foreach (var s in sampleMethods) Console.WriteLine($"  {s}");
        Console.WriteLine("\n-- FIELDS --");
        foreach (var s in sampleFields) Console.WriteLine($"  {s}");
    }

    Console.WriteLine($"\n{(dryRun ? "[dry run] " : "")}Done. {totalChanges} substitutions across {filesChanged} files.");
    return 0;
}

// ── decide whether a symbol should be renamed, based on its declaration location ──
class RenameDecider(List<string> renameRoots)
{
    public bool TryGetNewName(ISymbol symbol, out string? newName)
    {
        newName = null;
        var def = symbol.OriginalDefinition ?? symbol;

        switch (def)
        {
            case IMethodSymbol m:
                if (m.MethodKind is not (MethodKind.Ordinary or MethodKind.LocalFunction)) return false;
                if (m.Name.Length == 0 || !char.IsLower(m.Name[0])) return false;
                if (!DeclaredInRenameRoot(def, allowDesigner: true)) return false;
                newName = Util.ToPascalCase(m.Name);
                return newName != m.Name;

            case IFieldSymbol f:
                if (f.IsConst) return false;
                var acc = f.DeclaredAccessibility;
                if (acc != Accessibility.Private && acc != Accessibility.Protected &&
                    acc != Accessibility.ProtectedAndInternal) return false;
                if (!DeclaredInRenameRoot(def, allowDesigner: false)) return false;
                newName = Util.ToFieldName(f.Name, f.IsStatic);
                return newName != f.Name;

            default:
                return false;
        }
    }

    bool DeclaredInRenameRoot(ISymbol sym, bool allowDesigner)
    {
        var loc = sym.Locations.FirstOrDefault(l => l.IsInSource);
        if (loc?.SourceTree?.FilePath is not string path) return false;
        if (!Util.IsUnderAny(path, renameRoots)) return false;
        if (Util.IsObj(path) || Util.IsThirdParty(path) || Util.IsAssemblyInfo(path)) return false;
        if (!allowDesigner && Util.IsDesigner(path)) return false;
        return true;
    }
}

static class Util
{
    public static ISymbol? ResolveUsage(SemanticModel model, ExpressionSyntax node)
    {
        var info = model.GetSymbolInfo(node);
        var sym = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
        return sym is IMethodSymbol or IFieldSymbol ? sym : null;
    }

    public static bool NeedsEntryPoint(MethodDeclarationSyntax m) =>
        m.AttributeLists.SelectMany(al => al.Attributes)
            .Any(a => a.Name.ToString().Contains("DllImport") &&
                      !(a.ArgumentList?.Arguments
                            .Any(arg => arg.NameEquals?.Name.Identifier.Text == "EntryPoint") ?? false));

    public static string ToPascalCase(string name) =>
        string.IsNullOrEmpty(name) || char.IsUpper(name[0]) ? name : char.ToUpper(name[0]) + name[1..];

    public static string ToFieldName(string name, bool isStatic)
    {
        if (!isStatic && name.StartsWith('_')) return name;
        if (isStatic && name.StartsWith("s_", StringComparison.Ordinal)) return name;
        string bare = name.TrimStart('_');
        if (bare.StartsWith("s_", StringComparison.Ordinal)) bare = bare[2..];
        if (bare.Length > 0 && char.IsUpper(bare[0])) bare = char.ToLower(bare[0]) + bare[1..];
        return isStatic ? $"s_{bare}" : $"_{bare}";
    }

    public static string? ArgValue(string[] a, string key)
    {
        int i = Array.IndexOf(a, key);
        return i >= 0 && i + 1 < a.Length ? a[i + 1] : null;
    }
    public static IEnumerable<string> ArgValues(string[] a, string key)
    {
        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == key) yield return a[i + 1];
    }
    public static bool IsUnderAny(string path, List<string> roots)
    {
        string full = Path.GetFullPath(path);
        return roots.Any(r => full.StartsWith(r, StringComparison.OrdinalIgnoreCase));
    }
    public static bool IsObj(string path) => Regex.IsMatch(path, @"[/\\]obj[/\\]");
    public static bool IsThirdParty(string path) => Regex.IsMatch(path, @"[/\\](tparty|third[ _]?party)[/\\]", RegexOptions.IgnoreCase);
    public static bool IsAssemblyInfo(string path) => path.EndsWith("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase);
    public static bool IsDesigner(string path) =>
        path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
}

// ── rewriter: replaces tokens by span, adds EntryPoint to renamed P/Invoke methods ─
class SpanRewriter(
    Dictionary<TextSpan, string> replacements,
    Dictionary<TextSpan, string> entryPoints) : CSharpSyntaxRewriter
{
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.IdentifierToken) &&
            replacements.TryGetValue(token.Span, out string? newName))
            return SyntaxFactory.Identifier(token.LeadingTrivia, newName, token.TrailingTrivia);
        return base.VisitToken(token);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var idSpan = node.Identifier.Span;
        var visited = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
        if (entryPoints.TryGetValue(idSpan, out string? origName))
            visited = AddEntryPoint(visited, origName);
        return visited;
    }

    static MethodDeclarationSyntax AddEntryPoint(MethodDeclarationSyntax method, string entryPointName)
    {
        var newAttrLists = method.AttributeLists.Select(al =>
        {
            bool changed = false;
            var newAttrs = al.Attributes.Select(attr =>
            {
                if (!attr.Name.ToString().Contains("DllImport")) return attr;
                bool has = attr.ArgumentList?.Arguments
                    .Any(a => a.NameEquals?.Name.Identifier.Text == "EntryPoint") ?? false;
                if (has) return attr;
                changed = true;
                var nameEq = SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("EntryPoint"))
                    .WithEqualsToken(SyntaxFactory.Token(SyntaxKind.EqualsToken)
                        .WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space));
                var literal = SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(entryPointName));
                var newArg = SyntaxFactory.AttributeArgument(nameEq, null, literal)
                    .WithLeadingTrivia(SyntaxFactory.Space);
                return attr.WithArgumentList(attr.ArgumentList!.AddArguments(newArg));
            }).ToList();
            return changed
                ? al.WithAttributes(SyntaxFactory.SeparatedList(newAttrs, al.Attributes.GetSeparators()))
                : al;
        }).ToList();
        return method.WithAttributeLists(SyntaxFactory.List(newAttrLists));
    }
}

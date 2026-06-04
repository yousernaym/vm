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
        Console.WriteLine($"Registering MSBuild: {best.Name} {best.Version} @ {best.MSBuildPath}");
        MSBuildLocator.RegisterInstance(best);
        return;
    }

    // Fall back to the highest .NET SDK under the dotnet install root.
    string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT")
        ?? @"C:\Program Files\dotnet";
    string sdkRoot = Path.Combine(dotnetRoot, "sdk");
    var sdk = Directory.Exists(sdkRoot)
        ? Directory.GetDirectories(sdkRoot)
            .Where(d => File.Exists(Path.Combine(d, "MSBuild.dll")))
            .OrderByDescending(d => d)
            .FirstOrDefault()
        : null;
    if (sdk == null)
        throw new InvalidOperationException($"No MSBuild found (looked under {sdkRoot}).");
    Console.WriteLine($"Registering MSBuild SDK path: {sdk}");
    MSBuildLocator.RegisterMSBuildPath(sdk);
}

static async Task<int> Run(string[] args)
{
    string csprojPath = args.FirstOrDefault(a => a.EndsWith(".csproj"))
        ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\VisualMusic\VisualMusic.csproj"));
    bool dryRun = args.Contains("--dry-run");

    string projectRoot = Path.GetDirectoryName(Path.GetFullPath(csprojPath))!;
    Console.WriteLine($"Opening project: {csprojPath}");

    using var ws = MSBuildWorkspace.Create();
    ws.WorkspaceFailed += (_, e) =>
    {
        if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            Console.WriteLine($"  [workspace] {e.Diagnostic.Message}");
    };

    var project = await ws.OpenProjectAsync(csprojPath);
    var compilation = await project.GetCompilationAsync()
        ?? throw new InvalidOperationException("Failed to get compilation.");

    // Report only severe compile errors (a few are expected from design-time XAML).
    int errCount = compilation.GetDiagnostics().Count(d => d.Severity == DiagnosticSeverity.Error);
    Console.WriteLine($"Compilation loaded ({errCount} pre-existing diagnostics-as-errors, usually XAML design-time noise)");

    // documents we will read/rewrite (everything in the project tree except generated obj/)
    var docs = project.Documents
        .Where(d => d.FilePath != null && IsUnder(d.FilePath, projectRoot) && !IsObj(d.FilePath))
        .ToList();

    // ── pass 1: build the symbol → newName rename map ──────────────────────────
    var renameMap = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);

    foreach (var doc in docs)
    {
        var root = await doc.GetSyntaxRootAsync();
        var model = compilation.GetSemanticModel(root!.SyntaxTree);

        foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (model.GetDeclaredSymbol(m) is IMethodSymbol sym && ShouldRenameMethod(sym, projectRoot))
                renameMap.TryAdd(sym, ToPascalCase(sym.Name));
        }
        foreach (var lf in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>())
        {
            if (model.GetDeclaredSymbol(lf) is IMethodSymbol sym && sym.Name.Length > 0 && char.IsLower(sym.Name[0]))
                renameMap.TryAdd(sym, ToPascalCase(sym.Name));
        }
        foreach (var v in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            if (v.Parent?.Parent is not FieldDeclarationSyntax) continue;
            if (model.GetDeclaredSymbol(v) is IFieldSymbol sym && ShouldRenameField(sym, projectRoot, out string newName))
                renameMap.TryAdd(sym, newName);
        }
    }

    int methodCount = renameMap.Keys.Count(s => s is IMethodSymbol);
    int fieldCount = renameMap.Keys.Count(s => s is IFieldSymbol);
    Console.WriteLine($"\nRename map: {methodCount} methods + {fieldCount} fields = {renameMap.Count} symbols");

    if (dryRun)
    {
        Console.WriteLine("\n-- METHODS --");
        foreach (var kv in renameMap.Where(k => k.Key is IMethodSymbol).OrderBy(k => k.Key.Name))
            Console.WriteLine($"  {kv.Key.ContainingType?.Name}.{kv.Key.Name}  →  {kv.Value}");
        Console.WriteLine("\n-- FIELDS --");
        foreach (var kv in renameMap.Where(k => k.Key is IFieldSymbol).OrderBy(k => k.Key.ContainingType?.Name + "." + k.Key.Name))
            Console.WriteLine($"  {kv.Key.ContainingType?.Name}.{kv.Key.Name}  →  {kv.Value}");
        return 0;
    }

    // ── pass 2: rewrite each document using semantic resolution ────────────────
    int filesChanged = 0, totalChanges = 0;

    foreach (var doc in docs)
    {
        var root = await doc.GetSyntaxRootAsync();
        var model = compilation.GetSemanticModel(root!.SyntaxTree);

        // token-span → new identifier text
        var tokenReplacements = new Dictionary<TextSpan, string>();
        // method-identifier-span → original name (for P/Invoke EntryPoint preservation)
        var entryPoints = new Dictionary<TextSpan, string>();

        void Record(SyntaxToken idToken, ISymbol? sym)
        {
            if (sym == null) return;
            var def = sym.OriginalDefinition ?? sym;
            if (renameMap.TryGetValue(def, out string? newName) && idToken.Text != newName)
                tokenReplacements[idToken.Span] = newName;
        }

        // declarations
        foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var sym = model.GetDeclaredSymbol(m);
            if (sym != null && renameMap.ContainsKey(sym))
            {
                Record(m.Identifier, sym);
                if (NeedsEntryPoint(m))
                    entryPoints[m.Identifier.Span] = m.Identifier.Text;
            }
        }
        foreach (var lf in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>())
            Record(lf.Identifier, model.GetDeclaredSymbol(lf));
        foreach (var v in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            if (v.Parent?.Parent is FieldDeclarationSyntax)
                Record(v.Identifier, model.GetDeclaredSymbol(v));

        // usages
        foreach (var id in root.DescendantNodes().OfType<IdentifierNameSyntax>())
            Record(id.Identifier, ResolveUsage(model, id));
        foreach (var gn in root.DescendantNodes().OfType<GenericNameSyntax>())
            Record(gn.Identifier, ResolveUsage(model, gn));

        if (tokenReplacements.Count == 0 && entryPoints.Count == 0) continue;

        var rewriter = new SpanRewriter(tokenReplacements, entryPoints);
        var newRoot = rewriter.Visit(root);

        var text = await doc.GetTextAsync();
        var enc = text.Encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        File.WriteAllText(doc.FilePath!, newRoot.ToFullString(), enc);

        Console.WriteLine($"  [{tokenReplacements.Count,4} changes] {Path.GetRelativePath(projectRoot, doc.FilePath!)}");
        filesChanged++;
        totalChanges += tokenReplacements.Count;
    }

    Console.WriteLine($"\nDone. {totalChanges} substitutions across {filesChanged} files.");
    return 0;
}

// ── symbol filters ──────────────────────────────────────────────────────────────
static bool ShouldRenameMethod(IMethodSymbol sym, string root)
{
    if (sym.MethodKind != MethodKind.Ordinary) return false;
    if (sym.Name.Length == 0 || !char.IsLower(sym.Name[0])) return false;
    return DeclaredInAppSource(sym, root, allowDesigner: true);
}

static bool ShouldRenameField(IFieldSymbol sym, string root, out string newName)
{
    newName = "";
    if (sym.IsConst) return false;
    var acc = sym.DeclaredAccessibility;
    if (acc != Accessibility.Private && acc != Accessibility.Protected &&
        acc != Accessibility.ProtectedAndInternal) return false;
    if (!DeclaredInAppSource(sym, root, allowDesigner: false)) return false;
    newName = ToFieldName(sym.Name, sym.IsStatic);
    return newName != sym.Name;
}

// Declared in our app source: under project root, not tparty, not obj, (optionally not Designer).
static bool DeclaredInAppSource(ISymbol sym, string root, bool allowDesigner)
{
    var loc = sym.Locations.FirstOrDefault(l => l.IsInSource);
    if (loc?.SourceTree?.FilePath is not string path) return false;
    if (!IsUnder(path, root)) return false;
    if (IsObj(path)) return false;
    if (Regex.IsMatch(path, @"[/\\]tparty[/\\]")) return false;
    if (!allowDesigner && IsDesigner(path)) return false;
    return true;
}

// ── usage symbol resolution ─────────────────────────────────────────────────────
static ISymbol? ResolveUsage(SemanticModel model, ExpressionSyntax node)
{
    var info = model.GetSymbolInfo(node);
    var sym = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
    return sym switch
    {
        IMethodSymbol m => m,
        IFieldSymbol f => f,
        _ => null
    };
}

// ── P/Invoke detection ───────────────────────────────────────────────────────────
static bool NeedsEntryPoint(MethodDeclarationSyntax m) =>
    m.AttributeLists.SelectMany(al => al.Attributes)
        .Any(a => a.Name.ToString().Contains("DllImport") &&
                  !(a.ArgumentList?.Arguments
                        .Any(arg => arg.NameEquals?.Name.Identifier.Text == "EntryPoint") ?? false));

// ── name transforms ──────────────────────────────────────────────────────────────
static string ToPascalCase(string name) =>
    string.IsNullOrEmpty(name) || char.IsUpper(name[0]) ? name : char.ToUpper(name[0]) + name[1..];

static string ToFieldName(string name, bool isStatic)
{
    if (!isStatic && name.StartsWith('_')) return name;
    if (isStatic && name.StartsWith("s_", StringComparison.Ordinal)) return name;
    string bare = name.TrimStart('_');
    if (bare.StartsWith("s_", StringComparison.Ordinal)) bare = bare[2..];
    if (bare.Length > 0 && char.IsUpper(bare[0])) bare = char.ToLower(bare[0]) + bare[1..];
    return isStatic ? $"s_{bare}" : $"_{bare}";
}

// ── path helpers ─────────────────────────────────────────────────────────────────
static bool IsUnder(string path, string root) =>
    Path.GetFullPath(path).StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase);
static bool IsObj(string path) => Regex.IsMatch(path, @"[/\\]obj[/\\]");
static bool IsDesigner(string path) =>
    path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) ||
    path.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);

// ── rewriter: replaces tokens by span, adds EntryPoint to P/Invoke methods ───────
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

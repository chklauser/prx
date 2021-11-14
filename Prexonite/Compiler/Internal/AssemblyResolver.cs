#nullable enable
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Prexonite.Compiler.Internal;

internal sealed class AssemblyResolver : ICloneable
{
    private readonly ConcurrentDictionary<AssemblyName, Assembly> _assemblies = new();
    private readonly ConcurrentDictionary<string, Assembly> _cache = new();

    public Assembly Resolve(string name) => TryResolve(name) ??
        throw new PrexoniteException($"Could not resolve assembly by name '{name}'. Is it loaded?");

    public Assembly? TryResolve(string name)
    {
        // as an optimization, check if there is a cache hit
        if (_cache.TryGetValue(name, out var cacheHit))
        {
            return cacheHit;
        }

        // try to resolve the assembly (note that we can't use GetOrAdd because the resolution might fail)
        Assembly? resolvedAssembly = null;
        foreach (var assembly in _assemblies.Values)
        {
            if (Engine.DefaultStringComparer.Equals(assembly.FullName, name)
                || assembly.GetName() is var otherName &&
                (Engine.DefaultStringComparer.Equals(otherName.Name, name)
                    || Engine.DefaultStringComparer.Equals($"{otherName.Name},{otherName.Version}", name)))
            {
                resolvedAssembly = assembly;
                break;
            }
        }

        return resolvedAssembly == null ? resolvedAssembly : _cache.GetOrAdd(name, resolvedAssembly);
    }

    /// <summary>
    ///     Determines whether an assembly is already registered for use by the Prexonite VM.
    /// </summary>
    /// <param name = "ass">An assembly reference.</param>
    /// <returns>True if the supplied assembly is registered; false otherwise.</returns>
    [DebuggerStepThrough]
    public bool Contains(Assembly? ass) => ass != null && _assemblies.ContainsKey(ass.GetName());

    /// <summary>
    ///     Gets a list of all registered assemblies.
    /// </summary>
    /// <returns>A copy of the list of registered assemblies.</returns>
    [DebuggerStepThrough]
    public Assembly[] ToArray() => _assemblies.Values.ToArray();

    /// <summary>
    ///     Registers a new assembly for use by the Prexonite VM.
    /// </summary>
    /// <param name = "ass">An assembly reference.</param>
    /// <exception cref = "ArgumentNullException"><paramref name = "ass" /> is null.</exception>
    [DebuggerStepThrough]
    public void Add(Assembly ass)
    {
        if (ass == null)
            throw new ArgumentNullException(nameof(ass));
        _assemblies.GetOrAdd(ass.GetName(), ass);
    }

    /// <summary>
    ///     Removes an assembly from the list registered ones.
    /// </summary>
    /// <param name = "ass">The assembly to remove.</param>
    /// <exception cref = "ArgumentNullException"><paramref name = "ass" /> is null.</exception>
    [DebuggerStepThrough]
    public void Remove(Assembly ass)
    {
        if (ass == null)
            throw new ArgumentNullException(nameof(ass));
        _assemblies.TryRemove(ass.GetName(), out _);
        foreach (var (shortName, cached) in _cache)
        {
            if (ass == cached)
            {
                _cache.TryRemove(shortName, out _);
            }
        }
    }

    public AssemblyResolver Clone()
    {
        var copy = new AssemblyResolver();
        copy._assemblies.AddRange(_assemblies);
        // NOTE: we intentionally don't copy the cache. The cache is an optimization that should remain tied to
        // a single engine. The clone should not copy over cache entries (which might never be relevant to the
        // clone).
        return copy;
    }
    
    object ICloneable.Clone() => Clone();
}
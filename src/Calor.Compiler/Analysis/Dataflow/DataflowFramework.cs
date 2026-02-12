using Calor.Compiler.Binding;

namespace Calor.Compiler.Analysis.Dataflow;

/// <summary>
/// Represents the lattice for a dataflow analysis.
/// </summary>
/// <typeparam name="T">The type of dataflow facts.</typeparam>
public interface IDataflowLattice<T> where T : IEquatable<T>
{
    /// <summary>
    /// The bottom element of the lattice (most optimistic/least information).
    /// </summary>
    T Bottom { get; }

    /// <summary>
    /// The top element of the lattice (most conservative/full information).
    /// </summary>
    T Top { get; }

    /// <summary>
    /// Joins two lattice elements (least upper bound / meet operation).
    /// For forward may-analyses, this is typically union.
    /// For backward must-analyses, this is typically intersection.
    /// </summary>
    T Join(T a, T b);

    /// <summary>
    /// Checks if a is less than or equal to b in the lattice ordering.
    /// </summary>
    bool LessOrEqual(T a, T b);
}

/// <summary>
/// Represents a transfer function that computes the effect of a statement on dataflow facts.
/// </summary>
/// <typeparam name="T">The type of dataflow facts.</typeparam>
public interface ITransferFunction<T> where T : IEquatable<T>
{
    /// <summary>
    /// Computes the dataflow facts after executing a statement, given the facts before.
    /// </summary>
    T Transfer(BoundStatement statement, T input);

    /// <summary>
    /// Computes the dataflow facts after evaluating an expression (for condition blocks).
    /// </summary>
    T TransferExpression(BoundExpression? expression, T input);
}

/// <summary>
/// Direction of the dataflow analysis.
/// </summary>
public enum DataflowDirection
{
    Forward,
    Backward
}

/// <summary>
/// Results of a dataflow analysis for a single basic block.
/// </summary>
/// <typeparam name="T">The type of dataflow facts.</typeparam>
public sealed class BlockDataflowResult<T>
{
    public T In { get; set; }
    public T Out { get; set; }

    public BlockDataflowResult(T initial)
    {
        In = initial;
        Out = initial;
    }
}

/// <summary>
/// Generic dataflow analysis framework using worklist algorithm.
/// </summary>
/// <typeparam name="T">The type of dataflow facts.</typeparam>
public sealed class DataflowAnalysis<T> where T : IEquatable<T>
{
    private readonly IDataflowLattice<T> _lattice;
    private readonly ITransferFunction<T> _transfer;
    private readonly DataflowDirection _direction;
    private readonly int _maxIterations;

    public DataflowAnalysis(
        IDataflowLattice<T> lattice,
        ITransferFunction<T> transfer,
        DataflowDirection direction = DataflowDirection.Forward,
        int maxIterations = 1000)
    {
        _lattice = lattice ?? throw new ArgumentNullException(nameof(lattice));
        _transfer = transfer ?? throw new ArgumentNullException(nameof(transfer));
        _direction = direction;
        _maxIterations = maxIterations;
    }

    /// <summary>
    /// Runs the dataflow analysis on a control flow graph.
    /// </summary>
    /// <returns>A dictionary mapping each block to its dataflow results.</returns>
    public Dictionary<BasicBlock, BlockDataflowResult<T>> Analyze(ControlFlowGraph cfg)
    {
        var results = new Dictionary<BasicBlock, BlockDataflowResult<T>>();

        // Initialize all blocks with bottom
        foreach (var block in cfg.Blocks)
        {
            results[block] = new BlockDataflowResult<T>(_lattice.Bottom);
        }

        // Entry/exit block initialization
        if (_direction == DataflowDirection.Forward)
        {
            results[cfg.Entry].In = _lattice.Top;
        }
        else
        {
            results[cfg.Exit].Out = _lattice.Top;
        }

        // Get blocks in appropriate order
        var worklist = _direction == DataflowDirection.Forward
            ? new Queue<BasicBlock>(cfg.GetReversePostOrder())
            : new Queue<BasicBlock>(cfg.GetPostOrder());

        var inWorklist = new HashSet<BasicBlock>(worklist);
        var iterations = 0;

        while (worklist.Count > 0 && iterations < _maxIterations)
        {
            iterations++;
            var block = worklist.Dequeue();
            inWorklist.Remove(block);

            var result = results[block];
            var changed = false;

            if (_direction == DataflowDirection.Forward)
            {
                // Compute IN as join of all predecessors' OUT
                var newIn = block.Predecessors.Count == 0
                    ? result.In
                    : block.Predecessors
                        .Select(p => results[p].Out)
                        .Aggregate(_lattice.Bottom, _lattice.Join);

                if (!newIn.Equals(result.In))
                {
                    result.In = newIn;
                    changed = true;
                }

                // Compute OUT using transfer function
                var currentFacts = result.In;

                // Apply transfer for branch condition (if any)
                currentFacts = _transfer.TransferExpression(block.BranchCondition, currentFacts);

                // Apply transfer for each statement
                foreach (var stmt in block.Statements)
                {
                    currentFacts = _transfer.Transfer(stmt, currentFacts);
                }

                if (!currentFacts.Equals(result.Out))
                {
                    result.Out = currentFacts;
                    changed = true;
                }

                // Add successors to worklist if changed
                if (changed)
                {
                    foreach (var succ in block.Successors)
                    {
                        if (!inWorklist.Contains(succ))
                        {
                            worklist.Enqueue(succ);
                            inWorklist.Add(succ);
                        }
                    }
                }
            }
            else // Backward
            {
                // Compute OUT as join of all successors' IN
                var newOut = block.Successors.Count == 0
                    ? result.Out
                    : block.Successors
                        .Select(s => results[s].In)
                        .Aggregate(_lattice.Bottom, _lattice.Join);

                if (!newOut.Equals(result.Out))
                {
                    result.Out = newOut;
                    changed = true;
                }

                // Compute IN using transfer function (in reverse order for backward)
                var currentFacts = result.Out;

                // Apply transfer for each statement in reverse
                for (var i = block.Statements.Count - 1; i >= 0; i--)
                {
                    currentFacts = _transfer.Transfer(block.Statements[i], currentFacts);
                }

                // Apply transfer for branch condition
                currentFacts = _transfer.TransferExpression(block.BranchCondition, currentFacts);

                if (!currentFacts.Equals(result.In))
                {
                    result.In = currentFacts;
                    changed = true;
                }

                // Add predecessors to worklist if changed
                if (changed)
                {
                    foreach (var pred in block.Predecessors)
                    {
                        if (!inWorklist.Contains(pred))
                        {
                            worklist.Enqueue(pred);
                            inWorklist.Add(pred);
                        }
                    }
                }
            }
        }

        return results;
    }
}

/// <summary>
/// A set-based lattice for dataflow facts.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public sealed class SetLattice<T> : IDataflowLattice<ImmutableHashSet<T>> where T : notnull
{
    private readonly ImmutableHashSet<T> _universe;

    public SetLattice(IEnumerable<T>? universe = null)
    {
        _universe = universe != null
            ? ImmutableHashSet<T>.CreateRange(universe)
            : ImmutableHashSet<T>.Empty;
    }

    public ImmutableHashSet<T> Bottom => ImmutableHashSet<T>.Empty;
    public ImmutableHashSet<T> Top => _universe;

    public ImmutableHashSet<T> Join(ImmutableHashSet<T> a, ImmutableHashSet<T> b)
        => a.Union(b);

    public bool LessOrEqual(ImmutableHashSet<T> a, ImmutableHashSet<T> b)
        => a.IsSubsetOf(b);
}

/// <summary>
/// An intersection-based lattice for must-analyses.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public sealed class MustSetLattice<T> : IDataflowLattice<ImmutableHashSet<T>> where T : notnull
{
    private readonly ImmutableHashSet<T> _universe;

    public MustSetLattice(IEnumerable<T> universe)
    {
        _universe = ImmutableHashSet<T>.CreateRange(universe);
    }

    public ImmutableHashSet<T> Bottom => _universe; // Start with everything
    public ImmutableHashSet<T> Top => ImmutableHashSet<T>.Empty; // End with nothing

    public ImmutableHashSet<T> Join(ImmutableHashSet<T> a, ImmutableHashSet<T> b)
        => a.Intersect(b); // Must be in both paths

    public bool LessOrEqual(ImmutableHashSet<T> a, ImmutableHashSet<T> b)
        => b.IsSubsetOf(a); // Reversed for must-analysis
}

/// <summary>
/// Represents a variable definition (assignment site).
/// </summary>
public readonly record struct Definition(string VariableName, int BlockId, int StatementIndex);

/// <summary>
/// Immutable hash set wrapper that implements IEquatable for dataflow analysis.
/// </summary>
public readonly struct ImmutableHashSet<T> : IEquatable<ImmutableHashSet<T>> where T : notnull
{
    private readonly HashSet<T>? _set;

    private ImmutableHashSet(HashSet<T>? set)
    {
        _set = set;
    }

    public static ImmutableHashSet<T> Empty => new(null);

    public static ImmutableHashSet<T> CreateRange(IEnumerable<T> items)
        => new(new HashSet<T>(items));

    public static ImmutableHashSet<T> Create(T item)
        => new(new HashSet<T> { item });

    public int Count => _set?.Count ?? 0;

    public bool Contains(T item) => _set?.Contains(item) ?? false;

    public ImmutableHashSet<T> Add(T item)
    {
        var newSet = _set != null ? new HashSet<T>(_set) : new HashSet<T>();
        newSet.Add(item);
        return new ImmutableHashSet<T>(newSet);
    }

    public ImmutableHashSet<T> Remove(T item)
    {
        if (_set == null || !_set.Contains(item))
            return this;

        var newSet = new HashSet<T>(_set);
        newSet.Remove(item);
        return new ImmutableHashSet<T>(newSet);
    }

    public ImmutableHashSet<T> Union(ImmutableHashSet<T> other)
    {
        if (_set == null)
            return other;
        if (other._set == null)
            return this;

        var newSet = new HashSet<T>(_set);
        newSet.UnionWith(other._set);
        return new ImmutableHashSet<T>(newSet);
    }

    public ImmutableHashSet<T> Intersect(ImmutableHashSet<T> other)
    {
        if (_set == null || other._set == null)
            return Empty;

        var newSet = new HashSet<T>(_set);
        newSet.IntersectWith(other._set);
        return new ImmutableHashSet<T>(newSet);
    }

    public ImmutableHashSet<T> Except(ImmutableHashSet<T> other)
    {
        if (_set == null)
            return Empty;
        if (other._set == null)
            return this;

        var newSet = new HashSet<T>(_set);
        newSet.ExceptWith(other._set);
        return new ImmutableHashSet<T>(newSet);
    }

    public bool IsSubsetOf(ImmutableHashSet<T> other)
    {
        if (_set == null)
            return true;
        if (other._set == null)
            return _set.Count == 0;

        return _set.IsSubsetOf(other._set);
    }

    public IEnumerable<T> AsEnumerable() => _set ?? Enumerable.Empty<T>();

    public bool Equals(ImmutableHashSet<T> other)
    {
        if (_set == null && other._set == null)
            return true;
        if (_set == null || other._set == null)
            return false;

        return _set.SetEquals(other._set);
    }

    public override bool Equals(object? obj) => obj is ImmutableHashSet<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (_set == null)
            return 0;

        // Order-independent hash
        var hash = 0;
        foreach (var item in _set)
            hash ^= item.GetHashCode();
        return hash;
    }

    public static bool operator ==(ImmutableHashSet<T> left, ImmutableHashSet<T> right) => left.Equals(right);
    public static bool operator !=(ImmutableHashSet<T> left, ImmutableHashSet<T> right) => !left.Equals(right);
}

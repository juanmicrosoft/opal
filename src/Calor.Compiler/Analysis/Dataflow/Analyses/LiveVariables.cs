using Calor.Compiler.Binding;

namespace Calor.Compiler.Analysis.Dataflow.Analyses;

/// <summary>
/// Live variables analysis: determines which variables may be used before being redefined.
/// This is a backward may-analysis.
/// A variable is live at a point if there exists a path from that point to a use of the variable
/// that doesn't pass through a definition of the variable.
/// </summary>
public sealed class LiveVariablesAnalysis
{
    private readonly ControlFlowGraph _cfg;
    private readonly Dictionary<BasicBlock, BlockDataflowResult<ImmutableHashSet<string>>> _results;
    private readonly HashSet<string> _allVariables;

    public LiveVariablesAnalysis(ControlFlowGraph cfg)
    {
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _allVariables = CollectAllVariables(cfg);

        var lattice = new SetLattice<string>(_allVariables);
        var transfer = new LiveVariablesTransfer();
        var analysis = new DataflowAnalysis<ImmutableHashSet<string>>(
            lattice, transfer, DataflowDirection.Backward);

        _results = analysis.Analyze(cfg);
    }

    /// <summary>
    /// Gets the variables that are live at the entry of a block.
    /// </summary>
    public IEnumerable<string> GetLiveVariablesAtEntry(BasicBlock block)
    {
        if (_results.TryGetValue(block, out var result))
            return result.In.AsEnumerable();
        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Gets the variables that are live at the exit of a block.
    /// </summary>
    public IEnumerable<string> GetLiveVariablesAtExit(BasicBlock block)
    {
        if (_results.TryGetValue(block, out var result))
            return result.Out.AsEnumerable();
        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Checks if a variable is live at a specific point.
    /// </summary>
    public bool IsLive(BasicBlock block, string variableName, bool atEntry = true)
    {
        if (_results.TryGetValue(block, out var result))
        {
            var facts = atEntry ? result.In : result.Out;
            return facts.Contains(variableName);
        }
        return false;
    }

    /// <summary>
    /// Finds dead assignments (definitions where the variable is not live after).
    /// </summary>
    public IEnumerable<(BasicBlock Block, BoundStatement Statement, string Variable)> FindDeadAssignments()
    {
        foreach (var block in _cfg.Blocks)
        {
            if (!_results.TryGetValue(block, out var result))
                continue;

            var liveAfter = result.Out;

            // Process statements in reverse order
            for (var i = block.Statements.Count - 1; i >= 0; i--)
            {
                var stmt = block.Statements[i];
                var defined = BoundNodeHelpers.GetDefinedVariable(stmt);

                if (defined != null && !liveAfter.Contains(defined.Name))
                {
                    // This definition is not live after - it's dead
                    yield return (block, stmt, defined.Name);
                }

                // Update liveAfter for the next iteration (going backwards)
                // Gen: add used variables
                foreach (var used in BoundNodeHelpers.GetUsedVariables(stmt))
                {
                    liveAfter = liveAfter.Add(used.Name);
                }

                // Kill: remove defined variables
                if (defined != null)
                {
                    liveAfter = liveAfter.Remove(defined.Name);
                }
            }
        }
    }

    private static HashSet<string> CollectAllVariables(ControlFlowGraph cfg)
    {
        var variables = new HashSet<string>();

        foreach (var block in cfg.Blocks)
        {
            foreach (var stmt in block.Statements)
            {
                var defined = BoundNodeHelpers.GetDefinedVariable(stmt);
                if (defined != null)
                    variables.Add(defined.Name);

                foreach (var used in BoundNodeHelpers.GetUsedVariables(stmt))
                    variables.Add(used.Name);
            }

            if (block.BranchCondition != null)
            {
                foreach (var used in BoundNodeHelpers.GetUsedVariables(block.BranchCondition))
                    variables.Add(used.Name);
            }
        }

        return variables;
    }
}

internal sealed class LiveVariablesTransfer : ITransferFunction<ImmutableHashSet<string>>
{
    public ImmutableHashSet<string> Transfer(BoundStatement statement, ImmutableHashSet<string> input)
    {
        var result = input;

        // Gen: add used variables (they must be live before this statement)
        foreach (var used in BoundNodeHelpers.GetUsedVariables(statement))
        {
            result = result.Add(used.Name);
        }

        // Kill: remove defined variables (they don't need to be live before this definition)
        var defined = BoundNodeHelpers.GetDefinedVariable(statement);
        if (defined != null)
        {
            result = result.Remove(defined.Name);
        }

        return result;
    }

    public ImmutableHashSet<string> TransferExpression(BoundExpression? expression, ImmutableHashSet<string> input)
    {
        if (expression == null)
            return input;

        var result = input;
        foreach (var used in BoundNodeHelpers.GetUsedVariables(expression))
        {
            result = result.Add(used.Name);
        }

        return result;
    }
}

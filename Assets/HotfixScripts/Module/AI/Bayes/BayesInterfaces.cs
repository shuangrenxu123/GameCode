using System.Collections.Generic;

namespace Bayes
{
    public interface IBayesQuery
    {
        float Probability(string targetVar, string targetValue, EvidenceSet evidence);
        Distribution Marginal(string targetVar, EvidenceSet evidence);
        string MAP(string targetVar, EvidenceSet evidence);
    }

    public interface IBayesUpdate
    {
        void DefineVariable(string name, IReadOnlyList<string> values);
        void SetPrior(string varName, Distribution prior);
        void SetConditional(string childVar, IReadOnlyList<string> parentVars, Cpt table);
        void FitFromSamples(IEnumerable<Sample> samples, float smoothing = 0f);
    }

    public interface IBayesContext
    {
        IBayesQuery Query { get; }
        IBayesUpdate Update { get; }
    }
}

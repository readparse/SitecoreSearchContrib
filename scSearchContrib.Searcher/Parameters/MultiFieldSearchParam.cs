﻿namespace scSearchContrib.Searcher.Parameters
{
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Search;

    using scSearchContrib.Searcher.Utilities;

    using Sitecore.Search;

    public class MultiFieldSearchParam : SearchParam
    {
        public struct Refinement
        {
            public string Name { get; private set; }

            public string Value { get; private set; }

            public Refinement(string name, string value) : this() { Name = name; Value = value; }
        }

        public MultiFieldSearchParam()
        {
            Refinements = new List<Refinement>();
        }

        public QueryOccurance InnerCondition { get; set; }

        public IEnumerable<Refinement> Refinements { get; set; }

        public override BooleanQuery ProcessQuery(QueryOccurance condition, Index index)
        {
            var outerQuery = new BooleanQuery();

            var refinementQuery = ApplyRefinements(Refinements, InnerCondition);

            var translator = new QueryTranslator(index);
            var refBooleanQuery = translator.ConvertCombinedQuery(refinementQuery);
            var outerCondition = translator.GetOccur(condition);

            if (refBooleanQuery != null && refBooleanQuery.Clauses().Count > 0)
            {
                outerQuery.Add(refBooleanQuery, outerCondition);
            }

            var baseQuery = base.ProcessQuery(condition, index);
            if (baseQuery != null)
            {
                outerQuery.Add(baseQuery, outerCondition);
            }

            return outerQuery;
        }

        protected CombinedQuery ApplyRefinements(IEnumerable<Refinement> refinements, QueryOccurance condition)
        {
            if (!refinements.Any())
            {
                return new CombinedQuery();
            }

            var innerQuery = new CombinedQuery();

            foreach (var refinement in refinements)
            {
                AddFieldValueClause(innerQuery, refinement.Name, refinement.Value, condition);
            }

            return innerQuery;
        }
    }
}

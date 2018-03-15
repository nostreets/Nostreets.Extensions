using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.Helpers.QueryProvider
{
    public class QueryCommand
    {
        string commandText;
        ReadOnlyCollection<QueryParameter> parameters;

        public QueryCommand(string commandText, IEnumerable<QueryParameter> parameters)
        {
            this.commandText = commandText;
            this.parameters = parameters.ToReadOnly();
        }

        public string CommandText
        {
            get { return this.commandText; }
        }

        public ReadOnlyCollection<QueryParameter> Parameters
        {
            get { return this.parameters; }
        }
    }

    public class QueryParameter
    {
        string name;
        Type type;
        QueryType queryType;

        public QueryParameter(string name, Type type, QueryType queryType)
        {
            this.name = name;
            this.type = type;
            this.queryType = queryType;
        }

        public string Name
        {
            get { return this.name; }
        }

        public Type Type
        {
            get { return this.type; }
        }

        public QueryType QueryType
        {
            get { return this.queryType; }
        }
    }


    /// <summary>
    /// Defines query execution and materialization policies. 
    /// </summary>
    public class QueryTranslator
    {
        QueryLinguist linguist;
        QueryMapper mapper;
        QueryPolice police;

        public QueryTranslator(QueryLanguage language, QueryMapping mapping, QueryPolicy policy)
        {
            this.linguist = language.CreateLinguist(this);
            this.mapper = mapping.CreateMapper(this);
            this.police = policy.CreatePolice(this);
        }

        public QueryLinguist Linguist
        {
            get { return this.linguist; }
        }

        public QueryMapper Mapper
        {
            get { return this.mapper; }
        }

        public QueryPolice Police
        {
            get { return this.police; }
        }

        public virtual Expression Translate(Expression expression)
        {
            // pre-evaluate local sub-trees
            expression = PartialEvaluator.Eval(expression, this.mapper.Mapping.CanBeEvaluatedLocally);

            // apply mapping (binds LINQ operators too)
            expression = this.mapper.Translate(expression);

            // any policy specific translations or validations
            expression = this.police.Translate(expression);

            // any language specific translations or validations
            expression = this.linguist.Translate(expression);

            return expression;
        }
    }

     /// <summary>
    /// Defines the language rules for the query provider
    /// </summary>
    public abstract class QueryLanguage
    {
        public abstract QueryTypeSystem TypeSystem { get; }
        public abstract Expression GetGeneratedIdExpression(MemberInfo member);

        public virtual string Quote(string name)
        {
            return name;
        }

        public virtual bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public virtual bool AllowSubqueryInSelectWithoutFrom
        {
            get { return false; }
        }

        public virtual bool AllowDistinctInAggregates
        {
            get { return false; }
        }

        public virtual Expression GetRowsAffectedExpression(Expression command)
        {
            return new FunctionExpression(typeof(int), "@@ROWCOUNT", null);
        }

        public virtual bool IsRowsAffectedExpressions(Expression expression)
        {
            FunctionExpression fex = expression as FunctionExpression;
            return fex != null && fex.Name == "@@ROWCOUNT";
        }

        public virtual Expression GetOuterJoinTest(SelectExpression select)
        {
            // if the column is used in the join condition (equality test)
            // if it is null in the database then the join test won't match (null != null) so the row won't appear
            // we can safely use this existing column as our test to determine if the outer join produced a row

            // find a column that is used in equality test
            var aliases = DeclaredAliasGatherer.Gather(select.From);
            var joinColumns = JoinColumnGatherer.Gather(aliases, select).ToList();
            if (joinColumns.Count > 0)
            {
                // prefer one that is already in the projection list.
                foreach (var jc in joinColumns)
                {
                    foreach (var col in select.Columns)
                    {
                        if (jc.Equals(col.Expression))
                        {
                            return jc;
                        }
                    }
                }
                return joinColumns[0];
            }

            // fall back to introducing a constant
            return Expression.Constant(1, typeof(int?));
        }

        public virtual ProjectionExpression AddOuterJoinTest(ProjectionExpression proj)
        {
            var test = this.GetOuterJoinTest(proj.Select);
            var select = proj.Select;
            ColumnExpression testCol = null;
            // look to see if test expression exists in columns already
            foreach (var col in select.Columns)
            {
                if (test.Equals(col.Expression))
                {
                    var colType = this.TypeSystem.GetColumnType(test.Type);
                    testCol = new ColumnExpression(test.Type, colType, select.Alias, col.Name);
                    break;
                }
            }
            if (testCol == null)
            {
                // add expression to projection
                testCol = test as ColumnExpression;
                string colName = (testCol != null) ? testCol.Name : "Test";
                colName = proj.Select.Columns.GetAvailableColumnName(colName);
                var colType = this.TypeSystem.GetColumnType(test.Type);
                select = select.AddColumn(new ColumnDeclaration(colName, test, colType));
                testCol = new ColumnExpression(test.Type, colType, select.Alias, colName);
            }
            var newProjector = new OuterJoinedExpression(testCol, proj.Projector);
            return new ProjectionExpression(select, newProjector, proj.Aggregator);
        }

        class JoinColumnGatherer
        {
            HashSet<TableAlias> aliases;
            HashSet<ColumnExpression> columns = new HashSet<ColumnExpression>();

            private JoinColumnGatherer(HashSet<TableAlias> aliases)
            {
                this.aliases = aliases;
            }

            public static HashSet<ColumnExpression> Gather(HashSet<TableAlias> aliases, SelectExpression select)
            {
                var gatherer = new JoinColumnGatherer(aliases);
                gatherer.Gather(select.Where);
                return gatherer.columns;
            }

            private void Gather(Expression expression)
            {
                BinaryExpression b = expression as BinaryExpression;
                if (b != null)
                {
                    switch (b.NodeType)
                    {
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                            if (IsExternalColumn(b.Left) && GetColumn(b.Right) != null)
                            {
                                this.columns.Add(GetColumn(b.Right));
                            }
                            else if (IsExternalColumn(b.Right) && GetColumn(b.Left) != null)
                            {
                                this.columns.Add(GetColumn(b.Left));
                            }
                            break;
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            if (b.Type == typeof(bool) || b.Type == typeof(bool?))
                            {
                                this.Gather(b.Left);
                                this.Gather(b.Right);
                            }
                            break;
                    }
                }
            }

            private ColumnExpression GetColumn(Expression exp)
            {
                while (exp.NodeType == ExpressionType.Convert)
                    exp = ((UnaryExpression)exp).Operand;
                return exp as ColumnExpression;
            }

            private bool IsExternalColumn(Expression exp)
            {
                var col = GetColumn(exp);
                if (col != null && !this.aliases.Contains(col.Alias))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Determines whether the CLR type corresponds to a scalar data type in the query language
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool IsScalar(Type type)
        {
            type = TypeHelper.GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return false;
                case TypeCode.Object:
                    return
                        type == typeof(DateTimeOffset) ||
                        type == typeof(TimeSpan) ||
                        type == typeof(Guid) ||
                        type == typeof(byte[]);
                default:
                    return true;
            }
        }

        public virtual bool IsAggregate(MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
            {
                if (method.DeclaringType == typeof(Queryable)
                    || method.DeclaringType == typeof(Enumerable))
                {
                    switch (method.Name)
                    {
                        case "Count":
                        case "LongCount":
                        case "Sum":
                        case "Min":
                        case "Max":
                        case "Average":
                            return true;
                    }
                }
            }
            var property = member as PropertyInfo;
            if (property != null
                && property.Name == "Count"
                && typeof(IEnumerable).IsAssignableFrom(property.DeclaringType))
            {
                return true;
            }
            return false;
        }

        public virtual bool AggregateArgumentIsPredicate(string aggregateName)
        {
            return aggregateName == "Count" || aggregateName == "LongCount";
        }

        /// <summary>
        /// Determines whether the given expression can be represented as a column in a select expressionss
        /// </summary>
        public virtual bool CanBeColumn(Expression expression)
        {
            return this.MustBeColumn(expression) || this.IsScalar(expression.Type);
        }

        /// <summary>
        /// Determines whether the given expression must be represented as a column in a SELECT column list
        /// </summary>
        public virtual bool MustBeColumn(Expression expression)
        {
            switch (expression.NodeType)
            {
                case (ExpressionType)DbExpressionType.Column:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.AggregateSubquery:
                case (ExpressionType)DbExpressionType.Aggregate:
                    return true;
                default:
                    return false;
            }
        }

        public virtual QueryLinguist CreateLinguist(QueryTranslator translator)
        {
            return new QueryLinguist(this, translator);
        }
    }

    public class QueryLinguist
    {
        QueryLanguage language;
        QueryTranslator translator;

        public QueryLinguist(QueryLanguage language, QueryTranslator translator)
        {
            this.language = language;
            this.translator = translator;
        }

        public QueryLanguage Language 
        {
            get { return this.language; }
        }

        public QueryTranslator Translator
        {
            get { return this.translator; }
        }

        /// <summary>
        /// Provides language specific query translation.  Use this to apply language specific rewrites or
        /// to make assertions/validations about the query.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            // remove redundant layers again before cross apply rewrite
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantColumnRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);

            // convert cross-apply and outer-apply joins into inner & left-outer-joins if possible
            var rewritten = CrossApplyRewriter.Rewrite(this.language, expression);

            // convert cross joins into inner joins
            rewritten = CrossJoinRewriter.Rewrite(rewritten);

            if (rewritten != expression)
            {
                expression = rewritten;
                // do final reduction
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
                expression = RedundantColumnRemover.Remove(expression);
            }

            return expression;
        }

        /// <summary>
        /// Converts the query expression into text of this query language
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual string Format(Expression expression)
        {
            // use common SQL formatter by default
            return SqlFormatter.Format(expression);
        }

        /// <summary>
        /// Determine which sub-expressions must be parameters
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Parameterize(Expression expression)
        {
            return Parameterizer.Parameterize(this.language, expression);
        }
    }

    /// <summary>
    /// Defines query execution and materialization policies. 
    /// </summary>
    public class QueryPolicy
    {
        public QueryPolicy()
        {
        }

        /// <summary>
        /// Determines if a relationship property is to be included in the results of the query
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsIncluded(MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Determines if a relationship property is included, but the query for the related data is 
        /// deferred until the property is first accessed.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsDeferLoaded(MemberInfo member)
        {
            return false;
        }

        public virtual QueryPolice CreatePolice(QueryTranslator translator)
        {
            return new QueryPolice(this, translator);
        }

        public static readonly QueryPolicy Default = new QueryPolicy();
    }

    public class QueryPolice
    {
        QueryPolicy policy;
        QueryTranslator translator;

        public QueryPolice(QueryPolicy policy, QueryTranslator translator)
        {
            this.policy = policy;
            this.translator = translator;
        }

        public QueryPolicy Policy
        {
            get { return this.policy; }
        }

        public QueryTranslator Translator
        {
            get { return this.translator; }
        }

        public virtual Expression ApplyPolicy(Expression expression, MemberInfo member)
        {
            return expression;
        }

        /// <summary>
        /// Provides policy specific query translations.  This is where choices about inclusion of related objects and how
        /// heirarchies are materialized affect the definition of the queries.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            // add included relationships to client projection
            var rewritten = RelationshipIncluder.Include(this.translator.Mapper, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            // convert any singleton (1:1 or n:1) projections into server-side joins (cardinality is preserved)
            rewritten = SingletonProjectionRewriter.Rewrite(this.translator.Linguist.Language, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            // convert projections into client-side joins
            rewritten = ClientJoinedProjectionRewriter.Rewrite(this.policy, this.translator.Linguist.Language, expression);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            return expression;
        }

        /// <summary>
        /// Converts a query into an execution plan.  The plan is an function that executes the query and builds the
        /// resulting objects.
        /// </summary>
        public virtual Expression BuildExecutionPlan(Expression query, Expression provider)
        {
            return ExecutionBuilder.Build(this.translator.Linguist, this.policy, query, provider);
        }
    }

    public abstract class QueryType
    {
        public abstract bool NotNull { get; }
        public abstract int Length { get; }
        public abstract short Precision { get; }
        public abstract short Scale { get; }
    }

    public abstract class QueryTypeSystem
    {
        public abstract QueryType Parse(string typeDeclaration);
        public abstract QueryType GetColumnType(Type type);
        public abstract string GetVariableDeclaration(QueryType type, bool suppressSize);
    }
}

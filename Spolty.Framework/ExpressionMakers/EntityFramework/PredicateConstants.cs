namespace Spolty.Framework.ExpressionMakers.EntityFramework
{
    public static class PredicateConstants
    {
        public const string FieldEqualValueFormat = "{0}.{1} = @{2}";
        public const string FieldLessThanValueFormat = "{0}.{1} < @{2}";
        public const string FieldLessThanEqualValueFormat = "{0}.{1} <= @{2}";
        public const string FieldGreateThanEqualValueFormat = "{0}.{1} >= @{2}";
        public const string FieldGreateThanValueFormat = "{0}.{1} > @{2}";
        public const string FieldLikeValueFormat = "{0}.{1} LIKE @{2}";
        public const string WhereFormat = " WHERE ({0})";
     
        public const string AND = " And ";
        public const string OR = " Or ";
        public const string PerCentSign = "%";
    }
}
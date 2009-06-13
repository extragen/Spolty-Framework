using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins.Enums;

namespace Spolty.Framework.Parameters.Joins
{
    /// <summary>
    /// 
    /// </summary>
    public class JoinNode : BaseNode.BaseNode
    {
        private readonly ConditionList _conditions = new ConditionList();
        private readonly List<string> _currentFieldsNames;
        private readonly bool _isTypeNameEqualAssociatedPropertyName;
        private readonly List<string> _parentFieldsNames;
        private readonly string _associatedPropertyName;
        private JoinType _joinWithParentBy;
        private readonly string _parentPropertyName;

        /// <summary>
        /// Creates <see cref="JoinNode"/> object by entityType. Use only for Linq To Sql.
        /// If it child <see cref="JoinNode"/> then it'll be joined by association which defined in <see cref="MetaModel"/>.
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        public JoinNode(Type entityType)
            : this(entityType, String.Empty, entityType.Name)
        {
        }

        /// <summary>
        /// Creates child <see cref="JoinNode"/> object by entityType and defines field names which will be use for comparison in inner join.
        /// <see cref="JoinWithParentBy"/> by default is <see cref="JoinType.InnerJoin"/>.
        /// </summary>
        /// <param name="entityType">Type of Entity.</param>
        /// <param name="parentFieldsNames">fields which defined in the parent Entity. </param>
        /// <param name="currentFieldsNames">fields which defined for the current Entity.</param>
        public JoinNode(Type entityType, IEnumerable<string> parentFieldsNames,
                        IEnumerable<string> currentFieldsNames)
            : this(entityType, String.Empty, entityType.Name)
        {
            if (parentFieldsNames == null)
            {
                throw new SpoltyException("parentFieldsNames not defined");
            }

            if (currentFieldsNames == null)
            {
                throw new SpoltyException("currentFieldsNames not defined");
            }

            _parentFieldsNames.AddRange(parentFieldsNames);
            _currentFieldsNames.AddRange(currentFieldsNames);

            if (_parentFieldsNames.Count != _currentFieldsNames.Count)
            {
                throw new SpoltyException("Number of fields mismatch in parentFieldsNames and currentFieldsNames");
            }

            _isTypeNameEqualAssociatedPropertyName = false;

            _parentFieldsNames.TrimExcess();
            _currentFieldsNames.TrimExcess();
        }

        /// <summary>
        /// Creates child <see cref="JoinNode"/> object by entityType and defines field names which will be use for comparison in join.
        /// </summary>
        /// <param name="entityType">Type of Entity.</param>
        /// <param name="parentFieldsNames">fields which defined in the parent Entity. </param>
        /// <param name="currentFieldsNames">fields which defined for the current Entity.</param>
        /// <param name="joinWithParentBy">defines join between two Entity queries.</param>
        public JoinNode(Type entityType, IEnumerable<string> parentFieldsNames, IEnumerable<string> currentFieldsNames, JoinType joinWithParentBy)
            : this(entityType, String.Empty, entityType.Name, joinWithParentBy)
        {
            Checker.CheckArgumentNull(parentFieldsNames, "parentFieldsNames");
            Checker.CheckArgumentNull(currentFieldsNames, "currentFieldsNames");

            _parentFieldsNames.AddRange(parentFieldsNames);
            _currentFieldsNames.AddRange(currentFieldsNames);

            if (_parentFieldsNames.Count != _currentFieldsNames.Count)
            {
                throw new SpoltyException("Number of fields mismatch in parentFieldsNames and currentFieldsNames");
            }

            _isTypeNameEqualAssociatedPropertyName = _parentFieldsNames.Count == 0;

            _parentFieldsNames.TrimExcess();
            _currentFieldsNames.TrimExcess();
        }

//        /// <summary>
//        /// Creates child <see cref="JoinNode"/> object by entityType and defines join between Entity query.
//        /// </summary>
//        /// <param name="entityType">Type of Entity.</param>
//        /// <param name="joinWithParentBy">defines join between two Entity queries.</param>
//        public JoinNode(Type entityType, JoinType joinWithParentBy)
//            : this(entityType, entityType.Name, joinWithParentBy)
//        {
//        }

        /// <summary>
        /// Creates child <see cref="JoinNode"/> object by entityType and defined in Entity <paramref name="associatedPropertyName"/> 
        /// which associated with parent Entity.
        /// <see cref="JoinWithParentBy"/> by default is <see cref="JoinType.InnerJoin"/>.
        /// </summary>
        /// <param name="entityType">Type of Entity.</param>
        /// <param name="associatedPropertyName">associated with parent property.</param>
        public JoinNode(Type entityType, string parentPropertyName, string associatedPropertyName) : base(entityType)
        {
            Checker.CheckArgumentNull(entityType, "entityType");
            Checker.CheckArgumentNull(associatedPropertyName, "associatedPropertyName");

            _parentPropertyName = parentPropertyName;
            _associatedPropertyName = associatedPropertyName;
            _isTypeNameEqualAssociatedPropertyName = _associatedPropertyName == entityType.Name;
            _childNodes = new JoinNodeList(this);
            _parentFieldsNames = new List<string>(0);
            _currentFieldsNames = new List<string>(0);
        }

        /// <summary>
        /// Creates child <see cref="JoinNode"/> object by entityType and defined in Entity <paramref name="associatedPropertyName"/> 
        /// which associated with parent Entity.
        /// </summary>
        /// <param name="entityType">Type of Entity.</param>
        /// <param name="parentPropertyName"></param>
        /// <param name="associatedPropertyName">associated with parent property.</param>
        /// <param name="joinWithParentBy">defines join between two Entity queries.</param>
        public JoinNode(Type entityType, string parentPropertyName, string associatedPropertyName, JoinType joinWithParentBy)
            : this(entityType, parentPropertyName, associatedPropertyName)
        {
            _joinWithParentBy = joinWithParentBy;
        }

        internal bool IsTypeNameEqualAssociatedPropertyName
        {
            get { return _isTypeNameEqualAssociatedPropertyName; }
        }

        /// <summary>
        /// Gets type of join between current <see cref="JoinNode"/> and parent <see cref="JoinNode"/>.
        /// </summary>
        public JoinType JoinWithParentBy
        {
            get { return _joinWithParentBy; }
            internal set
            {
                switch (value)
                {
                    case JoinType.InnerJoin:
                        if (ParentNode != null && ((JoinNode) ParentNode).JoinWithParentBy != JoinType.LeftOuterJoin)
                        {
                            _joinWithParentBy = value;
                        }
                        else
                        {
                            _joinWithParentBy = JoinType.LeftOuterJoin;
                        }
                        break;
                    case JoinType.LeftOuterJoin:
                        _joinWithParentBy = JoinType.LeftOuterJoin;
                        break;
                    default:
                        break;
                }
                _joinWithParentBy = value;
            }
        }

        /// <summary>
        /// Gets property name associated with parent property.
        /// </summary>
        public string AssociatedPropertyName
        {
            get { return _associatedPropertyName; }
        }

        public string ParentPropertyName
        {
            get { return _parentPropertyName; }
        }

        /// <summary>
        /// Gets field names which defined in parent Entity. 
        /// </summary>
        public ReadOnlyCollection<string> ParentFieldsNames
        {
            get { return _parentFieldsNames.AsReadOnly(); }
        }

        /// <summary>
        /// Gets field name which defined in current Entity.
        /// </summary>
        public ReadOnlyCollection<string> CurrentFieldsNames
        {
            get { return _currentFieldsNames.AsReadOnly(); }
        }
        
        /// <summary>
        /// Gets conditions for current <see cref="JoinNode"/>.
        /// </summary>
        public ConditionList Conditions
        {
            get { return _conditions; }
        }

        /// <summary>
        /// Adds conditions for current <see cref="JoinNode"/>.
        /// </summary>
        /// <param name="conditions"></param>
        public void AddConditions(params BaseCondition[] conditions)
        {
            AddConditions((IEnumerable<BaseCondition>) conditions);
        }

        /// <summary>
        /// Adds conditions for current <see cref="JoinNode"/>.
        /// </summary>
        /// <param name="conditions"></param>
        public void AddConditions(IEnumerable<BaseCondition> conditions)
        {
            Checker.CheckArgumentNull(conditions, "conditions");
            Conditions.AddConditions(conditions);
            Conditions.SetElementType(EntityType);
            Conditions.RemoveDuplicates();
        }
        
        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result ^= _associatedPropertyName.GetHashCode();
            result ^= _isTypeNameEqualAssociatedPropertyName.GetHashCode();
            result ^= _currentFieldsNames.GetHashCode();
            result ^= _parentFieldsNames.GetHashCode();
            result ^= _joinWithParentBy.GetHashCode();
            return result;
        }

        #region IComparer<JoinNode> Members

        ///<summary>
        ///Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        ///</summary>
        ///<returns>
        ///Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
        ///</returns>
        ///<param name="y">The second object to compare.</param>
        ///<param name="x">The first object to compare.</param>
        public override int Compare(BaseNode.BaseNode x, BaseNode.BaseNode y)
        {
            const int hash = 100000000;
            int xHash = x.EntityType.GetHashCode();
            xHash += ((JoinNode) x).JoinWithParentBy == JoinType.InnerJoin ? 0 : hash;

            int yHash = y.EntityType.GetHashCode();
            yHash += ((JoinNode) y).JoinWithParentBy == JoinType.InnerJoin ? 0 : hash;
            return xHash - yHash;
        }

        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters.Aggregations;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins.Enums;

namespace Spolty.Framework.Parameters.Joins
{
    public class JoinNode : BaseNode.BaseNode, IParameterMarker
    {
        private readonly List<AggregateMethod> _conditionAggregateMethod;
        private readonly ConditionList _conditions = new ConditionList();
        private readonly List<string> _currentFieldsNames;
        private readonly bool _isTypeNameEqualPropertyName;
        private readonly List<string> _parentFieldsNames;
        private readonly string _propertyName;
        private JoinType _joinParentType;

        /// <summary>
        /// Creates JoinNode object by entityType
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        public JoinNode(Type entityType)
            : this(entityType, entityType.Name)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parentFieldsNames"></param>
        /// <param name="currentFieldsNames"></param>
        public JoinNode(Type entityType, IEnumerable<string> parentFieldsNames,
                        IEnumerable<string> currentFieldsNames)
            : this(entityType, entityType.Name)
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

            _isTypeNameEqualPropertyName = false;

            _parentFieldsNames.TrimExcess();
            _currentFieldsNames.TrimExcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="joinParentType"></param>
        /// <param name="parentFieldsNames"></param>
        /// <param name="currentFieldsNames"></param>
        public JoinNode(Type entityType, JoinType joinParentType, IEnumerable<string> parentFieldsNames,
                        IEnumerable<string> currentFieldsNames)
            : this(entityType, entityType.Name, joinParentType)
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

            _isTypeNameEqualPropertyName = _parentFieldsNames.Count == 0;

            _parentFieldsNames.TrimExcess();
            _currentFieldsNames.TrimExcess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="joinParentType"></param>
        public JoinNode(Type entityType, JoinType joinParentType)
            : this(entityType, entityType.Name, joinParentType)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        public JoinNode(Type entityType, string propertyName) : base(entityType)
        {
            if (entityType == null)
            {
                throw new SpoltyException("entityType is undefined");
            }

            if (String.IsNullOrEmpty(propertyName))
            {
                throw new SpoltyException("propertyName is undefined");
            }

            _propertyName = propertyName;
            _isTypeNameEqualPropertyName = _propertyName == entityType.Name;
            _childNodes = new JoinNodeList(this);
            _conditionAggregateMethod = new List<AggregateMethod>();
            _parentFieldsNames = new List<string>(0);
            _currentFieldsNames = new List<string>(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        /// <param name="joinParentType"></param>
        public JoinNode(Type entityType, string propertyName, JoinType joinParentType)
            : this(entityType, propertyName)
        {
            _joinParentType = joinParentType;
        }

        /// <summary>
        /// 
        /// </summary>
        public IExpressionMakerFactory Factory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JoinType JoinParentType
        {
            get { return _joinParentType; }
            internal set
            {
                switch (value)
                {
                    case JoinType.InnerJoin:
                        if (ParentNode != null && ((JoinNode) ParentNode).JoinParentType != JoinType.LeftJoin)
                        {
                            _joinParentType = value;
                        }
                        else
                        {
                            _joinParentType = JoinType.LeftJoin;
                        }
                        break;
                    case JoinType.LeftJoin:
                        _joinParentType = JoinType.LeftJoin;
                        break;
                    default:
                        break;
                }
                _joinParentType = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTypeNameEqualPropertyName
        {
            get { return _isTypeNameEqualPropertyName; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyCollection<AggregateMethod> ConditionAggregateMethod
        {
            get { return _conditionAggregateMethod.AsReadOnly(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public AggregateMethod SelectorAggregateMethod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyCollection<string> ParentFieldsNames
        {
            get { return _parentFieldsNames.AsReadOnly(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyCollection<string> CurrentFieldsNames
        {
            get { return _currentFieldsNames.AsReadOnly(); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public ConditionList Conditions
        {
            get { return _conditions; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="aggregateMethod"></param>
        public void SetAggregateMethod(Type entityType, AggregateMethod aggregateMethod)
        {
            var node = (JoinNode) FindInChildren(entityType, true);
            if (node == null)
            {
                throw new SpoltyException("Node is not found");
            }

            if (node._conditionAggregateMethod.IndexOf(aggregateMethod) < 0)
            {
                node._conditionAggregateMethod.Add(aggregateMethod);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditions"></param>
        public void AddConditions(params BaseCondition[] conditions)
        {
            AddConditions((IEnumerable<BaseCondition>) conditions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditions"></param>
        public void AddConditions(IEnumerable<BaseCondition> conditions)
        {
            Checker.CheckArgumentNull(conditions, "conditions");
            Conditions.AddConditions(conditions);
            Conditions.SetElementType(EntityType);
            Conditions.RemoveDuplicates();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var val2 = obj as JoinNode;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return EntityType == val2.EntityType && _joinParentType == val2._joinParentType;
                }
            }
            return false;
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
            result ^= _joinParentType.GetHashCode();
            result ^= _conditionAggregateMethod.GetHashCode();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator ==(JoinNode val1, JoinNode val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator !=(JoinNode val1, JoinNode val2)
        {
            return !(val1 == val2);
        }
    }
}
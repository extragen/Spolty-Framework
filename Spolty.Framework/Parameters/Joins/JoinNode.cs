using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Spolty.Framework.Exceptions;
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

        public JoinNode(Type bizObjectType)
            : this(bizObjectType, bizObjectType.Name)
        {
        }

        public JoinNode(Type bizObjectType, IEnumerable<string> parentFieldsNames,
                        IEnumerable<string> currentFieldsNames)
            : this(bizObjectType, bizObjectType.Name)
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

        public JoinNode(Type bizObjectType, JoinType joinParentType, IEnumerable<string> parentFieldsNames,
                        IEnumerable<string> currentFieldsNames)
            : this(bizObjectType, bizObjectType.Name, joinParentType)
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

        public JoinNode(Type bizObjectType, JoinType joinParentType)
            : this(bizObjectType, bizObjectType.Name, joinParentType)
        {
        }

        public JoinNode(Type bizObjectType, string propertyName) : base(bizObjectType)
        {
            if (bizObjectType == null)
            {
                throw new SpoltyException("bizObjectType is undefined");
            }

            if (String.IsNullOrEmpty(propertyName))
            {
                throw new SpoltyException("propertyName is undefined");
            }

            _propertyName = propertyName;
            _isTypeNameEqualPropertyName = _propertyName == bizObjectType.Name;
            _childNodes = new JoinNodeList(this);
            _conditionAggregateMethod = new List<AggregateMethod>();
            _parentFieldsNames = new List<string>(0);
            _currentFieldsNames = new List<string>(0);
        }

        public JoinNode(Type bizObjectType, string propertyName, JoinType joinParentType)
            : this(bizObjectType, propertyName)
        {
            _joinParentType = joinParentType;
        }

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

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public bool IsTypeNameEqualPropertyName
        {
            get { return _isTypeNameEqualPropertyName; }
        }

        public ReadOnlyCollection<AggregateMethod> ConditionAggregateMethod
        {
            get { return _conditionAggregateMethod.AsReadOnly(); }
        }

        public AggregateMethod SelectorAggregateMethod { get; set; }

        public ReadOnlyCollection<string> ParentFieldsNames
        {
            get { return _parentFieldsNames.AsReadOnly(); }
        }

        public ReadOnlyCollection<string> CurrentFieldsNames
        {
            get { return _currentFieldsNames.AsReadOnly(); }
        }

        public ConditionList Conditions
        {
            get { return _conditions; }
        }

        public void SetAggregateMethod(Type bizObjectType, AggregateMethod aggregateMethod)
        {
            var node = (JoinNode) FindInChildren(bizObjectType, true);
            if (node == null)
            {
                throw new SpoltyException("Node is not found");
            }

            if (node._conditionAggregateMethod.IndexOf(aggregateMethod) < 0)
            {
                node._conditionAggregateMethod.Add(aggregateMethod);
            }
        }

        public void AddConditions(params BaseCondition[] conditions)
        {
            AddConditions((IEnumerable<BaseCondition>) conditions);
        }

        public void AddConditions(IEnumerable<BaseCondition> conditions)
        {
            Conditions.AddConditions(conditions);
            Conditions.SetElementType(BizObjectType);
        }

        public override bool Equals(object obj)
        {
            var val2 = obj as JoinNode;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return BizObjectType == val2.BizObjectType && _joinParentType == val2._joinParentType;
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

        public static bool operator ==(JoinNode val1, JoinNode val2)
        {
            return Equals(val1, val2);
        }

        public static bool operator !=(JoinNode val1, JoinNode val2)
        {
            return !(val1 == val2);
        }
    }
}